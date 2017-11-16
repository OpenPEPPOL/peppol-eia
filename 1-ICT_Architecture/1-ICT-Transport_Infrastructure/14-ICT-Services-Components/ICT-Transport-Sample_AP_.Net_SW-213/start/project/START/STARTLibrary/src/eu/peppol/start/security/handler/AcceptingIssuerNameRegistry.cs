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

using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using System;

namespace STARTLibrary.src.eu.peppol.start.security.handler
{
    /// <summary>
    /// Verifies the token issuer's X.509 certificate used to sign the token. 
    /// 
    /// All issuer tokens are validated using a IssuerNameRegistry. The 
    /// purpose of a class derived from IssuerNameRegistry is to map the 
    /// issuer token to a string name and also to decide which tokens to 
    /// accept or reject.
    /// </summary>
    public class AcceptingIssuerNameRegistry : IssuerNameRegistry
    {
        readonly CertificateValidator validator = new CertificateValidator()
        {
            ExpectedIssuer = CertificateIssuer.AccessPointCA
        };

        /// <summary>
        /// Get issuer name from a security token
        /// </summary>        
        public override string GetIssuerName(SecurityToken securityToken)
        {
            if (securityToken is X509SecurityToken)
            {
                return GetIssuerNameFromX509Token((X509SecurityToken)securityToken);
            }
            if (securityToken is Saml2SecurityToken)
            {
                return GetIssuerNameFromSaml2Token((Saml2SecurityToken)securityToken);
            }

            // Any other tokens would be rejected:
            throw new SecurityTokenException("Untrusted issuer.");
        }

        private string GetIssuerNameFromX509Token(X509SecurityToken token)
        {
            // 1. Check the certificate
            validator.Validate(token.Certificate);
            // Throws a SecurityTokenValidationException if the certificate is 
            // not valid

            // 2. Check the token
            if (token.ValidFrom > DateTime.Now || token.ValidTo < DateTime.Now)
            {
                throw new SecurityTokenException("Token is not valid.");
            }

            return token.Certificate.Issuer;
        }

        private static string GetIssuerNameFromSaml2Token(Saml2SecurityToken token)
        {
            throw new NotImplementedException();
        }

        public override string GetIssuerName(SecurityToken securityToken, string requestedIssuerName)
        {
            return requestedIssuerName;
        }
    }
}
