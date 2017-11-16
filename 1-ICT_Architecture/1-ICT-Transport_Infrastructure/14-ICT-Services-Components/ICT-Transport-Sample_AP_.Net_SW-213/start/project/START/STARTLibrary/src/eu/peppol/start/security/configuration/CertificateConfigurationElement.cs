/*Version: MPL 1.1/EUPL 1.1
 * 
 * The contents of this file are subject to the Mozilla Public License Version 
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at:
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 * 
 * The Original Code is Copyright The PEPPOL project (http://www.peppol.eu)
 * 
 * Alternatively, the contents of this file may be used under the
 * terms of the EUPL, Version 1.1 or - as soon they will be approved 
 * by the European Commission - subsequent versions of the EUPL 
 * (the "Licence"); You may not use this work except in compliance 
 * with the Licence.
 * You may obtain a copy of the Licence at:
 * http://www.osor.eu/eupl/european-union-public-licence-eupl-v.1.1
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the Licence is distributed on an "AS IS" basis,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the Licence for the specific language governing permissions and
 * limitations under the Licence.
 * 
 * If you wish to allow use of your version of this file only
 * under the terms of the EUPL License and not to allow others to use
 * your version of this file under the MPL, indicate your decision by
 * deleting the provisions above and replace them with the notice and
 * other provisions required by the EUPL License. If you do not delete
 * thev provisions above, a recipient may use your version of this file
 * under either the MPL or the EUPL License.
 */

using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace STARTLibrary.src.eu.peppol.start.security.configuration
{
    /// <summary>
    /// Represents one "serviceCertificate" or "clientCertificate" element in the configuration. 
    /// </summary>
    public class CertificateConfigurationElement : ConfigurationElement
    {
        #region Properties
        [ConfigurationProperty("filename", IsRequired = false)]
        public string Filename
        {
            get { return (string)this["filename"]; }
            set { this["filename"] = value; }
        }
        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }
        [ConfigurationProperty("findValue", IsRequired = false)]
        public string FindValue
        {
            get { return (string)this["findValue"]; }
            set { this["findValue"] = value; }
        }
        [ConfigurationProperty("storeLocation", IsRequired = false)]
        public string StoreLocation
        {
            get { return (string)this["storeLocation"]; }
            set { this["storeLocation"] = value; }
        }
        [ConfigurationProperty("storeName", IsRequired = false)]
        public string StoreName
        {
            get { return (string)this["storeName"]; }
            set { this["storeName"] = value; }
        }
        [ConfigurationProperty("x509FindType", IsRequired = false)]
        public string X509FindType
        {
            get { return (string)this["x509FindType"]; }
            set { this["x509FindType"] = value; }
        }

        [ConfigurationProperty("encoded", IsRequired = false)]
        public string Encoded
        {
            get { return (string)this["encoded"]; }
            set { this["encoded"] = value; }
        }
        #endregion

        #region Certificate
        public X509Certificate2 Certificate
        {
            get
            {
                // The certificate can be left empty, but we should warn if the user made an error
                // when specifying the certicate:
                if (HasIncompleteData())
                {
                    throw new InvalidOperationException("Needs either filename and password or findValue, storeLocation, storeName, and x509FindType.");
                }

                if (HasFilenameAndPassword())
                {
                    return CertificateReader.FromFile(Filename, Password);
                }

                if (HasStoreAndFindInfo())
                {
                    return CertificateReader.FromStore(StoreName, StoreLocation, X509FindType, FindValue);
                }

                if (HasEncodedCertificate())
                {
                    return CertificateReader.FromEncodedCertificate(Encoded);
                }

                return null;
            }
        }

        private bool HasStoreAndFindInfo()
        {
            return
                !String.IsNullOrEmpty(FindValue) &&
                !String.IsNullOrEmpty(StoreName) &&
                !String.IsNullOrEmpty(StoreLocation) &&
                !String.IsNullOrEmpty(X509FindType);
        }

        /// <returns>
        /// True if either filename and password are specified OR if a .crt/.cer 
        /// filename is specified (which does not require a password to load).
        /// </returns>
        private bool HasFilenameAndPassword()
        {
            return
                !String.IsNullOrEmpty(Filename) &&
               ((IsPfxFile(Filename) && !String.IsNullOrEmpty(Password)) || // .pfx files include the private key and require a password
                 IsCertFile(Filename));                                     // .crt/.cer files only contain the (service) certificate
        }

        private bool HasEncodedCertificate()
        {
            return !String.IsNullOrEmpty(Encoded);
        }

        private static bool IsCertFile(string filename)
        {
            return Path.GetExtension(filename).Equals(".crt", StringComparison.InvariantCultureIgnoreCase) ||
                   Path.GetExtension(filename).Equals(".cer", StringComparison.InvariantCultureIgnoreCase);
        }
        private static bool IsPfxFile(string filename)
        {
            return Path.GetExtension(filename).Equals(".pfx", StringComparison.InvariantCultureIgnoreCase);
        }

        private bool HasIncompleteData()
        {
            return
                (!String.IsNullOrEmpty(Filename) && !HasFilenameAndPassword()) ||

                ((!String.IsNullOrEmpty(FindValue) ||
                     !String.IsNullOrEmpty(StoreName) ||
                     !String.IsNullOrEmpty(StoreLocation) ||
                     !String.IsNullOrEmpty(X509FindType)) && !HasStoreAndFindInfo());
        }
        #endregion
    }

    /// <summary>
    /// Helper class to read certificates from file and from the certificate store.
    /// </summary>
    public class CertificateReader
    {
        public static X509Certificate2 FromFile(string filename, string password)
        {
            string filePath = GetAbsolutePath(filename);

            if (!File.Exists(filePath))
                throw new ArgumentException(String.Format("The file \"{0}\" does not exist.", filePath));

            return new X509Certificate2(filePath, password, X509KeyStorageFlags.MachineKeySet);
        }

        private static string GetAbsolutePath(string filename)
        {
            if (Path.IsPathRooted(filename))
                return filename;

            string directory = !String.IsNullOrEmpty(AppDomain.CurrentDomain.RelativeSearchPath) ?
                AppDomain.CurrentDomain.RelativeSearchPath :
                Environment.CurrentDirectory;

            return Path.Combine(directory, filename);
        }

        public static X509Certificate2 FromStore(string storeName, string storeLocation, string findType, string findValue)
        {
            if (!Enum.IsDefined(typeof(StoreName), storeName))
                throw new ArgumentException(String.Format("Cannot parse storeName value \"{0}\".", storeName));
            if (!Enum.IsDefined(typeof(StoreLocation), storeLocation))
                throw new ArgumentException(String.Format("Cannot parse storeLocation value \"{0}\".", storeLocation));
            if (!Enum.IsDefined(typeof(X509FindType), findType))
                throw new ArgumentException(String.Format("Cannot parse x509FindType value \"{0}\".", findType));

            return FromStore(
                Parse<StoreName>(storeName),
                Parse<StoreLocation>(storeLocation),
                Parse<X509FindType>(findType),
                findValue);
        }

        public static X509Certificate2 FromStore(StoreName storeName, StoreLocation storeLocation, X509FindType findType, string findValue)
        {
            X509Store store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                var results = store.Certificates.Find(findType, findValue, true);

                if (results.Count == 0)
                    throw new ArgumentException(String.Format(
                        "No valid certificate with the findValue \"{0}\" found (searched store \"{1}\" in location \"{2}\".",
                        findValue, storeName, storeLocation));

                if (results.Count > 1)
                    throw new ArgumentException(String.Format(
                        "Found more than one certificate with the findValue \"{0}\".", findValue));

                return results[0];

            }
            finally
            {
                store.Close();
            }
        }

        private static T Parse<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }
        
        internal static X509Certificate2 FromEncodedCertificate(string encodedCertificate)
        {
            try
            {
                return new X509Certificate2(Convert.FromBase64String(encodedCertificate));
            }
            catch (System.Security.Cryptography.CryptographicException) { throw NewDecodeErrorException(encodedCertificate); }
            catch (FormatException) { throw NewDecodeErrorException(encodedCertificate); }
        }

        private static ArgumentException NewDecodeErrorException(string encodedCertificate)
        {
            if (encodedCertificate.Length > 20)
                encodedCertificate = encodedCertificate.Substring(0, 20) + "...";

            return new ArgumentException(
                String.Format("Cannot read certificate from encoded value \"{0}\".", encodedCertificate),
                "encodedCertificate");
        }

    }
}
