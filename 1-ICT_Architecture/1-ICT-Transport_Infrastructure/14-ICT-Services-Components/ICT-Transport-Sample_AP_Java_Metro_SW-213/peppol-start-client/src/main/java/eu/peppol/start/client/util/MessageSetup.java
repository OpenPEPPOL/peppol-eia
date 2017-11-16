/*
 * Version: MPL 1.1/EUPL 1.1
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
 * http://joinup.ec.europa.eu/software/page/eupl/licence-eupl
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
 * the provisions above, a recipient may use your version of this file
 * under either the MPL or the EUPL License.
 */
package eu.peppol.start.client.util;

import eu.peppol.start.smp.SMPValidationManager;
import eu.peppol.start.metadata.MessageMetadata;
import eu.peppol.start.soap.SOAPHeaderObject;
import java.io.File;
import java.io.IOException;
import java.util.UUID;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import org.busdox.servicemetadata.types.DocumentIdentifierType;
import org.busdox.servicemetadata.types.ParticipantIdentifierType;
import org.busdox.servicemetadata.types.ProcessIdentifierType;
import org.w3c.dom.Document;
import org.xml.sax.SAXException;

/**
 * This class sets up the message and the required parameters to execute the sending process.
 * Marcelo Tataje Salinas(marcelo@alfa1lab.com)
 */
public class MessageSetup {

    /**
     * Prefix for id of messages.
     */
    private static String STANDARD_MESSAGE_ID_PREFIX = "uuid:";
    /**
     * Logger to follow the class behavior.
     */
    private static org.apache.log4j.Logger logger4J = org.apache.log4j.Logger.getLogger(MessageSetup.class);
    /**
     * Redirection URL value for SMP in redirection case.
     */
    public static String SMP_REDIRECTION_URL = null;
    /**
     * Redirection Url of AP Destination.
     */
    public static String AP_REDIRECT_URL = null;
    /**
     * Certificate entry of the redirect AP.
     */
    public static String REDIRECT_CERTIFICATE = null;
    /**
     * Sole constructor to avoid instance.
     */
    private MessageSetup() {
    }

    /**
     * This function takes the Message and transforms it into a different format with the same content
     * in order to be transmitted to the AP Service.
     * @param document.
     * @return document.
     */
    public static Document setUpMessage(Document document, String targetXmlFile) {

        String xmlFile = targetXmlFile;

        DocumentBuilderFactory domFactory = DocumentBuilderFactory.newInstance();

        DocumentBuilder parser = null;
        try {
            parser = domFactory.newDocumentBuilder();
            document = parser.parse(new File(xmlFile));
        } catch (SAXException ex) {
            logger4J.error("SAXException has been thrown, please "
                    + "verify if it is a well-formed document - MESSAGE: "
                    + ex.getMessage());
        } catch (IOException ex) {
            logger4J.error("An IO Exception occurred with the document, please "
                    + "verify if the document is valid or exists - MESSAGE: "
                    + ex.getMessage());
        } catch (ParserConfigurationException ex) {
            logger4J.error("The document cannot be parsed, please "
                    + "verify if it is a well-formed document - MESSAGE: "
                    + ex.getMessage());

        }

        return document;
    }

    /**
     * Set up the Headers needed for the Transaction.
     * @param shobject.
     * @return SOAPHeaderObject.
     */
    public static SOAPHeaderObject setUpHeaders(SOAPHeaderObject shobject, MessageMetadata metadata) {

        shobject = new SOAPHeaderObject();
        shobject.setChannelIdentifier(null);
        String messageID = STANDARD_MESSAGE_ID_PREFIX + UUID.randomUUID().toString();
        shobject.setMessageIdentifier(messageID);

        DocumentIdentifierType docid = new DocumentIdentifierType();
        docid.setValue(metadata.getDocumentIdValue());
        docid.setScheme(metadata.getDocumentIdScheme());

        shobject.setDocumentIdentifier(docid);

        ProcessIdentifierType proid = new ProcessIdentifierType();
        proid.setValue(metadata.getProcessIdValue());
        proid.setScheme(metadata.getProcessIdScheme());
        shobject.setProcessIdentifier(proid);

        ParticipantIdentifierType senId = new ParticipantIdentifierType();
        senId.setValue(metadata.getSenderValue());
        senId.setScheme(metadata.getSenderScheme());
        shobject.setSenderIdentifier(senId);

        ParticipantIdentifierType recId = new ParticipantIdentifierType();
        recId.setValue(metadata.getRecipientValue());
        recId.setScheme(metadata.getRecipientScheme());
        shobject.setRecipientIdentifier(recId);

        return shobject;
    }

    /**
     * Verifies the Signature incoming from the SMP.
     * @param smpCertificate 
     *              Certificate from the SMP located in Signature tag.
     */
    public static void executeSMPVerification(Document smpData) {
        boolean isSignatureValid = false;
        isSignatureValid = SMPValidationManager.executeSMPvalidations(smpData);
        if (!isSignatureValid) {
            logger4J.error("WARNING: A potential harm has been found - "
                    + "SMP Signature is not valid. End of Process.");
            throw new RuntimeException("WARNING: A potential harm has been found - "
                    + "SMP Signature is not valid. End of Process.");
        }
    }

    /**
     * Check if the SMP is redirecting to a different SMP.
     * @param smpData
     *              SMP Document.
     * @return 
     *              boolean value which contains the redirect status.
     */
    public static boolean isSMPRedirection(Document smpData) {
        boolean isRedirect = false;               
        String redirectTag = "ns3:Redirect";                
        isRedirect = SMPValidationManager.isSMPRedirect(smpData, redirectTag);        
        return isRedirect;
    }        
}
