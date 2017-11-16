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

using STARTLibrary.src.eu.peppol.start.common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace STARTUnitTests
{
    /// <summary>
    ///This is a test class for Helper.cs in STARTLibrary. 
    ///It contains basic methods for participant lookup
    ///</summary>
    [TestClass()]
    public class MainMethodsTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for BuildSmpUrl
        ///</summary>
        [TestMethod()]
        public void BuildSmpUrlTest()
        {
            Helper target = new Helper();
            string recipientParticipant = "9902:dk87654321";
            string businessIdScheme = "iso6523-actorid-upis";
            string smlDomain = "sml.peppolcentral.org";
            string actual;
            actual = target.BuildSmpUrl(recipientParticipant, businessIdScheme, smlDomain);
        }

        /// <summary>
        ///A test for BuildSmpSignedServiceUrl
        ///</summary>
        [TestMethod()]
        public void BuildSmpSignedServiceUrlTest()
        {
            Helper target = new Helper();
            string recipientParticipant = "9902:dk87654321";
            string businessIdScheme = "iso6523-actorid-upis";
            string smlDomain = "sml.peppolcentral.org";
            string documentIdScheme = "busdox-docid-qns";
            string documentIdValue = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2::Invoice##urn:www.cenbii.eu:transaction:biicoretrdm010:ver1.0:#urn:www.peppol.eu:bis:peppol4a:ver1.0::2.0";
            string actual;
            actual = target.BuildSmpSignedServiceUrl(recipientParticipant, businessIdScheme, smlDomain, documentIdScheme, documentIdValue);
        }

        /// <summary>
        ///A test for GetEndPointAddress
        ///</summary>
        [TestMethod()]
        public void GetEndPointAddressTest()
        {
            Helper target = new Helper();
            string recipientParticipant = "9902:dk87654321";
            string businessIdScheme = "iso6523-actorid-upis";
            string smlDomain = "sml.peppolcentral.org";
            string documentIdScheme = "busdox-docid-qns";
            string documentIdValue = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2::Invoice##urn:www.cenbii.eu:transaction:biicoretrdm010:ver1.0:#urn:www.peppol.eu:bis:peppol4a:ver1.0::2.0";
            string actual;
            actual = target.GetEndPointAddress(recipientParticipant, businessIdScheme, smlDomain, documentIdScheme, documentIdValue);
        }

        /// <summary>
        ///A test for GetMD5Hash
        ///</summary>
        [TestMethod()]
        public void GetMD5HashTest()
        {
            Helper target = new Helper();
            string ParticipantIdentifier = "9902:dk87654321";
            string actual;
            actual = target.GetMD5Hash(ParticipantIdentifier);
        }

        /// <summary>
        ///A test for GetClientCertificate
        ///</summary>
        [TestMethod()]
        public void GetClientCertificateTest()
        {
            Helper target = new Helper();
            string recipientParticipant = "9902:dk87654321";
            string businessIdScheme = "iso6523-actorid-upis";
            string smlDomain = "sml.peppolcentral.org";
            string documentIdScheme = "busdox-docid-qns";
            string documentIdValue = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2::Invoice##urn:www.cenbii.eu:transaction:biicoretrdm010:ver1.0:#urn:www.peppol.eu:bis:peppol4a:ver1.0::2.0";
            string actual;
            actual = target.GetClientCertificate(recipientParticipant, businessIdScheme, smlDomain, documentIdScheme, documentIdValue);
        }
    }
}
