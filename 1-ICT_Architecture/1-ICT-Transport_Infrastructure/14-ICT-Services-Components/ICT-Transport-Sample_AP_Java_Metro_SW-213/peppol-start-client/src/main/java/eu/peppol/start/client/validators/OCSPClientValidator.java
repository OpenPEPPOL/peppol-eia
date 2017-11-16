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
package eu.peppol.start.client.validators;

import com.sun.xml.wss.impl.callback.CertificateValidationCallback.CertificateValidationException;
import com.sun.xml.wss.impl.callback.CertificateValidationCallback.CertificateValidator;
import eu.peppol.start.util.Configuration;
import eu.peppol.start.util.Util;
import java.io.FileInputStream;
import java.io.IOException;
import java.security.KeyStore;
import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import java.util.Arrays;
import org.apache.log4j.Logger;

/**
 * This class executes the OCSP Validation for the client side.
 * @author Marcelo Tataje(marcelo@alfa1lab.com)
 */
public class OCSPClientValidator implements CertificateValidator {

    /**
     * Logger to follow this class behavior.
     */
    private static Logger logger4J = Logger.getLogger(OCSPClientValidator.class);
    /**
     * URL from which data for validation is retrieved.
     */
    private static final String TEST_RESPONDER_URL = "http://pilot-ocsp.verisign.com:80";
    /**
     * Keystore type.
     */
    private static final String JKS = "JKS";
    /**
     * Server truststore path.
     */
    private static final String TRUSTSTORE_PATH = "server.truststore";
    /**
     * Server truststore password.
     */
    private static final String TRUSTSTORE_PASS = "server.truststore.password";
    /**
     * Server truststore alias.
     */
    private static final String TRUSTORE_ALIAS = "server.truststore.alias";
    /**
     * Configuration for certificates.
     */
    private final Configuration conf;

    /**
     * Constructor to initialize configuration parameters.
     */
    public OCSPClientValidator() {
        conf = Configuration.getInstance();
    }

    /**
     * Prepares the OCSP Validation in client side.
     * @param certParam
     *                  Certificate incoming from server as String.
     * @return 
     *          boolean indicates if is valid or not.
     */
    public final boolean prepareClientValidation(String certParam) {
        boolean flag = false;
        X509Certificate certFromServer = null;
        try {
            certFromServer = Util.generateX509Certificate(certParam);
        } catch (CertificateException ex) {
            logger4J.error("The Certificate has not valid data or is corrupted: " + ex.getMessage());
            throw new RuntimeException("The Certificate has not valid data or is corrupted: " + ex.getMessage());
        }

        flag = validate(certFromServer);
        return flag;
    }

    /**
     * Validates a X.509 Certificate.
     *
     * @param x509Cert
     * @return true if the certificate passes all validations,
     *          otherwise returns false.
     * @throws CertificateValidationException
     */
    public final boolean validate(final X509Certificate x509Cert) {
        String path = conf.getProperty(TRUSTSTORE_PATH);
        return certificateValidate(x509Cert, path);
    }

    /**
     * This method validate the X.509 Certificate.
     * @param xc
     * @return true if the certificate passes all validations,
     *          otherwise returns false.
     * @param path
     */
    public final boolean certificateValidate(final X509Certificate x509Cert, final String path) {

        FileInputStream in = null;
        boolean valid = false;

        try {            
            in = new FileInputStream(path);            
            KeyStore ks = KeyStore.getInstance(JKS);            

            ks.load(in, conf.getProperty(TRUSTSTORE_PASS).toCharArray());            
            X509Certificate rootcert =
                    (X509Certificate) ks.getCertificate(conf.getProperty(TRUSTORE_ALIAS));            
            X509Certificate[] chain = new X509Certificate[]{x509Cert};            
            valid = OCSPProcessManager.check(Arrays.asList(chain), rootcert, TEST_RESPONDER_URL);            
            logger4J.debug("Valid Certificate.");            
        } catch (Exception ex) {
            logger4J.error(ex.getMessage(), ex);
        } finally {
            if (in != null) {
                try {
                    in.close();
                } catch (IOException ex) {
                    logger4J.error(ex.getMessage(), ex);
                }
            }
        }
        return valid;
    }
}