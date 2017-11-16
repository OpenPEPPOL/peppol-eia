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

import eu.peppol.start.client.accessPointClient;
import eu.peppol.start.exception.DocumentTypeNotAcceptedException;
import eu.peppol.start.exception.LookupException;
import eu.peppol.start.exception.UnknownEndpointException;
import eu.peppol.start.metadata.MessageMetadata;
import eu.peppol.start.sml.SMLLookup;
import eu.peppol.start.soap.SOAPHeaderObject;
import eu.peppol.start.util.Configuration;
import eu.peppol.start.util.Util;
import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import java.util.List;
import org.apache.log4j.Logger;
import org.w3._2009._02.ws_tra.Create;
import org.w3._2009._02.ws_tra.Resource;
import org.w3c.dom.Document;

/**
 * Utility class to execute lookups and validations in client side.
 * @author Marcelo Tataje Salinas(marcelo@alfa1lab.com)
 */
public final class SendProcessUtil {

    /**
     * Logger to follow the class behavior.
     */
    private static Logger logger4J = Logger.getLogger(SendProcessUtil.class);
    /**
     * SML Address, required to retrieve url and uses its services.
     */
    private static String smlServiceAddress;
    /**
     * Configuration instance to access properties data.
     */
    private static Configuration conf;
    /**
     * The connecting port to execute Web Service Operations.
     */
    private static Resource port;
    /**
     * X509 Certificate from the receiver.
     */
    private static X509Certificate RECEIVER_CERTIFICATE = null;
    /**
     * Certificate in String format.
     */
    private static String CERTIFICATE_ENTRY = null;

    /**
     * Sole constructor to avoid instance.
     */
    private SendProcessUtil() {
    }

    /**
     * Reads the configuration file to retrieve values.
     */
    public static void readConfigurationProperties() {
        conf = Configuration.getInstance();
        smlServiceAddress = conf.getProperty("sml.service.address");
    }

    /**
     * Retrieve the destination endpoint address.
     * @param metadata as the data of participants and identifiers.
     * @return String data type with the endpoint address.
     */
    public static String getAccessPointAddress(MessageMetadata metadata)
            throws UnknownEndpointException, DocumentTypeNotAcceptedException, LookupException {

        SMLLookup sml = SMLLookup.getInstance();

        return sml.getEndpointAddress(smlServiceAddress,
                metadata.getRecipientScheme(),
                metadata.getRecipientValue(),
                metadata.getDocumentIdScheme(),
                metadata.getDocumentIdValue());
    }

    /**
     * Retrieve the certificate from the recipient Metadata.
     * @param metadata as the metadata of the recipient.
     * @param metadata as the data of participants and identifiers.
     * @return String data type with the endpoint address.
     */
    public static String getAccessPointCertificate(MessageMetadata metadata)
            throws UnknownEndpointException, DocumentTypeNotAcceptedException, LookupException {

        SMLLookup sml = SMLLookup.getInstance();

        String cert = sml.getEndpointCertificate(smlServiceAddress,
                metadata.getRecipientScheme(),
                metadata.getRecipientValue(),
                metadata.getDocumentIdScheme(),
                metadata.getDocumentIdValue(),
                metadata.getProcessIdScheme(),
                metadata.getProcessIdValue());

        return Util.completeCertificateEntry(cert);
    }

    /**
     * Retrieves the certificate from Signature from SMP
     * @param metadata
     *                  Metadata containing the certificate.
     * @return 
     *                  List with values.
     */
    public static Document getSMPdata(MessageMetadata metadata) {
        Document smpDocument = null;

        SMLLookup sml = SMLLookup.getInstance();

        smpDocument = sml.getDataFromSMP(smlServiceAddress,
                metadata.getRecipientScheme(),
                metadata.getRecipientValue(),
                metadata.getDocumentIdScheme(),
                metadata.getDocumentIdValue(),
                metadata.getProcessIdScheme(),
                metadata.getProcessIdValue());

        return smpDocument;
    }

    /**
     * Send a ping Message to an Access Point Service.
     * @param recipientAPUrl represents the address of the recipient Accesspoint.
     * @param metaCert represents the SMP certificate.
     */
    public static void sendPingMessage(Create body, String recipientAPUrl, X509Certificate metaCert) {
        accessPointClient client = accessPointClient.getInstance();
        MessageMetadata metadata = new MessageMetadata();
        client.setMetadataCertificate(metaCert);
        client.printSOAPLogging(true);
        Resource port = client.getPort(recipientAPUrl);
        logger4J.debug("Sending ping message...");
        client.send(port, metadata.getSoapHeader(), body);
        logger4J.debug("Ping Message Sent");
    }

    /**
     * Send a message to an Access Point Service.
     * @param recipientAPUrl represents the address of the recipient AccessPoint.
     * @param metaCert represents the SMP certificate.
     */
    public static void sendMessage(SOAPHeaderObject soapHeaderbject, String recipientAPUrl, X509Certificate receiverCert, Document document) {
        accessPointClient accessPointClient = null;
        /* Prepare Objects to Send a Message. */
        Create body = new Create();

        List<Object> objects = body.getAny();
        objects.add(document.getDocumentElement());

        accessPointClient = accessPointClient.getInstance();

        accessPointClient.setMetadataCertificate(receiverCert);
        accessPointClient.printSOAPLogging(true);
        port = accessPointClient.getPort(recipientAPUrl);

        logger4J.info("Sending message...");
        accessPointClient.send(port, soapHeaderbject, body);
        logger4J.info("Message Delivered to Endpoint: " + recipientAPUrl);
    }

    /**
     * Retrieve the data of x509 Certificate of AP.
     * @return 
     *          X509Certificate object.
     */
    public static X509Certificate getApSmlCertificate() {
        return RECEIVER_CERTIFICATE;
    }

    /**
     * Converts the contents of the certificate into a String to handle information.
     * @return 
     *          String containing certificate data.
     */
    public static String getApSmlCertificateString() {
        return CERTIFICATE_ENTRY;
    }

    /**
     * Set the values for the required parameters to execute Certificate Validation.
     * @param metadata
     *                  Metadata containing information of the process.
     * @throws CertificateException 
     *                  Exception thrown if any error with certificates is found.
     */
    public static void prepareCertificatesValidation(MessageMetadata metadata, boolean isRedirect)
            throws CertificateException {
        if(isRedirect) {
            CERTIFICATE_ENTRY = MessageSetup.REDIRECT_CERTIFICATE;                        
        } else {
            CERTIFICATE_ENTRY = getAccessPointCertificate(metadata);
        }        
        RECEIVER_CERTIFICATE = Util.generateX509Certificate(CERTIFICATE_ENTRY);
    }

    /**
     * Terminates the sending process before execute any action when a certificate is not valid.
     */
    public static void terminateSendingProcess() {
        logger4J.error("The process has been terminated since the certificates are not valid.");
        if (port != null) {
            logger4J.warn("Port for Create operation has been closed...");
            throw new RuntimeException("ERROR: Certificate of recipient found in Metadata"
                    + " does not match the certificate used to sign the response");
        }
    }
}
