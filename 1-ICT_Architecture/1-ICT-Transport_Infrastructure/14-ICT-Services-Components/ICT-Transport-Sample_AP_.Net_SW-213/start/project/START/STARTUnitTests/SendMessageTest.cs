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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Net;
using STARTUnitTests.classes;
using STARTLibrary.accesspointService;
using STARTLibrary.src.eu.peppol.start.io;
using STARTLibrary.src.eu.peppol.start.impl;
using STARTLibrary.src.eu.peppol.start.security.impl;

namespace STARTUnitTests
{
    /// <summary>
    ///This is a test class for SimpleAccessPointClientTest and is intended
    ///to contain all SimpleAccessPointClientTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SendMessageTest
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
        ///A test for 'SendSecureMessage' to yourself.
        ///</summary>
        [TestMethod()]
        //Go to STARTUnitTest.classes for parameters and certificates config.
        public void SendSecureMessage()
        {
            //SSL Callback
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, error) =>
            {
                return true;
            };

            string endpointConfigurationName = Parameter.endpointConfigurationName;
            int assuranceLevel = Parameter.assuranceLevel;
            string smlDomain = "sml.peppolcentral.org";
            Uri thisUri = Parameter.endpointAddress;
            X509Certificate2 clientCertificate = Certificate.ClientCertificate;
            X509Certificate2 serviceCertificate = Certificate.ServiceCertificate;
            SimpleAccessPointClient target = new SimpleAccessPointClient(endpointConfigurationName, assuranceLevel, smlDomain, thisUri, clientCertificate, serviceCertificate);

            IMessageMetadata metadata = new MessageMetadata();
            metadata.RecipientIdentifier = new ParticipantIdentifierType();
            metadata.RecipientIdentifier.Value = Parameter.recipientValue;
            metadata.RecipientIdentifier.scheme = Parameter.businessIdScheme;
            metadata.SenderIdentifier = new ParticipantIdentifierType();
            metadata.SenderIdentifier.Value = Parameter.senderValue;
            metadata.SenderIdentifier.scheme = Parameter.businessIdScheme;
            metadata.DocumentIdentifier = new DocumentIdentifierType();
            metadata.DocumentIdentifier.Value = Parameter.documentIdValue;
            metadata.DocumentIdentifier.scheme = Parameter.documentIdScheme;
            metadata.ProcessIdentifier = new ProcessIdentifierType();
            metadata.ProcessIdentifier.Value = Parameter.processIdValue;
            metadata.ProcessIdentifier.scheme = Parameter.processIdScheme;
            metadata.ChannelIdentifier = Parameter.senderValue;

            Random rnd = new Random((int)DateTime.Now.Ticks);
            metadata.MessageIdentifier = "uuid:" + Guid.NewGuid().ToString("D");

            XmlDocument body = new XmlDocument();
            body.LoadXml("<?xml version='1.0' encoding='UTF-8'?><s:Test xmlns:s=\"http://test.dk\">"
             + "Message sent through 'SendMessage' unit test" + "</s:Test>");

            string actual = null;

            //Go to TestResults/Out folder to look TraceLog and SOAP messages.
            actual = target.Send(metadata, body);
        }
    }
}
