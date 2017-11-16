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
using System.ComponentModel;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Tokens;
using STARTLibrary.src.eu.peppol.start.security;
using STARTLibrary.src.eu.peppol.start.security.configuration;

namespace STARTLibrary.src.eu.peppol.start.security.handler
{
    /// <summary>
    /// CertificateValidator which can be configured to expect either the AP CA
    /// or the SMP CA as issuer.
    /// </summary>
    public class CertificateValidator : CertificateValidatorBase
    {
        /// <summary>
        /// The expected issuer of the certificate being validated.
        /// </summary>
        /// <remarks>
        /// There are two intermediate CAs, one for AP certificates and another 
        /// one for SMP certificates.
        /// If this property is set, the validator does not only check that the
        /// certificate is valid, but also checks that the certificate of the
        /// intermediate CA which issued the certificate being validated is one 
        /// of the PEPPOL CAs.
        /// </remarks>
        public CertificateIssuer ExpectedIssuer
        {
            get { return _expectedIssuer; }
            set { _expectedIssuer = value; }
        }
    }

    /// <summary>
    /// CertificateValidator with pre-defined ExpectedIssuer = AccessPointCA.
    /// </summary>
    /// <remarks>
    /// Can be used in the web.config to define a certificateValidator in
    /// the microsoft.identityModel section.
    /// </remarks>
    public class AccessPointCertificateValidator : CertificateValidatorBase
    {
        public AccessPointCertificateValidator()
            : base()
        {
            _expectedIssuer = CertificateIssuer.AccessPointCA;
        }
    }

    public class CertificateValidatorBase : X509CertificateValidator
    {
        private CertificateValidationThumbprints _thumbprints;
        private X509RevocationMode _revocationMode = X509RevocationMode.Online;
        protected CertificateIssuer _expectedIssuer = CertificateIssuer.AccessPointCA;

        /// <summary>
        /// The thumbprints of certificates for the Root CA and the intermediate CAs.
        /// Required to identify the root certificate and (if ExpectedIssuer is set) 
        /// to verify the issuer of the certificate being validated.
        /// </summary>
        public CertificateValidationThumbprints Thumbprints
        {
            get
            {
                // If not set explicitly from the outside, try to use the default configuration:
                if (_thumbprints == null)
                {
                    _thumbprints = CertificateValidationThumbprints.FromConfiguration();
                }
                return _thumbprints;
            }
            set
            {
                _thumbprints = value;
            }

        }

        /// <summary>
        /// The expected certificate. 
        /// </summary>
        /// <remarks>
        /// Used to check that the certificate in a response matches the metadata
        /// received from the Service Metadata Publisher.
        /// If null, this check is skipped.
        /// </remarks>
        public X509Certificate2 ExpectedCertificate { get; set; }

        /// <summary>
        /// Allows to define additional certificates that are considered trusted root
        /// certificates (in addition to those placed in the certificate store).
        /// </summary>
        /// <remarks>
        /// Useful for integration tests when it should be possible to run the tests 
        /// without first manipulating the certificate store of the local machine.
        /// </remarks>
        public X509Certificate2Collection ExtraTrustedRootCertificates { get; set; }

        /// <summary>
        /// Allows to define additional certificates that are considered trusted 
        /// certificates (in addition to those placed in the certificate store).
        /// </summary>
        /// <remarks>
        /// Useful for integration tests when it should be possible to run the tests 
        /// without first manipulating the certificate store of the local machine.
        /// </remarks>
        public X509Certificate2Collection ExtraTrustedIntermediateCertificates { get; set; }

        /// <summary>
        /// Specifies how the validator should check for X509 certificate revocation.
        /// Default is RevocationMode.Online.
        /// </summary>
        public X509RevocationMode RevocationMode
        {
            get { return _revocationMode; }
            set { _revocationMode = value; }
        }

        /// <summary>
        /// Allows to define an additional certificate that is considered trusted root
        /// certificate (in addition to those placed in the certificate store).
        /// </summary>
        public X509Certificate2 ExtraTrustedRootCertificate
        {
            set
            {
                ExtraTrustedRootCertificates = new X509Certificate2Collection() { value };
            }
        }

        public override void Validate(X509Certificate2 certificate)
        {
            if (certificate == null)
                throw new SecurityTokenValidationException("Validation failed. No certificate to validate.");

            // 1) Validate that the certificate is either the PEPPOL root certificate or
            //    is issued by the correct CA 
            ValidateAsPEPPOLCertificate(certificate);

            // 2) Validate that the AP's certificate is the same as stated in the SMP
            ValidateAgainstExpectedCertificate(certificate);

            // 3) Possible custom validation
            Validator(certificate);
        }

        private void ValidateAsPEPPOLCertificate(X509Certificate2 certificate)
        {
            X509Chain chain = new X509Chain()
            {
                ChainPolicy = new X509ChainPolicy()
                {
                    RevocationMode = this.RevocationMode,
                    RevocationFlag = X509RevocationFlag.EndCertificateOnly
                }
            };
            if (ExtraTrustedRootCertificates != null)
                chain.ChainPolicy.ExtraStore.AddRange(ExtraTrustedRootCertificates);
            if (ExtraTrustedIntermediateCertificates != null)
                chain.ChainPolicy.ExtraStore.AddRange(ExtraTrustedIntermediateCertificates);

            //Validate chain errors or if it is revoked
            if (!BuildChain(chain, certificate))
            {
                throw new SecurityTokenValidationException("Validation failed. Chain could not be built.");
            }

            // Skip the check for the expected issuer if we are dealing with 
            // a certificate from the Root CA or an Intermediate CA:
            if (IsPeppolRoot(certificate) || IsPeppolIntermediateCA(certificate))
            {
                return;
            }

            if (!IsExpectedIssuer(chain))
            {
                throw new SecurityTokenValidationException("Validation failed. Issued by the wrong CA.");
            }
        }

        private bool BuildChain(X509Chain chain, X509Certificate2 certificate)
        {
            try
            {
                if (chain.Build(certificate) && chain.HasNoError())
                {
                    return true;
                }
                return false;
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return false;
            }
        }

        private bool IsExpectedIssuer(X509Chain chain)
        {
            if (_expectedIssuer == CertificateIssuer.None)
            {
                return true;
            }

            if (IssuerThumbprints == null)
            {
                throw new SecurityTokenValidationException("Validation failed. No intermediate CA certificate thumbprint(s) defined to check against.");
            }

            var issuer = chain.Issuer();

            return IssuerThumbprints.Contains(issuer.Certificate.Thumbprint, thumbprintComparer);
        }

        private bool IsPeppolRoot(X509Certificate2 certificate)
        {
            if (RootThumbprints == null)
            {
                throw new SecurityTokenValidationException("Validation failed. No Root CA certificate thumbprint(s) defined to check against.");
            }

            X509Chain chain = new X509Chain();
            chain.Build(certificate);

            for (var i = 1; i < chain.ChainElements.Count; i++)
            {
                if (RootThumbprints.Contains(chain.ChainElements[i].Certificate.Thumbprint, thumbprintComparer))
                    return true;
            }
            return false;
        }

        private bool IsPeppolIntermediateCA(X509Certificate2 certificate)
        {
            if (IssuerThumbprints == null)
            {
                return false;
            }

            X509Chain chain = new X509Chain();
            chain.Build(certificate);

            for (var i = 1; i < chain.ChainElements.Count; i++)
            {
                if (IssuerThumbprints.Contains(chain.ChainElements[i].Certificate.Thumbprint, thumbprintComparer))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// "When validating a signed response message, the sender Access Point SHOULD check 
        ///  that the certificate in the response matches the metadata received from the Service 
        ///  Metadata Publisher. This is done by comparing the subject common name in the 
        ///  certificate to the value stated in the metadata. This check ensures that only the 
        ///  legitimate Access Point stated in the service metadata will be able to produce 
        ///  correct responses."
        /// </summary>
        /// <param name="certificate">The certificate to be checked.</param>
        private void ValidateAgainstExpectedCertificate(X509Certificate2 certificate)
        {
            // The same validator might also be used for communication with a START client. In 
            // this case, no identifier is expected, and this validation step is skipped:
            if (ExpectedCertificate == null)
            {
                return;
            }

            if (!ExpectedCertificate.Equals(certificate))
            {
                throw new SecurityTokenValidationException("Validation failed. Certificate in the response does not match the metadata from the SMP.");
            }
        }

        public Action<X509Certificate2> Validator = cert =>
        { };

        #region CA thumbprints
        private static ThumbprintComparer thumbprintComparer = new ThumbprintComparer();
        private class ThumbprintComparer : System.Collections.Generic.IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.InvariantCultureIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj.ToUpper().GetHashCode();
            }
        }

        private string[] RootThumbprints
        {
            get { return Thumbprints.RootCAThumbprints; }
        }

        private string[] IssuerThumbprints
        {
            get
            {
                switch (_expectedIssuer)
                {
                    case CertificateIssuer.None: return null;
                    case CertificateIssuer.AccessPointCA: return Thumbprints.AccessPointCAThumbprints;
                    case CertificateIssuer.SmpCA: return Thumbprints.SmpCAThumbprints;
                    default:
                        throw new InvalidEnumArgumentException("Value for ExpectedIssuer is not supported.");
                }
            }
        }
        #endregion
    }

    public class CertificateValidationThumbprints
    {
        public string[] AccessPointCAThumbprints { get; set; }
        public string[] SmpCAThumbprints { get; set; }
        public string[] RootCAThumbprints { get; set; }

        /// <remarks>
        /// To make assignments easier.
        /// </remarks>
        public string AccessPointCAThumbprint { set { AccessPointCAThumbprints = new[] { value }; } }
        public string SmpCAThumbprint { set { SmpCAThumbprints = new[] { value }; } }
        public string RootCAThumbprint { set { RootCAThumbprints = new[] { value }; } }

        public static CertificateValidationThumbprints FromConfigurationElement(string elementName)
        {
            return FromConfigurationElements(new[] { elementName });
        }

        public static CertificateValidationThumbprints FromConfigurationElements(string[] elementNames)
        {
            return new CertificateValidationThumbprints()
            {
                AccessPointCAThumbprints = configuration.CertificatesConfiguration.Validation.AccessPointCAThumbprintsByName(elementNames),
                SmpCAThumbprints = configuration.CertificatesConfiguration.Validation.SmpCAThumbprintsByName(elementNames),
                RootCAThumbprints = configuration.CertificatesConfiguration.Validation.RootCAThumbprintsByName(elementNames)
            };
        }

        public static CertificateValidationThumbprints FromConfiguration()
        {
            return new CertificateValidationThumbprints()
            {
                AccessPointCAThumbprints = configuration.CertificatesConfiguration.Validation.AccessPointCAThumbprints,
                SmpCAThumbprints = configuration.CertificatesConfiguration.Validation.SmpCAThumbprints,
                RootCAThumbprints = configuration.CertificatesConfiguration.Validation.RootCAThumbprints
            };
        }
    }

    public static class X509ChainExtensions
    {
        // For a chain with length == 3, the elements are:
        // index 0  = certificate being validated (e.g. APP_1000000005)
        // index 1  = issuer                      (e.g. PEPPOL ACCESS POINT Test CA, i.e. an intermediate CA)
        // index 2  = root                        (e.g. PEPPOL Root Test CA, i.e. the root CA)
        public static X509ChainElement Issuer(this X509Chain chain)
        {
            return chain.ChainElements[1];
        }
        public static X509ChainElement Root(this X509Chain chain)
        {
            return chain.ChainElements[chain.ChainElements.Count - 1];
        }

        public static bool HasRootIn(this X509Chain chain, X509Certificate2Collection extraTrustedRootCertificates)
        {
            if (extraTrustedRootCertificates == null || extraTrustedRootCertificates.Count == 0)
            {
                return false;
            }

            return (extraTrustedRootCertificates.Contains(chain.Root().Certificate));
        }

        public static bool HasUntrustedRoot(this X509Chain chain)
        {
            return chain.ChainStatusEquals(X509ChainStatusFlags.UntrustedRoot);
        }

        public static bool HasNoError(this X509Chain chain)
        {
            return chain.ChainStatus.Length == 0 ||
                   chain.ChainStatusEquals(X509ChainStatusFlags.NoError);
        }

        public static bool ChainStatusEquals(this X509Chain chain, X509ChainStatusFlags statusToCheck)
        {
            return (chain.ChainStatus.Length == 1) &&
                   (chain.ChainStatus[0].Status == statusToCheck);
            //return chainStatus.All(status =>

            //    (status.Status & statusToCheck) == status.Status

            //    );
        }
    }
    /// <summary>
    /// Defines which issuer should have signed a given certificate.
    /// </summary>
    public enum CertificateIssuer
    {
        None,
        AccessPointCA,
        SmpCA
    }
}
