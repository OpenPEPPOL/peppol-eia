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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STARTLibrary.accesspointService;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security.Tokens;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Security.Cryptography;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.Xml.Linq;
using System.ServiceModel.Description;
using System.Net.Security;
using STARTLibrary.src.eu.peppol.start.security.handler;
using STARTLibrary.src.eu.peppol.start.common;
using System.Diagnostics;

namespace STARTLibrary.src.eu.peppol.start.impl
{
    public class ServiceMetadata
    {
        /// <summary>
        /// The address of the Access Point which should receive the CREATE request.
        /// </summary>
        public Uri Address { get; set; }
        /// <summary>
        /// The signing certificate of the recipient AP.
        /// </summary>
        public X509Certificate2 Certificate { get; set; }
        /// <summary>
        /// Creates a new ServiceMetadata object from the Endpoint and Credentials 
        /// information of the given ChannelFactory.
        /// Does NOT retrieve any metadata from an SMP.
        /// </summary>
        /// <param name="factory">A ChannelFactory object defining an endpoint.
        /// Uses the Uri of the endpoint's address ServiceCertificate of the Credentials.
        /// </param>
        public static ServiceMetadata FromKnownEndpoint(ChannelFactory<Resource> factory)
        {
            return new ServiceMetadata()
            {
                Address = factory.Endpoint.Address.Uri,
                Certificate = factory.Credentials.ServiceCertificate.DefaultCertificate
            };
        }

        /// <summary>
        /// Determines both address of the Access Point for a CREATE request and the expected common
        /// name in the certificate used by this Access Point.
        /// </summary>
        /// <returns>A ServiceMetadata object with the address of the Access Point and the common
        /// name in the certificate used by this Access Point.</returns>
        public static ServiceMetadata FromSml(string smlDomain, CreateRequest request)
        {
            return new ServiceMetadataPublishing(smlDomain, request).GetMetaDataFromSml(request.RecipientIdentifier.Value,
                                                                                        request.RecipientIdentifier.scheme,
                                                                                        smlDomain,
                                                                                        request.DocumentIdentifier.scheme,
                                                                                        request.DocumentIdentifier.Value);
        }
    }

    internal class ServiceMetadataPublishing
    {
        Helper help = new Helper();

        private static readonly XNamespace meta = "http://busdox.org/transport/identifiers/1.0/",
                                           adr = "http://www.w3.org/2005/08/addressing",
                                           pub = "http://busdox.org/serviceMetadata/publishing/1.0/";

        private readonly string smlDomain;
        private readonly CreateRequest request;

        private string RecipientIdentifierValue { get { return request.RecipientIdentifier.Value; } }
        private string RecipientIdentifierScheme { get { return request.RecipientIdentifier.scheme; } }
        private string DocumentIdentifierValue { get { return request.DocumentIdentifier.Value; } }
        private string DocumentIdentifierScheme { get { return request.DocumentIdentifier.scheme; } }
        private string ProcessIdentifierValue { get { return request.ProcessIdentifier.Value; } }
        private string ProcessIdentifierScheme { get { return request.ProcessIdentifier.scheme; } }

        internal ServiceMetadataPublishing(string smlDomain, CreateRequest request)
        {
            this.smlDomain = smlDomain;
            this.request = request;
        }

        internal ServiceMetadata GetMetaDataFromSml(string recipientParticipant, string businessIdScheme, string smlDomain, string documentIdScheme, string documentIdValue)
        {
            XmlDocument doc = LoadMetadataXml(recipientParticipant, businessIdScheme, smlDomain, documentIdScheme, documentIdValue);
            XmlDocument nextDoc = null;

            int redirectFromDoc = doc.GetElementsByTagName("Redirect", "*").Count;

            // Redirect node exits
            if (redirectFromDoc > 0)
            {
                // Get Redirect href from doc metadata
                string href = doc.GetElementsByTagName("Redirect", "*")[0].Attributes["href"].InnerText;

                // Get certificateUID from doc
                string UIDFromDoc = doc.GetElementsByTagName("CertificateUID", "*")[0].InnerText;

                // Load new metadata from redirect
                nextDoc = LoadMetadataXml(href);

                // If there is another redirect in metadata, throw ex;
                int redirectFromNextDoc = nextDoc.GetElementsByTagName("Redirect", "*").Count;
                if (redirectFromNextDoc > 0)
                {
                    throw help.MakePeppolException("bden:SecurityFault", "WARNING: More than one Redirection was found in Metadata for Participant.");
                }

                // Get certificate from redirect request
                string smpCertificate = nextDoc.GetElementsByTagName("X509Certificate", "*")[0].InnerText;
                byte[] arrayCertificate;
                arrayCertificate = Convert.FromBase64String(smpCertificate);
                Mono.Security.X509.X509Certificate smpCrt = new Mono.Security.X509.X509Certificate(arrayCertificate);

                // Get Subject UID
                if (smpCrt.SubjectUniqueIdentifier != null)
                {
                    Encoding enc = Encoding.Default;
                    string smpSubjectUID = enc.GetString(smpCrt.SubjectUniqueIdentifier);

                    // If subject UID from metadata and certificate are not equal, throw ex;
                    if (smpSubjectUID != UIDFromDoc)
                    {
                        throw help.MakePeppolException("bden:SecurityFault", "WARNING: Identifier of the destination SMP signing certificate does not corresponds to the unique identifier which the redirecting SMP claims belongs to the destination SMP.");
                    }
                }
                else
                {
                    // Certificate does not contain subject UID
                    Trace.WriteLine("WARNING: Subject Unique Identifier Could not be obtained from the Certificate Entry.");
                }
            }
            else
            {
                VerifySignature(doc); // process continues as default, throws exception if invalid
                nextDoc = doc;
            }

            return ReadMetadataFromXml(nextDoc);
        }

        private XmlDocument LoadMetadataXml(string recipientParticipant, string businessIdScheme, string smlDomain, string documentIdScheme, string documentIdValue)
        {
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load(help.BuildSmpSignedServiceUrl(recipientParticipant, businessIdScheme, smlDomain, documentIdScheme, documentIdValue));
            }
            catch (System.Net.WebException ex)
            {
                var ext = help.MakePeppolException("bden:UnknownEndpoint", "The endpoint is not known");

                switch (ex.Status)
                {
                    case System.Net.WebExceptionStatus.CacheEntryNotFound:
                        break;
                    case System.Net.WebExceptionStatus.ConnectFailure: throw ext;

                    case System.Net.WebExceptionStatus.ConnectionClosed:
                        break;
                    case System.Net.WebExceptionStatus.KeepAliveFailure:
                        break;
                    case System.Net.WebExceptionStatus.MessageLengthLimitExceeded:
                        break;
                    case System.Net.WebExceptionStatus.NameResolutionFailure: throw ext;

                    case System.Net.WebExceptionStatus.Pending:
                        break;
                    case System.Net.WebExceptionStatus.PipelineFailure:
                        break;
                    case System.Net.WebExceptionStatus.ProtocolError:
                        break;
                    case System.Net.WebExceptionStatus.ProxyNameResolutionFailure:
                        break;
                    case System.Net.WebExceptionStatus.ReceiveFailure:
                        break;
                    case System.Net.WebExceptionStatus.RequestCanceled:
                        break;
                    case System.Net.WebExceptionStatus.RequestProhibitedByCachePolicy:
                        break;
                    case System.Net.WebExceptionStatus.RequestProhibitedByProxy:
                        break;
                    case System.Net.WebExceptionStatus.SecureChannelFailure:
                        break;
                    case System.Net.WebExceptionStatus.SendFailure:
                        break;
                    case System.Net.WebExceptionStatus.ServerProtocolViolation:
                        break;
                    case System.Net.WebExceptionStatus.Success:
                        break;
                    case System.Net.WebExceptionStatus.Timeout:
                        break;
                    case System.Net.WebExceptionStatus.TrustFailure:
                        break;
                    case System.Net.WebExceptionStatus.UnknownError:
                        break;
                    default:
                        break;
                }
                throw help.MakePeppolException("bden:ServerError", "ServerError");
            }
            catch
            {
                throw help.MakePeppolException("bden:ServerError", "ServerError");
            }
            return doc;
        }

        private XmlDocument LoadMetadataXml(string href)
        {
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load(href);
            }
            catch (System.Net.WebException ex)
            {
                var ext = help.MakePeppolException("bden:UnknownEndpoint", "The endpoint is not known");

                switch (ex.Status)
                {
                    case System.Net.WebExceptionStatus.CacheEntryNotFound:
                        break;
                    case System.Net.WebExceptionStatus.ConnectFailure: throw ext;

                    case System.Net.WebExceptionStatus.ConnectionClosed:
                        break;
                    case System.Net.WebExceptionStatus.KeepAliveFailure:
                        break;
                    case System.Net.WebExceptionStatus.MessageLengthLimitExceeded:
                        break;
                    case System.Net.WebExceptionStatus.NameResolutionFailure: throw ext;

                    case System.Net.WebExceptionStatus.Pending:
                        break;
                    case System.Net.WebExceptionStatus.PipelineFailure:
                        break;
                    case System.Net.WebExceptionStatus.ProtocolError:
                        break;
                    case System.Net.WebExceptionStatus.ProxyNameResolutionFailure:
                        break;
                    case System.Net.WebExceptionStatus.ReceiveFailure:
                        break;
                    case System.Net.WebExceptionStatus.RequestCanceled:
                        break;
                    case System.Net.WebExceptionStatus.RequestProhibitedByCachePolicy:
                        break;
                    case System.Net.WebExceptionStatus.RequestProhibitedByProxy:
                        break;
                    case System.Net.WebExceptionStatus.SecureChannelFailure:
                        break;
                    case System.Net.WebExceptionStatus.SendFailure:
                        break;
                    case System.Net.WebExceptionStatus.ServerProtocolViolation:
                        break;
                    case System.Net.WebExceptionStatus.Success:
                        break;
                    case System.Net.WebExceptionStatus.Timeout:
                        break;
                    case System.Net.WebExceptionStatus.TrustFailure:
                        break;
                    case System.Net.WebExceptionStatus.UnknownError:
                        break;
                    default:
                        break;
                }
                throw help.MakePeppolException("bden:ServerError", "ServerError");
            }
            catch
            {
                throw help.MakePeppolException("bden:ServerError", "ServerError");
            }
            return doc;
        }

        private void VerifySignature(XmlDocument doc)
        {
            var signedXml = new SignedXml(doc);
            XmlElement xmlsignature = doc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#").Cast<XmlElement>().FirstOrDefault();
            signedXml.LoadXml(xmlsignature);

            var certificate = signedXml.KeyInfo.Cast<KeyInfoX509Data>().Select(kid => kid.Certificates.Cast<X509Certificate2>().FirstOrDefault()).FirstOrDefault();

            if (certificate == null || !ValidateCertificate(certificate) || !signedXml.CheckSignature(certificate, true))
            {
                throw help.MakePeppolException("bden:SecurityFault", "There is a security error in processing this request");
            }
        }

        private static readonly CertificateValidator smpCertificateValidator =
                new CertificateValidator()
                {
                    ExpectedIssuer = CertificateIssuer.SmpCA
                };

        private static bool ValidateCertificate(X509Certificate2 certificate)
        {
            try
            {
                smpCertificateValidator.Validate(certificate);
                // Valid if no exception was thrown:
                return true;
            }
            catch (SecurityTokenValidationException)
            {
                return false;
            }
        }

        private ServiceMetadata ReadMetadataFromXml(XmlDocument doc)
        {
            using (var reader = new XmlNodeReader(doc))
            {
                XElement xml = XElement.Load(reader);

                return new ServiceMetadata()
                {
                    Address = ReadAddressFromXml(xml),
                    Certificate = ReadCertificateFromXml(xml)
                };
            }
        }

        private Uri ReadAddressFromXml(XElement xml)
        {
            return (from pi in xml.Descendants(meta + "ProcessIdentifier")
                    from a in pi.Attributes("scheme")
                    where a.Value == ProcessIdentifierScheme && pi.Value == ProcessIdentifierValue
                    select new Uri(pi.Parent.Descendants(adr + "Address").First().Value)).FirstOrDefault();
        }

        /// <summary>
        /// Reads "the complete signing certificate of the recipient AP, as a PEM base 
        /// 64 encoded X509 DER formatted value" from the metadata.
        /// </summary>
        private X509Certificate2 ReadCertificateFromXml(XElement xml)
        {
            string base64EncodedCertificate =
                   (from pi in xml.Descendants(meta + "ProcessIdentifier")
                    from a in pi.Attributes("scheme")
                    where a.Value == ProcessIdentifierScheme && pi.Value == ProcessIdentifierValue
                    select pi.Parent.Descendants(pub + "Certificate").First().Value).FirstOrDefault();

            byte[] decoded = System.Convert.FromBase64String(base64EncodedCertificate);

            return new X509Certificate2(decoded);
        }
    }
}

