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
import eu.peppol.start.util.Util;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import java.util.zip.GZIPInputStream;
import java.util.zip.InflaterInputStream;
import org.apache.log4j.Logger;
import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

/**
 * This class handles the redirection process.
 * @author Marcelo Tataje(marcelo@alfa1lab.com)
 */
public class RedirectionHandler {

    /**
     * Logger to follow the class behavior.
     */
    private static Logger logger4J = Logger.getLogger(RedirectionHandler.class);

    /**
     * Sole constructor to avoid instances of this class.
     */
    private RedirectionHandler() {
    }

    /**
     * Finds if a given xml document has another document inside with 
     * redirection.
     * @param targetXML
     *              The XML document which is the target.
     * @param tag
     *              The tag which will be the matcher.
     * @return 
     *              The document if it has redirection.
     */
    public static Document redirectLookup(Document targetXML, String tag) {
        Document redirectDoc = null;
        Node redirection = null;
        boolean flag = false;
        NodeList nodeList = getRedirectionList(targetXML);
        for (int i = 0; i < nodeList.getLength(); i++) {
            redirection = nodeList.item(i);
            if (redirection.getNodeName().equals(tag)) {
                logger4J.debug("The application found that the SMP has redirection.");
                flag = true;
                MessageSetup.SMP_REDIRECTION_URL = redirection.getAttributes().getNamedItem("href").getTextContent();
                //TODO: Remove after complete
                MessageSetup.SMP_REDIRECTION_URL = MessageSetup.SMP_REDIRECTION_URL;
                redirectDoc = getRedirectDocument(MessageSetup.SMP_REDIRECTION_URL);
                break;
            }
        }
        logger4J.info("Does SMP has redirect? : " + flag);
        return redirectDoc;
    }

    /**
     * Retrieves the subject unique identifier from the redirection document.
     * @param smpData
     *              The xml document given as a parameter.
     * @return 
     *              The subject unique identifier in string format.
     */
    public static String getSubjectUIDFromDocument(Document smpData) {
        String suid = null;
        NodeList redirectionList = getRedirectionList(smpData).item(0).
                getChildNodes();
        for (int j = 0; j < redirectionList.getLength(); j++) {
            Node suidItem = redirectionList.item(j);
            if (suidItem.getNodeName().equals("ns3:CertificateUID")) {
                suid = suidItem.getTextContent();
            }
        }
        return suid;
    }

    /**
     * Retrieves the list of nodes contained in Redirection.
     * @param targetXML
     *              The xml in which the search will be executed.
     * @return 
     *              A list of the nodes of Redirection tag.
     */
    private static NodeList getRedirectionList(Document targetXML) {
        NodeList output = null;
        output = targetXML.getChildNodes().item(0).getChildNodes().
                item(0).getChildNodes();
        return output;
    }
    
    private static NodeList getMetadataList(Document targetXML) {
        NodeList output = null;
        output = targetXML.getChildNodes().item(0).
                getChildNodes().item(0).getChildNodes().item(0).
                getChildNodes().item(2).getChildNodes().item(0).
                getChildNodes().item(1).getChildNodes().item(0).
                getChildNodes();
        return output;
    }

    /**
     * Gets the redirection document.
     * @param redirectUrl
     *            The url to connect and get the document.
     * @return 
     *            Document which will be retrieved from an url.
     */
    public static Document getRedirectDocument(String redirectUrl) {
        Document doc = null;
        String metadata = null;
        try {
            URL url = new URL(redirectUrl);
            HttpURLConnection smlConn = (HttpURLConnection) url.openConnection();
            metadata = getMetadata(smlConn);
            smlConn.disconnect();
            doc = Util.parseStringtoDocument(metadata);
            doc.normalize();
        } catch (IOException ex) {
            logger4J.error("Connection to redirected SMP could not be stablished");
            throw new RuntimeException("Connection to redirected SMP could "
                    + "not be stablished. " + ex.getMessage());
        }

        return doc;
    }

    /**
     * Gets all the content of a given connection.
     * @param conn
     *          Parameter to retrieve the content of XML.
     * @return 
     *          All the content in string format.
     */
    private static String getMetadata(HttpURLConnection conn) {
        InputStream in = null;
        InputStream result = null;
        BufferedReader buffReader = null;
        StringBuilder strBuffer = null;

        try {
            String encoding = conn.getContentEncoding();
            in = conn.getInputStream();

            if (encoding != null && encoding.equalsIgnoreCase("gzip")) {
                result = new GZIPInputStream(in);
            } else if (encoding != null && encoding.equalsIgnoreCase("deflate")) {
                result = new InflaterInputStream(in);
            } else {
                result = in;
            }

            buffReader = new BufferedReader(new InputStreamReader(result));

            String line = null;
            strBuffer = new StringBuilder();
            while ((line = buffReader.readLine()) != null) {
                strBuffer.append(line).append("\n");
            }
        } catch (IOException ex) {
            logger4J.error(ex.getMessage(), ex);
        } finally {
            if (in != null) {
                try {
                    in.close();
                } catch (IOException ex) {
                    logger4J.error(ex.getMessage(), ex);
                }
            }
            if (result != null) {
                try {
                    result.close();
                } catch (IOException ex) {
                    logger4J.error(ex.getMessage(), ex);
                }
            }
            if (buffReader != null) {
                try {
                    buffReader.close();
                } catch (IOException ex) {
                    logger4J.error(ex.getMessage(), ex);
                }
            }
        }
        return (strBuffer != null) ? strBuffer.toString() : null;
    }

    /**
     * Search for the redirection AP url.
     * @param document
     *              Document in which the url will be searched.
     * @return 
     *              The AP Url from the redirection source.
     */
    public static String getRedirectionAPUrl(Document document) {
        String apUrl = null;
        document.normalize();

        NodeList rootList = getMetadataList(document);

        for (int i = 0; i < rootList.getLength(); i++) {            
            if (rootList.item(i).getNodeName().contains("EndpointReference")) {
                Node node = rootList.item(i);
                apUrl = node.getFirstChild().getTextContent();
                break;
            }
        }
        return apUrl;
    }
    /**
     * Gets the certificate from the redirection url.
     * @param doc
     *             Document to be examined.
     * @return 
     *              Certificate entry in String format.
     */
    public static String getCertificateRedirection(Document doc) {
        String entry = null;        
        NodeList rootList = getMetadataList(doc);
        
        for (int i = 0; i < rootList.getLength(); i++) {            
            if (rootList.item(i).getNodeName().contains("ns3:Certificate")) {
                Node node = rootList.item(i);
                entry = node.getFirstChild().getTextContent();                
                break;
            }
        }
        entry = Util.completeCertificateEntry(entry);        
        return entry;
    }

    /**
     * Verifies if the Subject unique ID of redirect matches certificate's one;
     * The PEPPOL SMP specification states that the Subject UID should be validated,
     * however this is an optional field for certificates.
     * Since all PEPPOL certificates are issued with the Subject UID field,
     * a production environment could benefit from always validating this field. 
     * However, for future compatibility and to illustrate the issue we only validate 
     * the Subject UID if it is present in the certificate and issue a warning if it is not.
     * @param cert
     *          Certificate in string format.
     * @param suid
     *          The Subject Unique ID from the Redirect.
     * @return 
     *          Boolean if is valid or not.
     */
    public static boolean isSubjectUIDValid(String cert, String suid) {
        boolean isValid = true;
        try {
            /*
             * The PEPPOL SMP specification states that the Subject UID should be validated,
             * however this is an optional field for certificates. 
             * Since all PEPPOL certificates are issued with the Subject UID field,
             * a production environment could benefit from always validating this field.
             * However, for future compatibility and to illustrate the issue we only validate
             * the Subject UID if it is present in the certificate and issue a warning if it is not.
             */
            X509Certificate smpCert = Util.generateX509Certificate(cert);
            boolean[] certUid = smpCert.getSubjectUniqueID();
            if (certUid != null){
                isValid = false;
                if(certUid.toString().equals(suid)) {
                    isValid = true;
                }
            }
                    
        } catch (CertificateException ex) {
            logger4J.error("An exception has been found while processing certificate "
                    + "data. The process ends with message: " + ex.getMessage());
            throw new RuntimeException("An exception has been found while processing certificate "
                    + "data. The process ends with message: " + ex.getMessage());
        }
        return isValid;
    }
}
