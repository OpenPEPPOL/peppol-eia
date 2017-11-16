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
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Web.Services.Description;

namespace STARTAccessPoint.eu.peppol.start.behavior
{
    public class HttpToHttpsWsdlBehavior : IEndpointBehavior
    {
        /// <summary>
        /// Adjusts the WSDL output by replacing the service name and replacing
        /// http addresses with corresponding https addresses.
        /// </summary>
        /// <remarks>
        /// When exporting the WSDL description for the service with the 
        /// SecureServiceBinding directly, an exception with the message
        ///   "Security policy export failed. The binding contains both an 
        ///    AsymmetricSecurityBindingElement and a secure transport binding element. 
        ///    Policy export for such a binding is not supported."
        ///  will be thrown because the combination of elements is not supported.
        /// A way around this is to redirect to a different metadata location which uses
        /// the same binding except for httpTransport instead of httpsTransport. 
        /// 
        /// There are two problems with the result:
        /// 1. It contains the alternate service name
        /// 2. The URLs use http addresses, not https addresses.
        ///
        /// This class allows to replace the service name in the WSDL description and
        /// to replace the URLs with https addresses.
        /// </remarks>
        /// <param name="serviceHost">The host for which the port names should be 
        /// replaced.</param>
        /// <param name="nameToBeReplaced">The service name which appears in the 
        /// generated WSDL /// description and which shall be replaced.</param>
        /// <param name="replaceName">The service name supposed to appear in the WSDL description.</param>
        public static void AdjustWsdlOutput(ServiceHost serviceHost, string nameToBeReplaced, string replaceName)
        {
            foreach (var endpoint in serviceHost.Description.Endpoints)
            {
                endpoint.Behaviors.Add(new HttpToHttpsWsdlBehavior() { NameToBeReplaced = nameToBeReplaced, ReplaceName = replaceName });
            }
        }

        public string NameToBeReplaced { get; set; }
        public string ReplaceName { get; set; }

        #region IEndpointBehavior Members
        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
        #endregion
    }
}
