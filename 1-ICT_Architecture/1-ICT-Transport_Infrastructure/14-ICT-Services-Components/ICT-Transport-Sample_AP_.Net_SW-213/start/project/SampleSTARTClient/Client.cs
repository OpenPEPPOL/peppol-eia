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
using System.Xml;
using System.Net;
using System.IO;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using STARTLibrary.src.eu.peppol.start.security.common;
using STARTLibrary.src.eu.peppol.start.security.impl;
using STARTLibrary.src.eu.peppol.start.io;
using STARTLibrary.src.eu.peppol.start.impl;
using STARTLibrary.accesspointService;
using STARTLibrary.src.eu.peppol.start.common;

namespace SampleSTARTClient
{
    class Client
    {
        static void Main(string[] args)
        {
            string endpointName = ConfigurationManager.AppSettings["endpointName"] ??
                                      "SecurePeppolClient";
            Console.WriteLine("Using endpoint configuration with name \"{0}\"..", endpointName);

            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, error) =>
            {
                return true;
            };

            Console.WriteLine("\nStarting..");

            //Send message
            StartMessage(endpointName, Certificates.FromConfig(endpointName));
            //Make ping
            //MakePing(endpointName, "https://192.168.1.40:443/start-ap/accessPointService.svc");

            Console.WriteLine("\nDone. Press any key to proceed.");
            Console.ReadKey();
        }

        public static void StartMessage(string endpointConfigName, Certificates certificates)
        {
            Console.WriteLine("\nInstantiating client..");

            string senderValue = "9902:DK87654321";
            string recipientValue = "9902:DK76543218";
            string businessIdScheme = "iso6523-actorid-upis";
            string documentIdScheme = "busdox-docid-qns";
            string documentIdValue = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2::Invoice##urn:www.cenbii.eu:transaction:biicoretrdm010:ver1.0:#urn:www.peppol.eu:bis:peppol4a:ver1.0::2.0";
            string processIdValue = "urn:www.cenbii.eu:profile:bii04:ver1.0";
            string processIdScheme = "cenbii-procid-ubl";

            IMessageMetadata metadata = new MessageMetadata();
            metadata.RecipientIdentifier = new ParticipantIdentifierType();
            metadata.RecipientIdentifier.Value = recipientValue.ToLower();
            metadata.RecipientIdentifier.scheme = businessIdScheme;

            metadata.SenderIdentifier = new ParticipantIdentifierType();
            metadata.SenderIdentifier.Value = senderValue.ToLower();
            metadata.SenderIdentifier.scheme = businessIdScheme;

            metadata.DocumentIdentifier = new DocumentIdentifierType();
            metadata.DocumentIdentifier.Value = documentIdValue;
            metadata.DocumentIdentifier.scheme = documentIdScheme;

            metadata.ProcessIdentifier = new ProcessIdentifierType();
            metadata.ProcessIdentifier.Value = processIdValue;
            metadata.ProcessIdentifier.scheme = processIdScheme;

            string channelIdentifier = senderValue.ToLower();
            metadata.ChannelIdentifier = channelIdentifier;

            Random rnd = new Random((int)DateTime.Now.Ticks);
            metadata.MessageIdentifier = "uuid:" + Guid.NewGuid().ToString("D");

            XmlDocument body = new XmlDocument();
            body.Load(@"D:\path-to\MyDocument.xml");

            /* Lookup recipient participant */
            Helper help = new Helper();
            string recipientEndPointAddress = help.GetEndPointAddress(metadata.RecipientIdentifier.Value,
                                                            metadata.RecipientIdentifier.scheme,
                                                            ConfigurationManager.AppSettings["smlDomain"],
                                                            metadata.DocumentIdentifier.scheme,
                                                            metadata.DocumentIdentifier.Value);

            /* Start client and send */
            var secureClient = new SimpleAccessPointClient(endpointConfigName,
                                                          int.Parse(ConfigurationManager.AppSettings["AssuranceLevel"]),
                                                          ConfigurationManager.AppSettings["smlDomain"],
                                                          new Uri(recipientEndPointAddress),
                                                          certificates.ClientCertificate,
                                                          certificates.ServiceCertificate);

            Console.WriteLine("\nPlease Wait..");
            Console.WriteLine("\n" + secureClient.Send(metadata, body));
        }

        static void MakePing(string endpointConfigName, string endpointAddress)
        {
            PingProcess(endpointConfigName,
                        endpointAddress,
                        Certificates.FromConfig(endpointConfigName));
        }

        static void PingProcess(string endpointConfigName, string endpointAddress, Certificates certificates)
        {
            IMessageMetadata metadata = new MessageMetadata()
            {
                RecipientIdentifier = new ParticipantIdentifierType()
                {
                    Value = "busdox:recipient",
                    scheme = "busdox-actorid-transport"
                },
                SenderIdentifier = new ParticipantIdentifierType()
                {
                    Value = "busdox:sender",
                    scheme = "busdox-actorid-transport"
                },

                DocumentIdentifier = new DocumentIdentifierType()
                {
                    Value = "busdox:ping",
                    scheme = "busdox-docid-qns"
                },

                ProcessIdentifier = new ProcessIdentifierType()
                {
                    Value = "busdox:noprocess",
                    scheme = "busdox-procid-transport"
                }
            };

            var secureClient = new SimpleAccessPointClient(endpointConfigName, int.Parse(ConfigurationManager.AppSettings["AssuranceLevel"]),
                                                          null,
                                                          new Uri(endpointAddress),
                                                          certificates.ClientCertificate,
                                                          certificates.ServiceCertificate);

            XmlDocument body = new XmlDocument();
            body.LoadXml("<Ping xmlns =\"http://busdox.org/transport/start/1.0/\"/>");

            Console.WriteLine("\nSending XML Ping..");
            secureClient.Send(metadata, body);

            Console.WriteLine("\nRemote server response was received successfully.");
        }

        public class Certificates
        {
            private static Certificates fromConfig;

            public static Certificates FromConfig(string endpointName)
            {
                if (fromConfig == null)
                {
                    var credentials = STARTLibrary.src.eu.peppol.start.security.configuration.CertificatesConfigurationSection.Section.ClientCredentials.ByEndpointName(endpointName);

                    if (credentials == null)
                    {
                        throw new ArgumentException(String.Format("No certificate configuration found for endpoint \"{0}\".", endpointName), "endpointName");
                    }

                    fromConfig = new Certificates()
                    {
                        ClientCertificate = credentials.ClientCertificate.Certificate,
                        ServiceCertificate = credentials.ServiceCertificate.Certificate
                    };
                }
                return fromConfig;
            }

            public static Certificates FromConfig()
            {
                return FromConfig(DefaultEndpointName);
            }

            private const string DefaultEndpointName = "*";

            public X509Certificate2 ServiceCertificate { get; set; }
            public X509Certificate2 ClientCertificate { get; set; }
        }
    }
}
