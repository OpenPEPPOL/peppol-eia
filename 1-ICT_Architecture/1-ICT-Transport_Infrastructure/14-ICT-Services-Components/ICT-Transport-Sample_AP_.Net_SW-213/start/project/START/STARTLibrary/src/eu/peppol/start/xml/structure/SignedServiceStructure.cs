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
using System.Xml.Serialization;
using System.Xml;

namespace STARTLibrary.src.eu.peppol.start.xml.structure
{
    [XmlRoot(Namespace = "http://busdox.org/serviceMetadata/publishing/1.0/")]
    public class SignedServiceMetadata
    {
        [XmlElement(Namespace = "http://busdox.org/serviceMetadata/publishing/1.0/")]
        public ServiceMetadata ServiceMetadata
        {
            get;
            set;
        }
        [XmlElement(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
        public Signature Signature
        {
            get;
            set;
        }
    }

    #region ServiceMetaData

    [XmlRoot(Namespace = "http://busdox.org/serviceMetadata/publishing/1.0/")]
    public partial class ServiceMetadata
    {
        [XmlElement]
        public Redirect Redirect
        {
            get;
            set;
        }

        [XmlElement]
        public ServiceInformation ServiceInformation
        {
            get;
            set;
        }
    }

    public partial class Redirect
    {
        [XmlElement(Namespace = "http://busdox.org/serviceMetadata/publishing/1.0/")]
        public CertificateUID CertificateUID
        {
            get;
            set;
        }

        [XmlAttribute("href")]
        public string href
        {
            get;
            set;
        }
    }

    public partial class CertificateUID
    {
        [XmlText]
        public string Text
        {
            get;
            set;
        }
    }

    public partial class ServiceInformation
    {
        [XmlElement(Namespace = "http://busdox.org/transport/identifiers/1.0/")]
        public DocumentIdentifier DocumentIdentifier
        {
            get;
            set;
        }

        [XmlElement]
        public ProcessList ProcessList
        {
            get;
            set;
        }
    }

    public partial class DocumentIdentifier
    {
        [XmlAttribute("scheme")]
        public string scheme
        {
            get;
            set;
        }

        [XmlText]
        public string Text
        {
            get;
            set;
        }
    }

    public partial class ProcessList
    {
        [XmlElement]
        public Process[] Process
        {
            get;
            set;
        }
    }

    #region ProcessMethods

    public class Process
    {
        [XmlElement(Namespace = "http://busdox.org/transport/identifiers/1.0/")]
        public ProcessIdentifier ProcessIdentifier
        {
            get;
            set;
        }
        [XmlElement]
        public ServiceEndpointList ServiceEndpointList
        {
            get;
            set;
        }
    }

    public class ProcessIdentifier
    {
        [XmlAttribute("scheme")]
        public string scheme
        { get; set; }

        [XmlText]
        public string Name
        { get; set; }
    }

    public class ServiceEndpointList
    {
        [XmlElement]
        public EndPoint Endpoint
        {
            get;
            set;
        }
    }

    public class EndPoint
    {
        [XmlAttribute]
        public string transportProfile
        { get; set; }

        [XmlElement(ElementName = "EndpointReference", Namespace = "http://www.w3.org/2005/08/addressing")]
        public SMPEndpointReference EndpointReference
        {
            get;
            set;
        }

        [XmlElement]
        public string RequireBusinessLevelSignature
        {
            get;
            set;
        }

        [XmlElement]
        public string MinimumAuthenticationLevel
        {
            get;
            set;
        }

        [XmlElement]
        public string ServiceActivationDate
        {
            get;
            set;
        }

        [XmlElement]
        public string ServiceExpirationDate
        {
            get;
            set;
        }

        [XmlElement]
        public string Certificate
        {
            get;
            set;
        }

        [XmlElement]
        public string ServiceDescription
        {
            get;
            set;
        }

        [XmlElement]
        public string TechnicalContactUrl
        {
            get;
            set;
        }

        [XmlElement]
        public string TechnicalInformationUrl
        {
            get;
            set;
        }
    }

    public class SMPEndpointReference
    {
        [XmlElement(Namespace = "http://www.w3.org/2005/08/addressing")]
        public string Address
        {
            get;
            set;
        }
    }
    #endregion
    #endregion

    #region Signature

    public partial class Signature
    {

        [XmlElement]
        public SignedInfo SignedInfo
        {
            get;
            set;
        }

        [XmlElement]
        public string SignatureValue
        {
            get;
            set;
        }
        [XmlElement]
        public KeyInfo KeyInfo
        {
            get;
            set;
        }
    }

    #region SignatureMethod

    public partial class SignedInfo
    {
        [XmlElement]
        public CanonicalizationMethod CanonicalizationMethod
        {
            get;
            set;
        }

        [XmlElement]
        public SignatureMethod SignatureMethod
        {
            get;
            set;
        }

        [XmlElement]
        public Reference Reference
        {
            get;
            set;
        }

    }

    public partial class CanonicalizationMethod
    {
        [XmlAttribute]
        public string Algorithm
        {
            get;
            set;
        }
    }

    public partial class SignatureMethod
    {
        [XmlAttribute]
        public string Algorithm
        {
            get;
            set;
        }
    }

    public partial class Reference
    {
        [XmlAttribute]
        public string URI
        {
            get;
            set;
        }

        [XmlElement]
        public Transforms Transforms
        {
            get;
            set;
        }

        [XmlElement]
        public DigestMethod DigestMethod
        {
            get;
            set;
        }

        [XmlElement]
        public string DigestValue
        {
            get;
            set;
        }
    }

    public partial class Transforms
    {
        public Transform Transform
        {
            get;
            set;
        }
    }
    public partial class Transform
    {
        [XmlAttribute]
        public string Algorithm
        {
            get;
            set;
        }
    }

    public partial class DigestMethod
    {
        [XmlAttribute]
        public string Algorithm
        {
            get;
            set;
        }
    }

    #region KeyInfo

    public partial class KeyInfo
    {
        [XmlElement]
        public X509Data X509Data
        {
            get;
            set;
        }
    }

    public partial class X509Data
    {
        [XmlElement]
        public string X509SubjectName
        {
            get;
            set;
        }

        [XmlElement]
        public string X509Certificate
        {
            get;
            set;
        }
    }

    #endregion
    #endregion
    #endregion

}