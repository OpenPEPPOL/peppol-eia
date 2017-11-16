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

using System.Linq;
using System.Xml;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Selectors;
using System.ServiceModel.Security;
using System.IdentityModel.Tokens;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace STARTAccessPoint.eu.peppol.start.saml2handler
{
    class StartSaml2TokenSerializerAdapter : SecurityTokenSerializerAdapter
    {
        private readonly StartSaml2SecurityTokenHandler samlHandler;

        public StartSaml2TokenSerializerAdapter(SecurityTokenHandlerCollection securityTokenHandlerCollection,
            SecurityVersion securityVersion)
            : base(securityTokenHandlerCollection, securityVersion)
        {
            samlHandler = new StartSaml2SecurityTokenHandler() { Configuration = securityTokenHandlerCollection.Configuration };
        }

        protected override SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            return reader.LocalName == "Assertion" ?
                                           samlHandler.ReadToken(reader, tokenResolver) :
                                                  base.ReadTokenCore(reader, tokenResolver);
        }

        protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader)
        {
            var xmlWasModified = false;
            var xmlText = reader.ReadOuterXml();

            var xml = XElement.Parse(xmlText);
            if (xml.ContainsKeyIdentifier() && xml.MissesTokenType())
            {
                // See long comment at the end of the file for an explanation
                xml.AddMissingTokenType();
                xmlWasModified = true;
            }

            // ReadOuterXml() advances the reader, so we have to get a new reader
            // even if the xml was not modified:
            using (reader = xmlWasModified ? xml.CreateReader() :
                                             XmlReader.Create(new System.IO.StringReader(xmlText)))
            {
                return base.ReadKeyIdentifierClauseCore(reader);
            }
        }
    }

    /// <summary>
    /// Methods to add this attribute 
    ///   b:TokenType="http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0" 
    /// to element SecurityTokenReference.
    /// </summary>
    internal static class XElementExtensions
    {
        private static readonly XName keyIdentifier = XName.Get("KeyIdentifier", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
        private static readonly XName tokenType = XName.Get("TokenType", "http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd");
        private static readonly string expectedTokenTypeValue = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";

        internal static bool ContainsKeyIdentifier(this XElement xml)
        {
            return xml.Elements(keyIdentifier).Count() == 1;
        }

        internal static bool MissesTokenType(this XElement xml)
        {
            return xml.Attributes(tokenType).Count() == 0;
        }

        internal static void AddMissingTokenType(this XElement xml)
        {
            xml.Add(new XAttribute(tokenType, expectedTokenTypeValue));
        }
    }

    class StartSaml2SecurityTokenHandler : Saml2SecurityTokenHandler
    {
        protected override ReadOnlyCollection<SecurityKey> ResolveSecurityKeys(Saml2Assertion assertion, SecurityTokenResolver resolver)
        {
            return new List<SecurityKey>() { assertion.SigningCredentials.SigningKey }.AsReadOnly();
        }
    }

    // Messages from Java clients may cause this exception:
    //
    //   "Cannot read KeyIdentifierClause from element 'KeyIdentifier' with namespace 
    //   'http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'. 
    //    Custom KeyIdentifierClauses require custom SecurityTokenSerializers, please refer 
    //    to the SDK for examples."
    //   at System.ServiceModel.Security.WSSecurityJan2004.SecurityTokenReferenceJan2004ClauseEntry.ReadKeyIdentifierClauseCore(XmlDictionaryReader reader) 
    //   at System.ServiceModel.Security.WSSecurityTokenSerializer.ReadKeyIdentifierClauseCore(XmlReader reader) 
    //   at Microsoft.IdentityModel.Tokens.SecurityTokenSerializerAdapter.ReadKeyIdentifierClauseCore(XmlReader reader) 
    //   at eu.peppol.start.services.StartSaml2Adap.ReadKeyIdentifierClauseCore(XmlReader reader) ...
    // 
    // Method SecurityTokenReferenceJan2004ClauseEntry.ReadKeyIdentifierClauseCore() uses the 
    // TokenType to find the StrEntry which can read the KeyIdentifierClause. If the TokenType
    // is missing, the exception is raised.
    //
    // According to the SAML Token Profile 1.1:
    //  "Every key identifier, direct, or embedded reference to a SAML assertion SHOULD contain
    //   a wsse11:TokenType attribute and the value of this attribute MUST be the value [...]
    //     V1.1     http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1
    //     V2.0     http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0
    //   that identifies the type and version of the referenced security token. When the 
    //   referenced assertion is a SAML V2.0 Assertion the reference MUST contain a 
    //   wsse11:TokenType attribute.

    // The problem is that the XML generated by some Java clients does not contain this 
    // TokenType:
    //
    //  From a Java client:
    //    <wsse:SecurityTokenReference 
    //        xmlns:ns16="http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512" 
    //        xmlns:ns15="http://www.w3.org/2003/05/soap-envelope" 
    //        wsu:Id="uuid_ac4774e6-f6e6-4cae-a729-343cf02fa0d7">
    //      <wsse:KeyIdentifier 
    //          ValueType="http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID">SamlID1295348474805</wsse:KeyIdentifier>
    //    </wsse:SecurityTokenReference>
    //
    // From a .NET client:
    //    <o:SecurityTokenReference 
    //         b:TokenType="http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0" 
    //         xmlns:b="http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd"
    //         u:Id="uuid-3c344eee-0c8d-4ccf-a87f-f7a9d992e63a-2" >
    //      <o:KeyIdentifier 
    //           ValueType="http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID">SamlID1265272040703</o:KeyIdentifier>
    //    </o:SecurityTokenReference>
    //
    // The custom StartSaml2TokenSerializerAdapter fixes this: Its ReadKeyIdentifierClauseCore() 
    // method adds the missing TokenType to the XML before passing it to the base class.
}


