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
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.IdentityModel.Protocols.WSIdentity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.SecurityTokenService;
using System.Security.Cryptography.X509Certificates;

namespace STARTLibrary.src.eu.peppol.start.security.handler
{
    /// <summary>
    /// This class generates a SAML 2.0 security token for the client
    /// </summary>
    public static class AccessPointToken
    {
        public static ClaimsIdentity MakeClaims(string subjectName, int assuranceLevel)
        {
            if (assuranceLevel < 1 || assuranceLevel > 4)
            {
                throw new Exception("assuranceLevel must be between 1 and 4");
            }

            Claim identityClaim = new Claim("urn:eu:busdox:attribute:assurance-level", assuranceLevel.ToString());

            identityClaim.Properties[ClaimProperties.SamlAttributeNameFormat] = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic";
            ClaimsIdentity id = new ClaimsIdentity(new[] { identityClaim });

            // Sets the saml 'NameID' element within the 'Subject' element
            Claim nameIdentifier = new Claim(WSIdentityConstants.ClaimTypes.NameIdentifier, subjectName);
            nameIdentifier.Properties[ClaimProperties.SamlNameIdentifierFormat] = "http://busdox.org/profiles/serviceMetadata/1.0/UniversalBusinessIdentifier/1.0/";
            id.Claims.Add(nameIdentifier);

            return id;
        }
                
        public static Saml2SecurityToken ClaimsToSaml2SenderVouchesToken(ClaimsIdentity claimsId, string issuerUrl, X509Certificate2 cert)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = claimsId,
                TokenIssuerName = issuerUrl,
                Lifetime = new Lifetime(DateTime.Now.ToUniversalTime() - TimeSpan.FromHours(12), DateTime.Now.ToUniversalTime() + TimeSpan.FromHours(12)),
                SigningCredentials = new X509SigningCredentials(cert, SecurityAlgorithms.RsaSha1Signature, SecurityAlgorithms.Sha1Digest)
            };
            descriptor.AddAuthenticationClaims(Saml2Constants.AuthenticationContextClasses.X509.ToString(), DateTime.Now.ToUniversalTime());
            Saml2SecurityToken token = new AccessPointTokenHandler().CreateToken(descriptor) as Saml2SecurityToken;
            
            return token;
        }
    }
}
