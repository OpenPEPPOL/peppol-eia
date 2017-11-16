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
using System.Xml;
using System.ServiceModel;
using System.Security.Cryptography.X509Certificates;
using STARTLibrary.accesspointService;
using STARTLibrary.src.eu.peppol.start.io;
using STARTLibrary.src.eu.peppol.start.common;
using common = STARTLibrary.src.eu.peppol.start.security.common;
using System.Diagnostics;

namespace STARTLibrary.src.eu.peppol.start.security.impl
{
    public class SimpleAccessPointClient
    {
        Helper help = new Helper();

        private static readonly Dictionary<string, ChannelFactory<STARTLibrary.accesspointService.Resource>> Factories = new Dictionary<string, ChannelFactory<STARTLibrary.accesspointService.Resource>>();

        private readonly ChannelFactory<STARTLibrary.accesspointService.Resource> resourceFactory;
        private readonly int assuranceLevel;
        private readonly string smlDomain;
        private readonly Uri thisUri;
                
        public SimpleAccessPointClient(string endpointConfigurationName,
                                        int assuranceLevel,
                                        string smlDomain,
                                        Uri thisUri,
                                        X509Certificate2 clientCertificate,
                                        X509Certificate2 serviceCertificate)
        {
            this.assuranceLevel = assuranceLevel;
            this.thisUri = thisUri;
            this.smlDomain = smlDomain;

            Factories.Clear();

            if (!Factories.ContainsKey(endpointConfigurationName))
            {
                resourceFactory = common.Utilities.CreateResourceChannelFactory<STARTLibrary.accesspointService.Resource>(endpointConfigurationName, clientCertificate, serviceCertificate);
                Factories.Add(endpointConfigurationName, resourceFactory);
            }
            else
            {
                resourceFactory = Factories[endpointConfigurationName];
            }
        }
        #region "AP methods"

        /// <summary>
        /// Send metadata to create a message
        /// </summary>        
        public string Send(IMessageMetadata metadata, XmlDocument body)
        {
            try
            {
                STARTLibrary.accesspointService.Create messageBody = new STARTLibrary.accesspointService.Create();
                messageBody.Any = new XmlElement[] { body.DocumentElement };

                var request = new STARTLibrary.accesspointService.CreateRequest
                {
                    MessageIdentifier = metadata.MessageIdentifier,
                    ChannelIdentifier = metadata.ChannelIdentifier,
                    RecipientIdentifier = metadata.RecipientIdentifier,
                    SenderIdentifier = metadata.SenderIdentifier,
                    DocumentIdentifier = metadata.DocumentIdentifier,
                    ProcessIdentifier = metadata.ProcessIdentifier,
                    Create = messageBody
                };

                return common.Utilities.CallCreate(resourceFactory, thisUri, smlDomain, assuranceLevel, request);
            }
            catch (Exception) 
            {
                throw help.MakePeppolException("bden:ServerError", "ServerError");
            }
        }

        #endregion
    }
}