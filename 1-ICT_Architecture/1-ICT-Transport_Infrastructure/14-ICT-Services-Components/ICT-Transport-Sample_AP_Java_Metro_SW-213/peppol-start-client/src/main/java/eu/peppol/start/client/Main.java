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
package eu.peppol.start.client;

import eu.peppol.start.client.util.MessageSetup;
import eu.peppol.start.client.util.SendProcessUtil;
import eu.peppol.start.exception.AccessPointClientException;
import eu.peppol.start.soap.SOAPHeaderObject;
import eu.peppol.start.metadata.MessageMetadata;
import org.w3c.dom.Document;
import java.security.cert.CertificateException;

/**
 * The accessPointClient class aims to hold all the processes required for
 * consuming an AccessPoint.
 *
 * @author  Jose Gorvenia Narvaez(jose@alfa1lab.com)
 *          Marcelo Tataje Salinas(marcelo@alfa1lab.com)
 *          George Reátegui Ravina (jorge@alfa1lab.com)
 * 
 */
public class Main {

    private static org.apache.log4j.Logger logger4J = org.apache.log4j.Logger.getLogger(Main.class);
    private static String XML_FILE_PATH = null;
    private static String DOCUMENT_IDENTIFIER_TYPE_VALUE = null;
    private static String DOCUMENT_IDENTIFIER_TYPE_SCHEME = null;
    private static String PROCESS_IDENTIFIER_TYPE_VALUE = null;
    private static String PROCESS_IDENTIFIER_TYPE_SCHEME = null;
    private static String PARTICIPANT_IDENTIFIER_SCHEME = null;
    private static String SENDER_PARTICIPANT_IDENTIFIER_VALUE = null;
    private static String RECEIVER_PARTICIPANT_IDENTIFIER_VALUE = null;

    /**
     * Main method which executes the application and send the message.
     * @param args
     *          Arguments for the application. Must be set when invoking the
     *          application.
     * @throws AccessPointClientException
     *          Thrown if an issue is found while running process in Client Side.
     * @throws CertificateException 
     *          Thrown if an issue is found while working with certificates.
     */
    public static void main(String[] args) throws AccessPointClientException, CertificateException {
        
        /*Setting arguments for .jar version.*/    	
    	SENDER_PARTICIPANT_IDENTIFIER_VALUE = args[0];
        RECEIVER_PARTICIPANT_IDENTIFIER_VALUE = args[1];
        DOCUMENT_IDENTIFIER_TYPE_VALUE = args[2];
        PROCESS_IDENTIFIER_TYPE_VALUE = args[3];
        XML_FILE_PATH = args[4];
        
        String recipientAPUrl = null;
        Document document = null;
        Document smpDocument = null;
        SOAPHeaderObject soapHeaderbject = null;
        SendProcessUtil.readConfigurationProperties();
        boolean isRedirect = false;

        MessageMetadata metadata = new MessageMetadata();
        metadata.setRecipientScheme(PARTICIPANT_IDENTIFIER_SCHEME);
        metadata.setRecipientValue(RECEIVER_PARTICIPANT_IDENTIFIER_VALUE);
        metadata.setDocumentIdScheme(DOCUMENT_IDENTIFIER_TYPE_SCHEME);
        metadata.setDocumentIdValue(DOCUMENT_IDENTIFIER_TYPE_VALUE);
        metadata.setProcessIdScheme(PROCESS_IDENTIFIER_TYPE_SCHEME);
        metadata.setProcessIdValue(PROCESS_IDENTIFIER_TYPE_VALUE);
        metadata.setSenderScheme(PARTICIPANT_IDENTIFIER_SCHEME);
        metadata.setSenderValue(SENDER_PARTICIPANT_IDENTIFIER_VALUE);        
        document = MessageSetup.setUpMessage(document, XML_FILE_PATH);
        
        smpDocument = SendProcessUtil.getSMPdata(metadata);
        MessageSetup.executeSMPVerification(smpDocument);
        logger4J.info("SMP Signature has been verified. Status: Valid.");  
        isRedirect = MessageSetup.isSMPRedirection(smpDocument);
        if (isRedirect) {
            logger4J.info("The AP prepares to execute a sending process with "
                    + "SMP redirection.");           
            recipientAPUrl = MessageSetup.AP_REDIRECT_URL;            
        } else {
            logger4J.info("The AP prepares to execute a sending process "
                    + "using SMP direct access.");          
            recipientAPUrl = SendProcessUtil.getAccessPointAddress(metadata);            
        }
        soapHeaderbject = MessageSetup.setUpHeaders(soapHeaderbject, metadata);
        SendProcessUtil.prepareCertificatesValidation(metadata, isRedirect);
        SendProcessUtil.sendMessage(soapHeaderbject, recipientAPUrl, SendProcessUtil.getApSmlCertificate(), document);
    }
}
