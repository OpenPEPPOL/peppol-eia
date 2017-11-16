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
 
/*Notes:
 * 
 * GetEndPointAddress method:
 * 
 * If serviceGroupElements variable is null means that an ERROR 404 was produced due to ParticipantIdentifier does not exist
 * If serviceGroupElements variable is not null means that the ParticipantIdentifier exists
 * Then a request with smpSignedServiceUrl parameter is executed for getting signedServiceElements
 * If signedServiceElements variable is null means that the SMP not support service capabitily
 * If signedServiceElements variable is not null means that the SMP has service capabilities
 * If signedServiceElements can not capture EndPointAddress, ParticipantIdentifier is uppercase (has to be lowercase)
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Web;
using System.Security.Cryptography;
using STARTLibrary.src.eu.peppol.start.xml.parser;
using STARTLibrary.src.eu.peppol.start.xml.request;
using STARTLibrary.src.eu.peppol.start.xml.structure;
using System.ServiceModel;

namespace STARTLibrary.src.eu.peppol.start.common
{
    public class Helper
    {
        ServiceGroupParser _serviceGroupParser = new ServiceGroupParser();
        SignedServiceParser _signedServiceParser = new SignedServiceParser();

        /// <summary>
        /// Get EndPoint Address with participant validation
        /// </summary>
        /// <param name="recipientParticipant"></param>
        /// <param name="businessIdScheme"></param>
        /// <param name="smlDomain"></param>
        /// <param name="documentIdScheme"></param>
        /// <param name="documentIdValue"></param>
        /// <returns>EndPointAddress if participant exists and has SMP service capabilities</returns>
        public string GetEndPointAddress(string recipientParticipant, string businessIdScheme, string smlDomain, string documentIdScheme, string documentIdValue)
        {
            string endPointAddress = null;

            string smpUrl = BuildSmpUrl(recipientParticipant, businessIdScheme, smlDomain);
            ServiceGroup serviceGroupElements = _serviceGroupParser.GetServiceGroup(smpUrl);

            if (serviceGroupElements != null)
            {
                string smpSignedServiceUrl = BuildSmpSignedServiceUrl(recipientParticipant, businessIdScheme, smlDomain, documentIdScheme, documentIdValue);
                SignedServiceMetadata signedServiceElements = _signedServiceParser.GetSignedService(smpSignedServiceUrl);

                if (signedServiceElements != null)
                {
                    if (signedServiceElements.ServiceMetadata.ServiceInformation == null)
                    {
                        string href = signedServiceElements.ServiceMetadata.Redirect.href;
                        SignedServiceMetadata redirectServiceElements = _signedServiceParser.GetSignedService(href);

                        if (redirectServiceElements != null)
                        {
                            if (redirectServiceElements.ServiceMetadata.Redirect == null)
                            {
                                try
                                {
                                    endPointAddress = redirectServiceElements.ServiceMetadata.ServiceInformation.ProcessList.Process[0].ServiceEndpointList.Endpoint.EndpointReference.Address;
                                }
                                catch (Exception)
                                {
                                    throw MakePeppolException("bden:UnknownEndpoint", "The endpoint is not known.");
                                }
                            }
                            else
                            {
                                throw MakePeppolException("bden:SecurityFault", "WARNING: More than one Redirection was found in Metadata for Participant.");
                            }
                        }
                        else
                        {
                            throw MakePeppolException("bden:DocumentTypeNotAccepted", "The recipient does not accept documents of this type.");
                        }
                    }
                    else
                    {
                        try
                        {
                            endPointAddress = signedServiceElements.ServiceMetadata.ServiceInformation.ProcessList.Process[0].ServiceEndpointList.Endpoint.EndpointReference.Address;
                        }
                        catch (Exception)
                        {
                            throw MakePeppolException("bden:UnknownEndpoint", "The endpoint is not known.");
                        }
                    }
                }
                else
                {
                    throw MakePeppolException("bden:DocumentTypeNotAccepted", "The recipient does not accept documents of this type.");
                }
            }
            else
            {
                throw MakePeppolException("bden:ChannelFull", "The channel is not accepting messages for this destination.");
            }
            return endPointAddress;
        }

        /// <summary>
        /// Get Client Certificate of EndPoint Address with participant validation
        /// </summary>
        /// <param name="recipientParticipant"></param>
        /// <param name="businessIdScheme"></param>
        /// <param name="smlDomain"></param>
        /// <param name="documentIdScheme"></param>
        /// <param name="documentIdValue"></param>
        /// <returns>Client Certificate if participant exists and has SMP service capabilities</returns>
        public string GetClientCertificate(string recipientParticipant, string businessIdScheme, string smlDomain, string documentIdScheme, string documentIdValue)
        {
            string clientCertificate = null;

            string smpUrl = BuildSmpUrl(recipientParticipant, businessIdScheme, smlDomain);
            ServiceGroup serviceGroupElements = _serviceGroupParser.GetServiceGroup(smpUrl);

            if (serviceGroupElements != null)
            {
                string smpSignedServiceUrl = BuildSmpSignedServiceUrl(recipientParticipant, businessIdScheme, smlDomain, documentIdScheme, documentIdValue);
                SignedServiceMetadata signedServiceElements = _signedServiceParser.GetSignedService(smpSignedServiceUrl);

                if (signedServiceElements != null)
                {
                    try
                    {
                        clientCertificate = signedServiceElements.ServiceMetadata.ServiceInformation.ProcessList.Process[0].ServiceEndpointList.Endpoint.Certificate;
                    }
                    catch (Exception)
                    {
                        clientCertificate = null;
                    }
                }
                else
                {
                    clientCertificate = null;
                }
            }
            else
            {
                clientCertificate = null;
            }
            return clientCertificate;
        }

        /// <summary>
        /// Get SMP Service Group deserialization objects
        /// </summary>
        /// <param name="recipientParticipant"></param>
        /// <param name="businessIdScheme"></param>
        /// <param name="smlDomain"></param>
        /// <returns>serviceGroupElements type of ServiceGroup</returns>
        public ServiceGroup GetServiceGroupObjects(string recipientParticipant, string businessIdScheme, string smlDomain)
        {
            ServiceGroupParser _serviceGroupParser = new ServiceGroupParser();
            string smpUrl = BuildSmpUrl(recipientParticipant, businessIdScheme, smlDomain);
            ServiceGroup serviceGroupElements = _serviceGroupParser.GetServiceGroup(smpUrl);
            return serviceGroupElements;
        }

        /// <summary>
        /// Get Signed Service deserialization objects
        /// </summary>
        /// <param name="recipientParticipant"></param>
        /// <param name="businessIdScheme"></param>
        /// <param name="smlDomain"></param>
        /// <param name="documentIdScheme"></param>
        /// <param name="documentIdValue"></param>
        /// <returns>signedServiceElements type of SignedServiceMetadata</returns>
        public SignedServiceMetadata GetSignedServiceObjects(string recipientParticipant, string businessIdScheme, string smlDomain, string documentIdScheme, string documentIdValue)
        {
            SignedServiceParser _signedServiceParser = new SignedServiceParser();
            string smpSignedServiceUrl = BuildSmpSignedServiceUrl(recipientParticipant, businessIdScheme, smlDomain, documentIdScheme, documentIdValue);
            SignedServiceMetadata signedServiceElements = _signedServiceParser.GetSignedService(smpSignedServiceUrl);
            return signedServiceElements;
        }

        /// <summary>
        /// Get MD5 hash of ParticipantIdentifier
        /// </summary>
        /// <param name="ParticipantIdentifier"></param>
        /// <returns>ParticipantIdentifier MD5 hash string</returns>
        public string GetMD5Hash(string ParticipantIdentifier)
        {
            UTF8Encoding _Encoding = new UTF8Encoding();
            byte[] _DataByte = _Encoding.GetBytes(ParticipantIdentifier.ToLower());
            byte[] _ResultByte = null;
            MD5CryptoServiceProvider _MD5CryptoServiceProvider = new MD5CryptoServiceProvider();
            _ResultByte = _MD5CryptoServiceProvider.ComputeHash(_DataByte);
            StringBuilder _StringBuilder = new StringBuilder();
            for (int i = 0; i <= _ResultByte.Length - 1; i++)
            {
                if (_ResultByte[i] < 16)
                {
                    _StringBuilder.Append("0");
                }
                _StringBuilder.Append(_ResultByte[i].ToString("x"));
            }
            return _StringBuilder.ToString();
        }

        /// <summary>
        /// Build Signed Service Url
        /// </summary>
        /// <param name="participantIdentifier"></param>
        /// <param name="businessIdScheme"></param>
        /// <param name="smlDomain"></param>
        /// <param name="documentIdScheme"></param>
        /// <param name="documentIdValue"></param>
        /// <returns>Signed Service url string</returns>
        public string BuildSmpSignedServiceUrl(string participantIdentifier, string businessIdScheme, string smlDomain, string documentIdScheme, string documentIdValue)
        {
            string url = string.Format("b-{0}.{1}.{2}/{1}::{3}/services/{4}::{5}",
                                          GetMD5Hash(participantIdentifier),
                                          businessIdScheme,
                                          smlDomain,
                                          participantIdentifier,
                                          documentIdScheme,
                                          documentIdValue).Replace(":", "%3A").Replace("#", "%23");

            return string.Concat("http://", url);
        }

        /// <summary>
        /// Build SMP Url
        /// </summary>
        /// <param name="participantIdentifier"></param>
        /// <param name="businessIdScheme"></param>
        /// <param name="smlDomain"></param>
        /// <returns>SMP url string</returns>
        public string BuildSmpUrl(string participantIdentifier, string businessIdScheme, string smlDomain)
        {
            string url = string.Format("b-{0}.{1}.{2}/{1}::{3}",
                                          GetMD5Hash(participantIdentifier.ToLower()),
                                          businessIdScheme,
                                          smlDomain,
                                          participantIdentifier,
                                          businessIdScheme).Replace(":", "%3A").Replace("#", "%23");

            return string.Concat("http://", url);
        }

        /// <summary>
        /// Url Encode
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Encoded url</returns>
        public static string EncodeUrl(string url)
        {
            return HttpUtility.UrlEncode(url);
        }

        /// <summary>
        /// Url Decode
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Decoded url</returns>
        public static string DecodeUrl(string url)
        {
            return HttpUtility.UrlDecode(url);
        }

        /// <summary>
        /// Make Peppol Exception
        /// </summary>        
        /// <returns>Throw an exception</returns>
        public FaultException MakePeppolException(string SubCode, string Reason)
        {
            return new FaultException(new FaultReason(Reason),
                                          new FaultCode("Sender", "http://schemas.xmlsoap.org/soap/envelope/",
                                                         new FaultCode(SubCode, "http://bden")), "http://busdox.org/2010/02/channel/fault");
        }
    }
}
