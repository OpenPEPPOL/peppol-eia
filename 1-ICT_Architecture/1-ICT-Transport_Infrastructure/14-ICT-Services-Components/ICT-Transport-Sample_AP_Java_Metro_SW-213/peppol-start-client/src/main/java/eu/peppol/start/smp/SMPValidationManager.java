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
package eu.peppol.start.smp;

import eu.peppol.start.client.util.MessageSetup;
import eu.peppol.start.util.Configuration;
import eu.peppol.start.util.Util;
import java.io.BufferedOutputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.security.KeyStore;
import java.security.KeyStoreException;
import java.security.Security;
import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import java.util.logging.Level;
import javax.xml.crypto.KeySelector;
import javax.xml.crypto.MarshalException;
import javax.xml.crypto.dsig.XMLSignature;
import javax.xml.crypto.dsig.XMLSignatureException;
import javax.xml.crypto.dsig.XMLSignatureFactory;
import javax.xml.crypto.dsig.dom.DOMValidateContext;
import org.apache.log4j.Logger;
import org.bouncycastle.asn1.ocsp.OCSPResponseStatus;
import org.bouncycastle.jce.provider.BouncyCastleProvider;
import org.bouncycastle.ocsp.CertificateID;
import org.bouncycastle.ocsp.OCSPException;
import org.bouncycastle.ocsp.OCSPReq;
import org.bouncycastle.ocsp.OCSPReqGenerator;
import org.bouncycastle.ocsp.OCSPResp;
import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

/**
 * This class gives functionality for SMP validations and connections.
 * @author Marcelo Tataje(marcelo@alfa1lab.com)
 */
public class SMPValidationManager {

    /**
     * Logger to follow the class behavior.
     */
    private static Logger logger4J = Logger.getLogger(SMPValidationManager.class);
    /**
     * Trusted CN for SMP.
     */
    private static String TRUSTED_SMP_CN = "CN=PEPPOL SERVICE METADATA PUBLISHER";
    /**
     * 
     */
    private static String TEST_RESPONDER_URL = "http://pilot-ocsp.verisign.com:80";
    /**
     * Configuration instance to load the properties.
     */
    private static Configuration config = Configuration.getInstance();

    /*
     * Sole constructor to avoid instance of this class.
     */
    private SMPValidationManager() {
    }

    /**
     * This method executes all the validations for SMP Signature.
     * @param smpData
     *              The document obtained while connecting to SMP.
     * @return 
     *              Boolean status to indicate if the send process is allowed
     *              or it must be rollback due to the fail in SMP validation.
     */
    public static boolean executeSMPvalidations(Document smpData) {
        boolean flag = false;
        boolean isRevoked = false;
        boolean isSignatureValid = false;
        boolean hasRootTrusted = false;
        String smpCertificate = extractSMPCertificate(smpData);
        smpCertificate = Util.completeCertificateEntry(smpCertificate);

        isSignatureValid = checkSignature(smpData);

        if (isSignatureValid) {
            hasRootTrusted = verifyRootIssuers(smpCertificate);
            if (hasRootTrusted) {
                isRevoked = getRevocationStatus(smpCertificate);
                if (isRevoked) {
                    return flag;
                } else {
                    flag = true;
                }
            } else {
                return flag;
            }
        } else {
            return flag;
        }
        return flag;
    }

    /**
     * Validates date and begins the process of revocation validation.
     * @param certificate
     *                  Certificate in String format.
     * @return 
     *                  Boolean value to indicates validity of certificate.
     */
    private static boolean getRevocationStatus(String certificate) {
        boolean flag = false;
        X509Certificate smpSignCertificate = null;
        try {
            smpSignCertificate = Util.generateX509Certificate(certificate);
            smpSignCertificate.checkValidity();
            flag = validateRevocationStatus(smpSignCertificate);
        } catch (CertificateException ex) {
            logger4J.error("An exception has been found while processing certificate "
                    + "data. The process ends with message: " + ex.getMessage());
            throw new RuntimeException("An exception has been found while processing certificate "
                    + "data. The process ends with message: " + ex.getMessage());
        }
        return flag;
    }

    /**
     * Checks the signature of SMP to ensure valid SMP contents.
     * @param smpDoc
     *          Full document cotaining all the data of the SMP.
     * @return 
     *          Status result of the signature validation.
     */
    private static boolean checkSignature(Document smpDoc) {
        boolean flag = false;
        String smpStringCert = extractSMPCertificate(smpDoc);
        smpStringCert = Util.formatStringCertificate(smpStringCert.trim().replaceAll("\\r|\\n", ""));
        Node signature = null;
        X509Certificate smpSignCertificate = null;
        try {
            smpSignCertificate = Util.generateX509Certificate(smpStringCert);
            signature = (Node) smpDoc.getElementsByTagName("Signature").item(0);
            KeySelector ks = KeySelector.singletonKeySelector(smpSignCertificate.getPublicKey());
            DOMValidateContext domValidator = new DOMValidateContext(ks, signature);
            XMLSignatureFactory xmlFactory = XMLSignatureFactory.getInstance("DOM");
            XMLSignature xmlSign = xmlFactory.unmarshalXMLSignature(domValidator);
            flag = xmlSign.validate(domValidator);
            logger4J.debug("Signature status for x509Data is valid? : " + flag);
        } catch (XMLSignatureException ex) {
            logger4J.error("An exception has been found while checking and verifying "
                    + "the signature. The process ends with message: " + ex.getMessage());
            throw new RuntimeException("An exception has been found while checking "
                    + "and verifying the signature. The process ends with message: " + ex.getMessage());
        } catch (MarshalException ex) {
            logger4J.error("An exception has been found while marshalling the XML "
                    + "document. The process ends with message: " + ex.getMessage());
            throw new RuntimeException("An exception has been found while marshalling the XML "
                    + "document. The process ends with message: " + ex.getMessage());
        } catch (CertificateException ex) {
            logger4J.error("An exception has been found while processing certificate "
                    + "data. The process ends with message: " + ex.getMessage());
            throw new RuntimeException("An exception has been found while processing certificate "
                    + "data. The process ends with message: " + ex.getMessage());
        }

        return flag;
    }

    /**
     * Verifies if the certificate has been validated by a trusted issuer.
     * @param certificate
     *              Certificate obtained from x509Data of SMP as String..
     * @return 
     *              Status if the certificate is valid.
     */
    private static boolean verifyRootIssuers(String certificate) {
        boolean flag = false;
        X509Certificate smpSignCertificate = null;
        String trustedCN = null;
        try {
            smpSignCertificate = Util.generateX509Certificate(certificate);
            trustedCN = smpSignCertificate.getIssuerDN().getName();
            if (!trustedCN.contains(TRUSTED_SMP_CN)) {
                return flag;
            }
            flag = true;
        } catch (CertificateException ex) {
            java.util.logging.Logger.getLogger(SMPValidationManager.class.getName()).log(Level.SEVERE, null, ex);
        }

        return flag;
    }

    /**
     * Validates the revocation status of Certificate included in SMP Signature. 
     * @param targetCertificate
     *                  The SMP obtained from x509 data.
     * @return 
     *                  True if is revoked or invalid, false if is ok.
     */
    private static boolean validateRevocationStatus(X509Certificate targetCertificate) {
        boolean flag = false;
        Security.addProvider(new BouncyCastleProvider());
        OCSPReqGenerator ocspRequest = null;
        OCSPReq req = null;
        URL verifierUrl = null;
        X509Certificate trustedCA = null;
        try {
            KeyStore ks = Util.getKeystoreFromPath(config.getProperty("server.truststore"),
                    config.getProperty("server.truststore.password"));
            trustedCA = (X509Certificate) ks.getCertificate(config.getProperty("server.truststore.alias"));
            ocspRequest = new OCSPReqGenerator();
            ocspRequest.addRequest(new CertificateID(CertificateID.HASH_SHA1,
                    trustedCA, targetCertificate.getSerialNumber()));
            req = ocspRequest.generate();
            verifierUrl = new URL(TEST_RESPONDER_URL);
            HttpURLConnection connection = (HttpURLConnection) verifierUrl.openConnection();

            connection.setRequestProperty(config.getProperty("revocation.content.key"),
                    config.getProperty("revocation.content.value"));
            connection.setRequestProperty(config.getProperty("revocation.accept.key"),
                    config.getProperty("revocation.accept.value"));
            connection.setDoOutput(true);
            OutputStream out = connection.getOutputStream();
            DataOutputStream dataOut = new DataOutputStream(
                    new BufferedOutputStream(out));
            dataOut.write(req.getEncoded());
            dataOut.flush();
            dataOut.close();
            InputStream in = (InputStream) connection.getContent();
            OCSPResp response = new OCSPResp(in);
            int status = response.getStatus();

            if (status == OCSPResponseStatus.SUCCESSFUL) {
                logger4J.debug("OCSP for SMP Signature is successful.");
                return false;
            } else if (status == OCSPResponseStatus.TRY_LATER) {
                logger4J.debug("OCSP could not be executed, please try later or "
                        + "verify your connection.");
                flag = true;
            } else if (status == OCSPResponseStatus.MALFORMED_REQUEST) {
                logger4J.debug("OCSP Validation has found a malformed request.");
                flag = true;
            } else if (status == OCSPResponseStatus.INTERNAL_ERROR) {
                logger4J.debug("OCSP Validation has found an Internal Error "
                        + "in server");
                flag = true;
            } else if (status == OCSPResponseStatus.UNAUTHORIZED) {
                logger4J.debug("OCSP has found the Certificate is Revoked");
                flag = true;
            }

        } catch (KeyStoreException ex) {
            logger4J.error("KeystoreException found while executing validation: "
                    + ex.getMessage());
            throw new RuntimeException("KeystoreException found while executing "
                    + "validation: " + ex.getMessage());
        } catch (MalformedURLException ex) {
            logger4J.error("MalformedURLException found while executing validation: "
                    + ex.getMessage());
            throw new RuntimeException("MalformedURLException found while "
                    + "executing validation: " + ex.getMessage());
        } catch (OCSPException ex) {
            logger4J.error("OCSP Internal error found while executing validation: "
                    + ex.getMessage());
            throw new RuntimeException("OCSP Internal error found while executing "
                    + "validation: " + ex.getMessage());
        } catch (IOException ex) {
            logger4J.error("IO Internal error found while executing validation: "
                    + ex.getMessage());
            throw new RuntimeException("IO Internal error found while executing "
                    + "validation: " + ex.getMessage());
        }

        return flag;
    }

    /**
     * Retrieves the certificate of the Signature of SMP.
     * @param doc
     *          The incoming XML document from SMP.
     * @return 
     *          String containing the certificate value.
     */
    private static String extractSMPCertificate(Document doc) {
        String smpCert = null;
        doc.normalize();
        NodeList nodeList = doc.getChildNodes().item(0).getChildNodes();

        for (int i = 0; i < nodeList.getLength(); i++) {
            Node targetNode = nodeList.item(i);
            if (targetNode.getNodeName().equals("Signature")) {
                NodeList signatureList = targetNode.getChildNodes();
                for (int j = 0; j < signatureList.getLength(); j++) {
                    Node signatureNode = signatureList.item(j);
                    if (signatureNode.getNodeName().equals("KeyInfo")) {
                        NodeList keyInfoList = signatureNode.getChildNodes();
                        for (int k = 0; k < keyInfoList.getLength(); k++) {
                            Node x509Data = keyInfoList.item(k);
                            if (x509Data.getNodeName().equals("X509Data")) {
                                NodeList certList = x509Data.getChildNodes();
                                for (int x = 0; x < certList.getLength(); x++) {
                                    Node certNode = certList.item(x);
                                    if (certNode.getNodeName().equals("X509Certificate")) {
                                        smpCert = certNode.getTextContent();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return smpCert;
    }

    /**
     * Iterate the target nodes to find if a redirect command is found.
     * @param smpData
     *              Document incoming from SMP.
     * @param redirectTag
     *              The tag which contains the ns3:Redirect string.
     * @return 
     *              boolean value if the tag has been found.
     */
    public static boolean isSMPRedirect(Document smpData, String redirectTag) {
        boolean isSMPRedirect = false;
        Document hasSMPRedirect = null;
        String smpCertString = null;
        String suid = null;
        smpData.normalize();
        smpCertString = extractSMPCertificate(smpData);
        smpCertString = Util.completeCertificateEntry(smpCertString);

        hasSMPRedirect = RedirectionHandler.redirectLookup(smpData, redirectTag);
        if (hasSMPRedirect != null) {
            suid = RedirectionHandler.getSubjectUIDFromDocument(smpData);
            if (suid == null) {
                logger4J.warn("Subject Unique Identifier has not been found. "
                        + "Sending Process is cancelled.");
                throw new RuntimeException("Subject Unique Identifier for Certificate "
                            + "was not found in metadata.");
            } else {
                isSMPRedirect = true;
                if (!RedirectionHandler.isSubjectUIDValid(smpCertString, suid)) {
                    logger4J.warn("Identifier of the destination SMP signing "
                            + "certificate does not corresponds to the unique "
                            + "identifier which the redirecting SMP claims "
                            + "belongs to the destination SMP.");
                    throw new RuntimeException("Identifier of the destination SMP signing "
                            + "certificate does not corresponds to the unique "
                            + "identifier which the redirecting SMP claims "
                            + "belongs to the destination SMP.");
                }
                Document temp = RedirectionHandler.redirectLookup(
                        hasSMPRedirect, redirectTag);
                if (temp != null) {
                    logger4J.error("More than one Redirection was found in "
                            + "Metadata. Sending process has terminated.");
                    throw new RuntimeException("More than one Redirection was "
                            + "found in Metadata. Sending process has "
                            + "terminated.");
                }
                MessageSetup.AP_REDIRECT_URL = RedirectionHandler.
                        getRedirectionAPUrl(hasSMPRedirect);
                MessageSetup.REDIRECT_CERTIFICATE = RedirectionHandler.
                        getCertificateRedirection(hasSMPRedirect);
            }
        } else {
            return isSMPRedirect;
        }

        return isSMPRedirect;
    }
}
