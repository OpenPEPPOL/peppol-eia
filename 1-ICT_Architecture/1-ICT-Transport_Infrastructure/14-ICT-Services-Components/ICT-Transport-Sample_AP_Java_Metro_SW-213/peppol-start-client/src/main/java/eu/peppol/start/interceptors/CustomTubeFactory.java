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

import com.sun.xml.ws.api.pipe.Tube;
import com.sun.xml.ws.assembler.dev.ClientTubelineAssemblyContext;
import com.sun.xml.ws.assembler.dev.ServerTubelineAssemblyContext;
import com.sun.xml.ws.assembler.dev.TubeFactory;
import javax.xml.ws.WebServiceException;
import org.apache.log4j.Logger;

/**
 * The purpose of this class is to create Tubes to intercept request and response.
 * @author Jan Victoir(jan@alfa1lab.com)
 */
public final class CustomTubeFactory implements TubeFactory {

    /**
     * Logger to follow class behavior.
     */
    private static final Logger logger4J = Logger.getLogger(CustomTubeFactory.class.getName());

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
     * Creates a Tube to intercept a message based on the request context.
     * @param context
     *              Context defined as request.
     * @return
     *              Custom Tube specifying the side in which the interceptors will work.
     * @throws WebServiceException 
     *              Generic exception for Web Services.
     */
    public Tube createTube(ClientTubelineAssemblyContext context) throws WebServiceException {
        printDebugLogging("Creating client-side interceptor tube");
        return new CustomTube(context.getTubelineHead(), CustomTube.Side.Client);
    }
    
    /**
     * Creates a Tube to intercept a message based on the response context.
     * @param context
     *              Context defined as response.
     * @return
     *              Custom Tube specifying the side in which the interceptors will work.
     * @throws WebServiceException 
     *              Generic exception for Web Services.
     */
    public Tube createTube(ServerTubelineAssemblyContext context) throws WebServiceException {
        printDebugLogging("Creating server-side interceptor tube");
        return new CustomTube(context.getTubelineHead(), CustomTube.Side.Endpoint);
    }
}