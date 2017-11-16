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

using System.ServiceModel;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Configuration;
using System.ServiceModel.Description;
using System.IdentityModel.Selectors;

namespace STARTAccessPoint.eu.peppol.start.saml2handler
{
    /// <summary>
    /// Overrides CreateSecurityTokenManager to return a StartSecurityTokenManager instance.
    /// </summary>
    /// <remarks>
    /// Just a helper class which allows to use two custom classes:  
    /// StartSaml2TokenSerializerAdapter and StartSaml2SecurityTokenHandler.
    /// </remarks>
    public class StartServiceCredentials : FederatedServiceCredentials
    {
        public static void WrapStandardCredentials(ServiceHost serviceHost)
        {
            var credentials = serviceHost.Description.Behaviors.Remove<ServiceCredentials>();
            serviceHost.Description.Behaviors.Add(new StartServiceCredentials(credentials));
        }

        public StartServiceCredentials(ServiceCredentials credentials)
            : base(credentials) { }

        public StartServiceCredentials(ServiceCredentials credentials, ServiceConfiguration configuration)
            : base(credentials, configuration) { }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new StartSecurityTokenManager(this);
        }

        protected override ServiceCredentials CloneCore()
        {
            return new StartServiceCredentials(this);
        }
    }

    /// <summary>
    /// Overrides CreateSecurityTokenSerializer to return a StartSaml2Adapter instance.
    /// </summary>
    /// <remarks>
    /// Just a helper class which allows to use two custom classes:  
    /// StartSaml2TokenSerializerAdapter and StartSaml2SecurityTokenHandler.
    /// </remarks>
    class StartSecurityTokenManager : FederatedSecurityTokenManager
    {
        public StartSecurityTokenManager(StartServiceCredentials credentials)
            : base(credentials, credentials.SecurityTokenHandlers, credentials.ClaimsAuthenticationManager)
        {
        }

        public override SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version)
        {
            var securityTokenSerializer = base.CreateSecurityTokenSerializer(version) as SecurityTokenSerializerAdapter;
            return new StartSaml2TokenSerializerAdapter(securityTokenSerializer.SecurityTokenHandlers, securityTokenSerializer.SecurityVersion);
        }
    }
}
