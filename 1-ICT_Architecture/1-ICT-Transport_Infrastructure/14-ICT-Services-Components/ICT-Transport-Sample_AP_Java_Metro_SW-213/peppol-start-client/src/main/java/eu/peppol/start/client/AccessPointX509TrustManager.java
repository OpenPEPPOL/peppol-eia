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

import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import java.util.logging.Level;
import javax.net.ssl.X509TrustManager;
import org.apache.log4j.Logger;

/**
 * The AccessPointX509TrustManager is pointed to authenticate the remote side
 * when using SSL.
 *
 * @author  Alexander Aguirre Julcapoma(alex@alfa1lab.com)
 *          Jose Gorvenia Narvaez(jose@alfa1lab.com)
 *          Marcelo Tataje Salinas(marcelo@alfa1lab.com)
 *          George Reátegui Ravina (jorge@alfa1lab.com)
 */
public class AccessPointX509TrustManager implements X509TrustManager {

    /** Logger to follow this class behavior. */
    private static final Logger logger =  Logger.getLogger(AccessPointX509TrustManager.class);

    /** The permitted remote common names, or null if no restriction. */
    private String commonNames;

    /** The accepted issuer. */
    private X509Certificate rootCertificate;

    /**
     * Constructor with parameters.
     *
     * @param acceptedCommonNames A Collection(Set) of Names accepted.
     * @param acceptedRootCertificate Represents a Certificate.
     *
     * @throws Exception Throws an Exception.
     */
    public AccessPointX509TrustManager(final String acceptedCommonNames,
            final X509Certificate acceptedRootCertificate) {

        this.rootCertificate = acceptedRootCertificate;
        this.commonNames = acceptedCommonNames;
    }

    public void checkClientTrusted(X509Certificate[] xcs, String string) throws CertificateException {
        throw new UnsupportedOperationException("Not supported yet.");
    }

    public void checkServerTrusted(X509Certificate[] xcs, String string) throws CertificateException {
        /*This is not implemented because no SSL validation is required. */
    }

    public X509Certificate[] getAcceptedIssuers() {
        throw new UnsupportedOperationException("Not supported yet.");
    }
}