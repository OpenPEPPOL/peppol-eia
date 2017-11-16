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
package eu.peppol.start.saml;

import com.sun.xml.wss.impl.callback.KeyStoreCallback;
import com.sun.xml.wss.impl.callback.PrivateKeyCallback;
import eu.peppol.start.util.Configuration;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.security.Key;
import java.security.KeyStore;
import java.security.KeyStoreException;
import java.security.NoSuchAlgorithmException;
import java.security.PrivateKey;
import java.security.UnrecoverableKeyException;
import java.security.cert.CertificateException;
import java.util.Iterator;
import java.util.Map;
import java.util.logging.Level;
import javax.security.auth.callback.Callback;
import javax.security.auth.callback.CallbackHandler;
import javax.security.auth.callback.UnsupportedCallbackException;
import org.apache.log4j.Logger;

/**
 * This class enable WSDL to access directly the keystore certificate
 * instead of hardcode path in WSDL document.
 * 
 * @author Marcelo Tataje Salinas(marcelo@alfa1lab.com)
 */
public class KeyStoreCallbackHandler implements CallbackHandler {

    /** Logger to follow this class behavior. */
    private static final Logger logger4J = Logger.getLogger(KeyStoreCallbackHandler.class);
    /** Keystore to be loaded by the configuration class. */
    private KeyStore keyStore;
    /** Certificate password to be loaded by the configuration class. */
    private String password;
    /** Loader of configuration properties file. */
    private Configuration config;
    /** Keystore type string. */
    private String KEYSTORE_TYPE = "JKS";

    /**
     * Constructor of the class which loads the keystore.
     */
    public KeyStoreCallbackHandler() {
        logger4J.info("Configuration for Keystore has been loaded");
        InputStream is = null;
        config = Configuration.getInstance();
        password = config.getProperty("server.Keystore.password");
        try {
            keyStore = KeyStore.getInstance(KEYSTORE_TYPE);
            String keystoreURL = config.getProperty("server.keystore");
            is = new FileInputStream(keystoreURL);
            keyStore.load(is, password.toCharArray());
        } catch (IOException ex) {
            java.util.logging.Logger.getLogger(KeyStoreCallbackHandler.class.getName()).log(Level.SEVERE, ex.getMessage());
            logger4J.error(ex.getMessage(), ex);
            throw new RuntimeException(ex);
        } catch (NoSuchAlgorithmException ex) {
            java.util.logging.Logger.getLogger(KeyStoreCallbackHandler.class.getName()).log(Level.SEVERE, ex.getMessage());
            logger4J.error(ex.getMessage(), ex);
            throw new RuntimeException(ex);
        } catch (CertificateException ex) {
            java.util.logging.Logger.getLogger(KeyStoreCallbackHandler.class.getName()).log(Level.SEVERE, ex.getMessage());
            logger4J.error(ex.getMessage(), ex);
            throw new RuntimeException(ex);
        } catch (KeyStoreException ex) {
            java.util.logging.Logger.getLogger(KeyStoreCallbackHandler.class.getName()).log(Level.SEVERE, ex.getMessage());
            logger4J.error(ex.getMessage(), ex);
            throw new RuntimeException(ex);
        } finally {
            try {
                is.close();
            } catch (IOException ex) {
                java.util.logging.Logger.getLogger(KeyStoreCallbackHandler.class.getName()).log(Level.SEVERE, ex.getMessage());
                logger4J.error(ex.getMessage(), ex);
            }
        }
    }

    public void handle(Callback[] callbacks) throws IOException, UnsupportedCallbackException {
        logger4J.info("Handle: KeystoreCallbackHandler is loading...");
        for (int i = 0; i < callbacks.length; i++) {
            if (callbacks[i] instanceof KeyStoreCallback) {
                KeyStoreCallback cb = (KeyStoreCallback) callbacks[i];
                logger4J.debug("Keystore Properties are listed as follow: ");
                print(cb.getRuntimeProperties());
                cb.setKeystore(keyStore);
            } else if (callbacks[i] instanceof PrivateKeyCallback) {
                try {
                    PrivateKeyCallback cb = (PrivateKeyCallback) callbacks[i];
                    System.out.println("Private Key properties are listed as follow: ");
                    print(cb.getRuntimeProperties());
                    Key privkey = keyStore.getKey(cb.getAlias(), password.toCharArray());
                    cb.setKey((PrivateKey) privkey);
                } catch (KeyStoreException ex) {
                    java.util.logging.Logger.getLogger(KeyStoreCallbackHandler.class.getName()).log(Level.SEVERE, ex.getMessage());
                    logger4J.error(ex.getMessage(), ex);
                    throw new RuntimeException(ex);
                } catch (NoSuchAlgorithmException ex) {
                    java.util.logging.Logger.getLogger(KeyStoreCallbackHandler.class.getName()).log(Level.SEVERE, ex.getMessage());
                    logger4J.error(ex.getMessage(), ex);
                    throw new RuntimeException(ex);
                } catch (UnrecoverableKeyException ex) {
                    java.util.logging.Logger.getLogger(KeyStoreCallbackHandler.class.getName()).log(Level.SEVERE, ex.getMessage());
                    logger4J.error(ex.getMessage(), ex);
                    throw new RuntimeException(ex);
                }
            } else {
                throw new UnsupportedCallbackException(callbacks[i]);
            }
        }
    }
    
    /**
     * Prints the properties of the loaded certificate.
     * @param context 
     */
    private void print(Map context) {
        Iterator it = context.keySet().iterator();
        while (it.hasNext()) {
            StringBuffer sb = new StringBuffer("Props ");
            sb.append(it.next());
            logger4J.debug(sb.toString());
        }
    }
}