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

namespace STARTUnitTests.classes
{
	public class Parameter
	{
        //Parameters config.
        public static string smlDomain = "sml.peppolcentral.org";
        public static string senderValue = "9902:dk87654321";
        public static string recipientValue = "9902:dk87654321";
        public static string businessIdScheme = "iso6523-actorid-upis";
        public static string documentIdScheme = "busdox-docid-qns";
        public static string documentIdValue = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2::Invoice##urn:www.cenbii.eu:transaction:biicoretrdm010:ver1.0:#urn:www.peppol.eu:bis:peppol4a:ver1.0::2.0";
        public static string processIdValue = "urn:www.cenbii.eu:profile:bii04:ver1.0";
        public static string processIdScheme = "cenbii-procid-ubl";

        //Endpoint config.
        public static string endpointConfigurationName = "SecurePeppolClient";
        //Make sure your access point is deployed on IIS.
        public static Uri endpointAddress = new Uri("https://192.168.1.40:443/start-ap/accessPointService.svc");
        public static int assuranceLevel = 3;

        //Certificates config.
        public static string clientCertificatePath = @"D:\Certificates\keystore.pfx";
        public static string clientCertificatePwd = "password";
        public static string serviceCertificatePath = @"D:\Certificates\truststore.pfx";
        public static string serviceCertificatePwd = "password";
	}
}
