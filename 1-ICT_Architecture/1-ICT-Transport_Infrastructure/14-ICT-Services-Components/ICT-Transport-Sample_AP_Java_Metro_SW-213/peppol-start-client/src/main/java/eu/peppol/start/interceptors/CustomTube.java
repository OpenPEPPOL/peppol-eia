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
package eu.peppol.start.interceptors;

import eu.peppol.start.client.validators.OCSPClientValidator;
import com.sun.xml.ws.api.message.Message;
import com.sun.xml.ws.api.message.Packet;
import com.sun.xml.ws.api.pipe.NextAction;
import com.sun.xml.ws.api.pipe.Tube;
import com.sun.xml.ws.api.pipe.TubeCloner;
import com.sun.xml.ws.api.pipe.helper.AbstractFilterTubeImpl;
import eu.peppol.start.client.util.SendProcessUtil;
import eu.peppol.start.util.Util;
import java.util.Iterator;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import org.apache.log4j.Logger;

/**
 * This class is an implementation of a Custom Tube to intercept a response message
 * to execute validations.
 * @author Jan Victoir (jan@alfa1lab.com)
 */
final class CustomTube extends AbstractFilterTubeImpl {

    /**
     * Logger to follow the class behavior.
     */
    private static final Logger logger4J = Logger.getLogger(CustomTube.class.getName());
    /**
     * Boolean variable to define a flag.
     */
    private boolean readyToGetCertificate = false;
    /**
     * Verify the times a sequence is executed
     */
    private int times = 0;

    /**
     * Enum structure defining a Client and an Endpoint to retrieve their data.
     */
    static enum Side {

        Client,
        Endpoint
    }
    /**
     * Declaration of the structure defined previously.
     */
    private final Side side;

    /**
     * Prints logging in debug mode.
     * @param logMessage 
     */
    private void printDebugLogging(String logMessage) {
        if (logger4J.isDebugEnabled()) {
            logger4J.debug(logMessage);
        }
    }

    /**
     * Private constructor to avoid instance. Initialize side attribute and
     * inherits from super class.
     * @param original
     *              CustomTube from a context. 
     * @param cloner 
     *              Variable defined to specify if context is request or response.
     */
    private CustomTube(CustomTube original, TubeCloner cloner) {
        super(original, cloner);
        this.side = original.side;
    }

    /**
     * Override parent.
     * @param cloner
     * @return 
     */
    @Override
    public CustomTube copy(TubeCloner cloner) {
        return new CustomTube(this, cloner);
    }

    /**
     * Default constructor required to initialize from another class in same package.
     * @param tube
     * @param side 
     */
    CustomTube(Tube tube, Side side) {
        super(tube);
        this.side = side;
    }

    /**
     * Override parent.
     * @param request
     * @return 
     */
    @Override
    public NextAction processRequest(Packet request) {
        printDebugLogging(String.format("Message request intercepted on %s side", side));
        return super.processRequest(request);
    }

    /**
     * Override parent.
     * @param response
     * @return 
     */
    @Override
    public NextAction processResponse(Packet response) {
        printDebugLogging(String.format("Message response intercepted on %s side", side));
        try {
            dumpMessage(response);
        } catch (Exception ex) {
            logger4J.error("Exception ocurred while processing response: " + ex.getMessage());
            SendProcessUtil.terminateSendingProcess();;
        }
        return super.processResponse(response);
    }

    /**
     * Print a certificate as String.
     * @param xmlr
     *              Stream in XML format.
     * @return 
     *              XML content as String.
     */
    private String printText(XMLStreamReader xmlr) {
        String certificateString = null;
        if (xmlr.hasText()) {
            certificateString = xmlr.getText();            
        }
        return certificateString;
    }

    /**
     * Specify if the XML content contains data and if matches a constant.
     * @param xmlr
     *              Stream in XML format.
     * @return 
     *              Boolean as a flag.
     */
    private boolean printStartElement(XMLStreamReader xmlr) {
        String startElement = null;
        boolean found = false;
        if (xmlr.isStartElement()) {
            startElement = xmlr.getName().toString();
            String attr = printAttributes(xmlr);
            if (attr.contains("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary")) {
                found = true;
                readyToGetCertificate = true;                
            }
        }
        return found;
    }

    /**
     * Prints the attributes of XML Content.
     * @param xmlr
     *              Stream in XML format.
     * @return 
     *              Attributes list as String.
     */
    private String printAttributes(XMLStreamReader xmlr) {
        StringBuffer attrStringBuffer = new StringBuffer();
        int count = xmlr.getAttributeCount();

        if (count > 0) {
            for (int i = 0; i < count; i++) {
                attrStringBuffer.append(" ");
                attrStringBuffer.append(xmlr.getAttributeName(i).toString());
                attrStringBuffer.append("=");
                attrStringBuffer.append("\"");
                attrStringBuffer.append(xmlr.getAttributeValue(i));
                attrStringBuffer.append("\"");
            }
        }

        count = xmlr.getNamespaceCount();

        if (count > 0) {
            for (int i = 0; i < count; i++) {
                attrStringBuffer.append(" ");
                attrStringBuffer.append("xmlns");

                if (xmlr.getNamespacePrefix(i) != null) {
                    attrStringBuffer.append(":" + xmlr.getNamespacePrefix(i));
                }
                attrStringBuffer.append("=");
                attrStringBuffer.append("\"");
                attrStringBuffer.append(xmlr.getNamespaceURI(i));
                attrStringBuffer.append("\"");
            }
        }
        return attrStringBuffer.toString();
    }

    /**
     * Checks if certificate of server and client matches.
     * @param paramCert
     *                  Incoming certificate.
     * @throws Exception 
     *                  Generic Exception.
     */
    private void checkCertificate(String certParam) throws Exception {
        printDebugLogging("Processing check certificate validation...");
        String apSmlCert = SendProcessUtil.getApSmlCertificate().getPublicKey().toString();

        String apSmltCertString = SendProcessUtil.getApSmlCertificateString();
        String apSmltCertString2 = apSmltCertString.trim().replaceAll("\\r|\\n", "");
        printDebugLogging("Executing certificate validation...");

        if (!apSmltCertString2.contains(certParam)) {
            logger4J.error("ERROR: Certificate of recipient found in Metadata"
                    + "does not match the certificate used to sign the response");
            SendProcessUtil.terminateSendingProcess();
        }

    }

    /**
     * Executes the OCSP Check in for client.
     * @param certParam 
     *                  Certificate in X509 format as String.
     */
    private void OCSPcheckIn(String certParam) {
        if (certParam != null) {
            String serverCertificate = certParam.trim();

            serverCertificate = Util.formatStringCertificate(serverCertificate.trim());            
            OCSPClientValidator ocspClient = new OCSPClientValidator();
            ocspClient.prepareClientValidation(serverCertificate);
        }

    }

    /**
     * Follow a trace of the response to intercept it and retrieve data.
     * @param response
     *              Contents of the response packet.
     * @throws Exception 
     *              Generic Exception.
     */
    private void dumpMessage(Packet response) throws Exception {

        Message interceptMsg = response.getMessage();
        if (interceptMsg.hasHeaders()) {
            com.sun.xml.ws.api.message.HeaderList hl = interceptMsg.getHeaders();
            Iterator iter = hl.iterator();
            while (iter.hasNext()) {
                com.sun.xml.ws.message.stream.StreamHeader sh = (com.sun.xml.ws.message.stream.StreamHeader) iter.next();
                String localPart = sh.getLocalPart();
                if (localPart.equalsIgnoreCase("Security")) {
                    try {
                        XMLStreamReader xmlr = sh.readHeader();
                        int eventType = xmlr.getEventType();
                        boolean again = false;
                        String certi = "";
                        while (xmlr.hasNext()) {
                            eventType = xmlr.next();
                            again = printStartElement(xmlr);
                            certi = printText(xmlr);
                            if (readyToGetCertificate && eventType == 4) {
                                readyToGetCertificate = false;
                                checkCertificate(certi);
                                times++;
                                if (times == 3) {
                                    OCSPcheckIn(certi);
                                }
                            }
                        }                        
                    } catch (XMLStreamException e) {
                        logger4J.error("Exception found while reading XML Stream.");
                    }
                }
            }
        }
    }

    /**
     * Overrides parent.
     * @param throwable
     * @return 
     */
    @Override
    public NextAction processException(Throwable throwable) {
        logger4J.info(String.format("Message processing exception intercepted on %s side", side));
        return super.processException(throwable);
    }

    /**
     * Overrides parent.
     */
    @Override
    public void preDestroy() {
        try {
            printDebugLogging("preDestroy()");
        } finally {
            super.preDestroy();
        }
    }
}