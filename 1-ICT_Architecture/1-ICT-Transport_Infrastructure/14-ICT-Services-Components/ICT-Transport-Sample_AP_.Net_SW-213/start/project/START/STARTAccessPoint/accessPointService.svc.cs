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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Xml;
using System.Web;
using System.Configuration;
using System.ServiceModel.Activation;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using STARTLibrary;
using STARTLibrary.accesspointService;
using STARTLibrary.src.eu.peppol.start.io;
using STARTLibrary.src.eu.peppol.start.common;
using STARTLibrary.src.eu.peppol.start.impl;
using STARTLibrary.src.eu.peppol.start.security.configuration;
using security_impl = STARTLibrary.src.eu.peppol.start.security.impl;
using security_common = STARTLibrary.src.eu.peppol.start.security.common;
using STARTLibrary.src.eu.peppol.start.security.handler;
using STARTAccessPoint.eu.peppol.start.saml2handler;
using STARTAccessPoint.eu.peppol.start.behavior;
using STARTLibrary.src.eu.peppol.start.security.impl;
using System.Net;

namespace STARTAccessPoint
{
    [ServiceBehavior(Name = "accesspointService", Namespace = "http://www.w3.org/2009/02/ws-tra")]
    public class accesspointService : Resource
    {
        #region Resource Members

        IOLayer io = new IOLayer();
        Helper help = new Helper();
        WriteRequest wr = new WriteRequest();

        /// <summary>
        /// .NET accesspointService Create function
        /// </summary>
        public CreateResponse1 Create(CreateRequest request)
        {
            if (request == null || request.Create == null ||
               request.Create.Any == null || request.Create.Any.Length < 1 ||
               request.Create.Any[0] == null)
            {
                throw help.MakePeppolException("bden:ServerError", "ServerError");
            }
            if (!IsPing(request))
            {
                try
                {
                    wr.PersistsCreate(request);
                }
                catch (Exception)
                {
                    throw help.MakePeppolException("bden:SecurityFault", "There is a security error in processing this request.");
                }
            }
            return new CreateResponse1(new CreateResponse()
            {
            });
        }

        /// <summary>
        /// .NET accesspointService Get function
        /// </summary>
        public GetResponse1 Get(GetRequest request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// .NET accesspointService Put function
        /// </summary>
        public PutResponse1 Put(PutRequest request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// .NET accesspointService Delete function
        /// </summary>
        public DeleteResponse1 Delete(DeleteRequest request)
        {
            throw new NotImplementedException();
        }

        #endregion

        static bool IsPing(CreateRequest rq)
        {
            return rq.Create != null &&
                   rq.Create.Any != null &&
                   rq.Create.Any.Length == 1 &&
                   "Ping" == rq.Create.Any[0].Name &&
                   "http://busdox.org/transport/start/1.0/" == rq.Create.Any[0].NamespaceURI &&
                   rq.SenderIdentifier != null &&
                   "busdox-actorid-transport" == rq.SenderIdentifier.scheme &&
                   "busdox:sender" == rq.SenderIdentifier.Value &&
                   rq.RecipientIdentifier != null &&
                   "busdox-actorid-transport" == rq.RecipientIdentifier.scheme &&
                   "busdox:recipient" == rq.RecipientIdentifier.Value &&
                   rq.DocumentIdentifier != null &&
                   "busdox-docid-qns" == rq.DocumentIdentifier.scheme &&
                   "busdox:ping" == rq.DocumentIdentifier.Value &&
                   rq.ProcessIdentifier != null &&
                   "busdox-procid-transport" == rq.ProcessIdentifier.scheme &&
                   "busdox:noprocess" == rq.ProcessIdentifier.Value;
        }

        /// <remarks>
        /// Public so that it can be changed in the LIME/START tests.
        /// </remarks>
        public static string EndpointName = "SecurePeppolClient";

        private static ChannelFactory<STARTLibrary.accesspointService.Resource> CreateResourceFactory()
        {

            if (!STARTLibrary.src.eu.peppol.start.security.configuration.CertificatesConfiguration.HasClientCredentials(EndpointName))
            {
                throw new ConfigurationErrorsException(String.Format("Config section does not contain client credentials for endpoint \"{0}\".", EndpointName));
            }
            var credentials = STARTLibrary.src.eu.peppol.start.security.configuration.CertificatesConfiguration.ClientCredentials.ByEndpointName(EndpointName);

            return STARTLibrary.src.eu.peppol.start.security.common.Utilities.CreateResourceChannelFactory<STARTLibrary.accesspointService.Resource>(EndpointName,
                                                                    credentials.ClientCertificate.Certificate,
                                                                    credentials.ServiceCertificate.Certificate);
        }
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

    public class StartServiceFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            if (!CertificatesConfiguration.HasServiceCredentials(serviceType.FullName))
            {
                throw new ConfigurationErrorsException(String.Format("Config section does not contain service credentials for service \"{0}\".", serviceType.FullName));
            }
            var credentials = CertificatesConfiguration.ServiceCredentials.ByServiceName(serviceType.FullName);

            return InitServiceHost(
                    new ServiceHost(serviceType, baseAddresses),
                //Resource from IaccesspointService
                    typeof(STARTAccessPoint.Resource),
                    credentials.ServiceCertificate.Certificate
                );
        }

        public static ServiceHost InitServiceHost(ServiceHost serviceHost, Type contractType, X509Certificate2 serviceCertificate)
        {
            var ep = serviceHost.Description.Endpoints.Find(contractType);
            if (ep == null)
                throw new ArgumentException("Endpoint with this contract type could not be found.", "contractType");

            security_common.Utilities.ConfigBinding(ep.Binding);
            ep.Contract.ProtectionLevel = System.Net.Security.ProtectionLevel.Sign;

            FederatedServiceCredentials.ConfigureServiceHost(serviceHost,
                                                             new ServiceConfiguration
                                                             {
                                                                 ServiceCertificate = serviceCertificate
                                                             });

            // Allows to use custom StartSaml2Adapter and Saml2SecurityTokenHandler:
            StartServiceCredentials.WrapStandardCredentials(serviceHost);
            
            /* Change according to your IP or Server:                            */
            /* Remove this when using server.                                    */
            /* (This method is used when trying to change the host of wsdl to IP */
            HttpToHttpsWsdlBehavior.AdjustWsdlOutput(serviceHost,
                "https://pc-net00/start-ap/accessPointService.svc",
                "https://192.168.1.40:443/start-ap/accessPointService.svc");
            return serviceHost;
        }
    }
}
