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

using System.Configuration;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace STARTLibrary.src.eu.peppol.start.security.configuration
{
    public class CertificatesConfigurationSection : ConfigurationSection
    {
        public static CertificatesConfigurationSection Section
        {
            get { return (CertificatesConfigurationSection)ConfigurationManager.GetSection("peppol.certificates"); }
        }

        /// <summary>
        /// Checks whether the configuration section contains an element for the given service name.
        /// </summary>
        /// <param name="serviceName">The name of the service for which crendentials should be specified.</param>
        /// <returns>True if the element exists OR if an element with wildcard exist.</returns>
        /// <remarks>
        /// Should be understood as "has any credentials which can be used with this service',
        /// not 'has credentials specific to this service'.
        /// </remarks>
        public static bool HasServiceCredentials(string serviceName)
        {
            return Section != null && Section.ServiceCredentials != null && Section.ServiceCredentials.ByServiceName(serviceName) != null;
        }

        /// <summary>
        /// Checks whether the configuration section contains an element for the given endpoint name.
        /// </summary>
        /// <param name="endpointName">The name of the endpoint for which crendentials should be specified.</param>
        /// <returns>True if the element exists OR if an element with wildcard exist.</returns>
        /// <remarks>
        /// Should be understood as "has any credentials which can be used with this endpoint',
        /// not 'has credentials specific to this endpoint'.
        /// </remarks>
        public static bool HasClientCredentials(string endpointName)
        {
            return Section != null && Section.ClientCredentials != null && Section.ClientCredentials.ByEndpointName(endpointName) != null;
        }

        [ConfigurationProperty("serviceCredentials")]
        public ServiceCredentialsConfigurationElementCollection ServiceCredentials
        {
            get { return this["serviceCredentials"] as ServiceCredentialsConfigurationElementCollection; }
        }

        [ConfigurationProperty("clientCredentials")]
        public ClientCredentialsConfigurationElementCollection ClientCredentials
        {
            get { return this["clientCredentials"] as ClientCredentialsConfigurationElementCollection; }
        }

        [ConfigurationProperty("validation")]
        public ValidationConfigurationElementCollection Validation
        {
            get { return this["validation"] as ValidationConfigurationElementCollection; }
        }
    }

    /// <summary>
    /// For shorter notation only - e.g.
    ///     CertificatesConfiguration.ServiceCredentials
    /// instead of
    ///     CertificatesConfigurationSection.Section.ServiceCredentials
    /// </summary>
    public class CertificatesConfiguration
    {
        public static bool HasServiceCredentials(string serviceName)
        {
            return CertificatesConfigurationSection.HasServiceCredentials(serviceName);
        }

        public static bool HasClientCredentials(string endpointName)
        {
            return CertificatesConfigurationSection.HasClientCredentials(endpointName);
        }

        public static ServiceCredentialsConfigurationElementCollection ServiceCredentials
        {
            get
            {
                return CertificatesConfigurationSection.Section.ServiceCredentials;
            }
        }

        public static ClientCredentialsConfigurationElementCollection ClientCredentials
        {
            get
            {
                return CertificatesConfigurationSection.Section.ClientCredentials;
            }
        }

        public static ValidationConfigurationElementCollection Validation
        {
            get
            {
                return CertificatesConfigurationSection.Section.Validation;
            }
        }
    }

}
