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
package com.alfa1lab.maven.plugins.util;

/**
 * This class contains constants to build and configure the access point for an
 * user.
 * 
 * @author Marcelo Tataje (marcelo@alfa1lab.com)
 * 
 */
public class ConfigConstants {
	/**
	 * Value representing the server
	 */
	public static String SERVER = "server";
	/**
	 * Value representing the client
	 */
	public static String CLIENT = "client";
	/**
	 * Extension for xml file
	 */
	public static String XML_EXT = ".xml";
	/**
	 * Extension for wsdl file
	 */
	public static String WSDL_EXT = ".wsdl";
	/**
	 * Path fragment for src directory
	 */
	public static String SRC_PATH = "/src";
	/**
	 * Temp file name
	 */
	public static String TEMP_FILE = "/temp.xml";
	/**
	 * Path fragment for wsdl directories.
	 */
	public static String WSDL_FOLDER = "/wsdl/";
	/**
	 * Path for the properties files
	 */
	public static String PROPERTIES_PATH = "/main/java/configStart.properties";
	/**
	 * Path for the log4j.xml files
	 */
	public static String LOGGER_PATH = "/main/java/log4j.xml";
	/**
	 * Path for java client executor
	 */
	public static String MAIN_CLASS_PATH = "/main/java/eu/peppol/start/client/Main.java";
	/**
	 * Path for the web resource folder
	 */
	public static String WEB_RESOURCE_PATH = "/main/webapp/WEB-INF/wsdl/";
	/**
	 * Full path of the web.xml file
	 */
	public static String DEPLOYMENT_DESCRIPTOR_PATH = "/main/webapp/WEB-INF/web.xml";
	/**
	 * Message for a generic exception
	 */
	public static String GENERIC_EXCEPTION = "The application implementation type is invalid, verify your pom.xml";
	/**
	 * Message for a MOJO exception (plugin execution)
	 */
	public static String MOJO_EXCEPTION = "Values for the properties file are invalid, please verify.";
	/**
	 * Message for exception if some resource for reading is not available
	 */
	public static String PROPS_FILE_EXCEPTION = "The user properties file could not be found.";
	/**
	 * Message for exception if directories are invalid
	 */
	public static String INVALID_DIRECTORY_EXCEPTION = "Invalid base directory.";
	/**
	 * Message to inform the progress of the setup
	 */
	public static String LOAD_MESSAGE = "Executing variable replacement using properties file...";
	/**
	 * Property for logging path
	 */
	public static String LOG_PATH_PROP = "ap.logFile";
	/**
	 * Property for keystore path
	 */
	public static String KEYSTORE_PATH_PROP = "server.keystore";
	/**
	 * Property for keystore password
	 */
	public static String KEYSTORE_PASS_PROP = "server.Keystore.password";
	/**
	 * Property for keystore alias
	 */
	public static String KEYSTORE_ALIAS_PROP = "server.keystore.alias";
	/**
	 * Property for truststore path
	 */
	public static String TRUSTSTORE_PATH_PROP = "server.truststore";	
	/**
	 * Property for truststore pass
	 */
	public static String TRUSTSTORE_PASS_PROP = "server.truststore.password";
	/**
	 * Property for truststore alias
	 */
	public static String TRUSTSTORE_ALIAS_PROP = "server.truststore.alias";
	/**
	 * Property for inbox storage path
	 */
	public static String INBOX_PATH_PROP = "ap.inboxPath";
	/**
	 * Property for host ip address
	 */
	public static String HOST_PROP = "ap.host";
	/**
	 * Property for port that will be used in the servlet container
	 */
	public static String PORT_PROP = "ap.port";
	/**
	 * Property for the name of the service
	 */
	public static String AP_NAME_PROP = "ap.name";
	/**
	 * Pattern for the location of line in log4j.xml that will be replaced
	 */
	public static String LOG_PATTERN = "<param name=\"File\"";
	/**
	 * Pattern for the location of line in web.xml that will be replaced
	 */
	public static String DD_WEB_PATTERN = "<param-value>";
	/**
	 * Pattern for end of web.xml tag
	 */
	public static String DD_WEB_PATTERN_END = "</param-value>";
	/**
	 * Pattern for the location of line in wsdl files
	 */
	public static String WSDL_START_TAG_CONFIG_PATTERN = "<soap:address location=";
	/**
	 * String part of the protocol used for access point
	 */
	public static String WSDL_HTTPS_STRING_PART = "\"https://";
	/**
	 * Pattern for the location of address in xml files
	 */
	public static String WSDL_SERVICE_CONFIG_PATTERN = "\"https://${ap.host}:${ap.port}/${ap.name}\"";
	/**
	 * Pattern for logger start tag
	 */
	public static String LOGGER_START_TAG = "<param name=\"File\" value=\"";
	/**
	 * Pattern for logger end tag
	 */
	public static String LOGGER_END_TAG = "/peppol.start.log.xml\"/>";
	/**
	 * Pattern for the location of the keystore path
	 */
	public static String KS_PATH_PATTERN = "server.keystore";
	/**
	 * Pattern for the location of the keystore password
	 */
	public static String KS_PASS_PATTERN = "server.Keystore.password";
	/**
	 * Pattern for the location of the truststore path
	 */
	public static String TS_PATH_PATTERN = "server.truststore";
	/**
	 * Pattern for the location of the truststore pass
	 */
	public static String TS_PASS_PATTERN = "truststore.password";
	/**
	 * Custom message to show properties are loaded
	 */
	public static String PROPS_MESSAGE = "Properties files loaded at: ";
	/**
	 * Property for the XML file
	 */
	public static String XML_FILE_PROP = "xml.file.path";
	/**
	 * Property for the Document ID Value
	 */
	public static String DOC_ID_VALUE_PROP = "document.value";
	/**
	 * Property for the Document ID Scheme
	 */
	public static String DOC_ID_SCHEME_PROP = "document.scheme";
	/**
	 * Property for the Process ID Value
	 */
	public static String PROC_ID_VALUE_PROP = "process.value";
	/**
	 * Property for the Process ID Scheme
	 */
	public static String PROC_ID_SCHEME_PROP = "process.scheme";
	/**
	 * Property for Participant ID Scheme
	 */
	public static String PARTICIPANT_ID_SCHEME = "participant.scheme";
	/**
	 * Property for Sender
	 */
	public static String SENDER_ID_VALUE_PROP = "sender.value";
	/**
	 * Property for Recipient
	 */
	public static String RECIPIENT_ID_VALUE_PROP = "recipient.value";
	/**
	 * Property for AP Url
	 */
	public static String AP_SERVICE_URL_PROP = "ap.service.url";
	/**
	 * Pattern for XML Message
	 */
	public static String XML_FILE_PATTERN = "private static String XML_FILE_PATH";
	/**
	 * Pattern for Document Id Value
	 */
	public static String DOC_ID_VALUE_PATTERN = "private static String DOCUMENT_IDENTIFIER_TYPE_VALUE";
	/**
	 * Pattern for Document Id Scheme
	 */
	public static String DOC_ID_SCHEME_PATTERN = "private static String DOCUMENT_IDENTIFIER_TYPE_SCHEME";
	/**
	 * Pattern for Process Id Value
	 */
	public static String PROC_ID_VALUE_PATTERN = "private static String PROCESS_IDENTIFIER_TYPE_VALUE";
	/**
	 * Pattern for Process Id Scheme
	 */
	public static String PROC_ID_SCHEME_PATTERN = "private static String PROCESS_IDENTIFIER_TYPE_SCHEME";
	/**
	 * Pattern for Participant Id Scheme
	 */
	public static String PARTICIPANT_ID_PATTERN = "private static String PARTICIPANT_IDENTIFIER_SCHEME";
	/**
	 * Pattern for Sender Id Value
	 */
	public static String SENDER_ID_PATTERN = "private static String SENDER_PARTICIPANT_IDENTIFIER_VALUE";
	/**
	 * Pattern for Recipient Id Value
	 */
	public static String RECIPIENT_ID_PATTERN = "private static String RECEIVER_PARTICIPANT_IDENTIFIER_VALUE";
	/**
	 * Pattern for Accesspoint URL
	 */
	public static String AP_URL_PATTERN = "private static String ACCESSPOINT_SERVICE_URL";
	
	
	/**
	 * Sole constructor to avoid instance of object.
	 */
	private ConfigConstants() {

	}

}
