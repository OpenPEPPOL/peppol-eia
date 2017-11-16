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
using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;

namespace STARTLibrary.src.eu.peppol.start.security.handler
{
    class AccessPointTokenHandler : Saml2SecurityTokenHandler
    {
        /// <summary>
        /// Create a security token (for the client)
        /// </summary>        
        public override SecurityToken CreateToken(SecurityTokenDescriptor tokenDescriptor)
        {
            var t = base.CreateToken(tokenDescriptor) as Saml2SecurityToken;
            t.Assertion.Id = new Saml2Id("SamlID1265272040703");
            return t;
        }

        /// <summary>
        /// Create a issuer name identifier (for the client)
        /// </summary>  
        protected override Saml2NameIdentifier CreateIssuerNameIdentifier(SecurityTokenDescriptor tokenDescriptor)
        {
            var t = base.CreateIssuerNameIdentifier(tokenDescriptor);
            t.Format = new Uri("urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified");
            return t;
        }

        /// <summary>
        /// Create a saml subject (for the client)
        /// </summary>  
        protected override Saml2Subject CreateSamlSubject(SecurityTokenDescriptor tokenDescriptor)
        {
            var t = base.CreateSamlSubject(tokenDescriptor);
            t.SubjectConfirmations[0].Method = Saml2Constants.ConfirmationMethods.SenderVouches;
            return t;
        }
    }
}
