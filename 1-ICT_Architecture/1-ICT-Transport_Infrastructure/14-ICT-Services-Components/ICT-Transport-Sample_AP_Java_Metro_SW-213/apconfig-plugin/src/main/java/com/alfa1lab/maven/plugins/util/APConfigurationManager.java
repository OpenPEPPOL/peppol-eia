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

import java.io.BufferedReader;
import java.io.DataInputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.util.Hashtable;
import java.util.StringTokenizer;

/**
 * 
 * @author Marcelo Tataje (marcelo@alfa1lab.com) This class manages io processes
 *         to configure start ap.
 */
public class APConfigurationManager {

	/**
	 * Sole constructor to avoid create of instance.
	 */
	private APConfigurationManager() {

	}

	/**
	 * Returns a hashtable which contains the accesspoint properties.
	 * 
	 * @param propsFile
	 *            File containing the properties values set by user.
	 * @return Hashtable with keys and values for the configuration.
	 * @throws IOException
	 *             Fails if the file is corrupted (modified, renamed or
	 *             deleted).
	 */
	public static Hashtable<String, String> loadPropertiesMap(File propsFile)
			throws IOException {
		Hashtable<String, String> propsMap = null;
		FileInputStream fis = new FileInputStream(propsFile);
		DataInputStream dis = new DataInputStream(fis);
		BufferedReader br = null;
		StringTokenizer st = null;
		String line;

		propsMap = new Hashtable<String, String>();
		br = new BufferedReader(new InputStreamReader(dis));

		while ((line = br.readLine()) != null) {
			if (!line.contains("#") && line.trim().length() > 0) {
				st = new StringTokenizer(line, "=");
				String key = st.nextToken();
				String value = st.nextToken();
				propsMap.put(key, value);
			}
		}

		br.close();

		return propsMap;
	}

	/**
	 * Setup the project for an user based on parameters given.
	 * 
	 * @param propsMap
	 *            A map containing the properties set by user.
	 * @param apType
	 *            Type of the implementation client or server.
	 * @param srcDir
	 *            Full path in which src folder is located.
	 * @param wsdlName
	 *            Name of the wsdl file to execute the replacement.
	 * @throws Exception
	 *             Exception thrown if file is corrupted or the project
	 *             structure has been modified.
	 */
	public static void setupProject(Hashtable<String, String> propsMap,
			String apType, File srcDir, String wsdlName) throws Exception {

		File targetFile = null;
		StringBuilder sb = null;
		boolean isWSIT = false;

		sb = new StringBuilder();

		if (apType.equals(ConfigConstants.CLIENT)) {
			// Lookup for src/wsdl.xml for configuration on wsit
			sb.append(srcDir);
			sb.append("/");
			sb.append(wsdlName);
			sb.append(ConfigConstants.XML_EXT);
			targetFile = new File(sb.toString());
			sb.setLength(0);
			isWSIT = true;

			executeWSDLReplacement(srcDir, targetFile, propsMap, wsdlName,
					isWSIT);

			// Lookup for src/wsdl/wsdl.wsdl for configuration on META-INF
			isWSIT = false;
			sb.append(srcDir);
			sb.append(ConfigConstants.WSDL_FOLDER);
			sb.append(wsdlName);
			sb.append(ConfigConstants.WSDL_EXT);
			targetFile = new File(sb.toString());
			sb.setLength(0);

			executeWSDLReplacement(srcDir, targetFile, propsMap, wsdlName,
					isWSIT);

			// Lookup for configServer.properties
			sb.append(srcDir);
			sb.append(ConfigConstants.PROPERTIES_PATH);
			targetFile = new File(sb.toString());
			sb.setLength(0);
			executePropertiesReplacement(srcDir, targetFile, propsMap);

			// Lookup for log4j.xml
			sb.append(srcDir);
			sb.append(ConfigConstants.LOGGER_PATH);
			targetFile = new File(sb.toString());
			sb.setLength(0);

			executeLogFileReplacement(srcDir, targetFile, propsMap);

			sb.append(srcDir);
			sb.append(ConfigConstants.MAIN_CLASS_PATH);
			System.out.println("target file: " + sb.toString());
			targetFile = new File(sb.toString());
			System.out.println("Retrieve file: " + targetFile.getAbsolutePath());
			sb.setLength(0);

			executeMainClassReplacement(srcDir, targetFile, propsMap);

		} else if (apType.equals(ConfigConstants.SERVER)) {

			// Lookup for main WSDL to deploy
			sb.append(srcDir);
			sb.append(ConfigConstants.WSDL_FOLDER);
			sb.append(wsdlName);
			sb.append(ConfigConstants.WSDL_EXT);
			targetFile = new File(sb.toString());
			sb.setLength(0);
			isWSIT = false;

			executeWSDLReplacement(srcDir, targetFile, propsMap, wsdlName,
					isWSIT);

			// Lookup for main WSDL as web resource
			sb.append(srcDir);
			sb.append(ConfigConstants.WEB_RESOURCE_PATH);
			sb.append(wsdlName);
			sb.append(ConfigConstants.WSDL_EXT);
			targetFile = new File(sb.toString());
			sb.setLength(0);

			executeWSDLReplacement(srcDir, targetFile, propsMap, wsdlName,
					isWSIT);

			// Lookup for web.xml webapp configuration file
			sb.append(srcDir);
			sb.append(ConfigConstants.DEPLOYMENT_DESCRIPTOR_PATH);
			targetFile = new File(sb.toString());
			sb.setLength(0);
			executeWebXMLReplacement(srcDir, targetFile, propsMap);

			// Lookup for configServer.properties
			sb.append(srcDir);
			sb.append(ConfigConstants.PROPERTIES_PATH);
			targetFile = new File(sb.toString());
			sb.setLength(0);

			executePropertiesReplacement(srcDir, targetFile, propsMap);

			// Lookup for log4j.xml
			sb.append(srcDir);
			sb.append(ConfigConstants.LOGGER_PATH);
			targetFile = new File(sb.toString());

			executeLogFileReplacement(srcDir, targetFile, propsMap);
		} else {
			throw new Exception(ConfigConstants.GENERIC_EXCEPTION);
		}

	}

	/**
	 * Executes the replacement in log4j.xml files
	 * 
	 * @param srcDir
	 *            Full path of the src directory.
	 * @param targetFile
	 *            File that will be modified or updated.
	 * @param map
	 *            Map containing the properties.
	 * @throws IOException
	 *             Exception while reading or writing the new content of file.
	 */
	private static void executeLogFileReplacement(File srcDir, File targetFile,
			Hashtable<String, String> map) throws IOException {

		String line;
		String logPath = null;
		StringBuilder sb = null;
		FileInputStream fis = new FileInputStream(targetFile);
		DataInputStream dis = new DataInputStream(fis);
		BufferedReader br = new BufferedReader(new InputStreamReader(dis));
		File newLogFile = new File(srcDir + ConfigConstants.TEMP_FILE);
		FileWriter fw = new FileWriter(newLogFile);
		PrintWriter pw = new PrintWriter(fw);
		String backupPath = targetFile.getAbsolutePath();
		logPath = map.get(ConfigConstants.LOG_PATH_PROP);

		sb = new StringBuilder(ConfigConstants.LOGGER_START_TAG);
		sb.append(logPath);
		sb.append(ConfigConstants.LOGGER_END_TAG);

		while ((line = br.readLine()) != null) {

			if (line.contains(ConfigConstants.LOG_PATTERN)) {
				line = line.replace(line, sb.toString());
			}

			pw.write(line + "\n");
			pw.flush();

		}
		br.close();
		fis.close();
		dis.close();
		fw.close();
		pw.close();

		if (targetFile.delete()) {
			newLogFile.renameTo(new File(backupPath));
		}

	}

	/**
	 * Replace content of the .properties files in projects.
	 * 
	 * @param srcDir
	 *            Full path in which src folder is located.
	 * @param targetFile
	 *            File that will be modified.
	 * @param map
	 *            Map containing the properties.
	 * @throws IOException
	 *             Exception while reading or writing the new content of the
	 *             file.
	 */
	private static void executePropertiesReplacement(File srcDir,
			File targetFile, Hashtable<String, String> map) throws IOException {
		int count = 0;
		String keystorePath = null;
		String keystorePass = null;
		String keystoreAlias = null;
		String truststorePath = null;
		String truststorePass = null;
		String truststoreAlias = null;
		FileInputStream fis = new FileInputStream(targetFile);
		DataInputStream dis = new DataInputStream(fis);
		BufferedReader br = new BufferedReader(new InputStreamReader(dis));
		File newPropsFile = new File(srcDir + ConfigConstants.TEMP_FILE);
		FileWriter fw = new FileWriter(newPropsFile);
		PrintWriter pw = new PrintWriter(fw);
		String backupPath = targetFile.getAbsolutePath();
		String line;
		keystorePath = map.get(ConfigConstants.KEYSTORE_PATH_PROP);
		keystorePass = map.get(ConfigConstants.KEYSTORE_PASS_PROP);
		keystoreAlias = map.get(ConfigConstants.KEYSTORE_ALIAS_PROP);
		truststorePath = map.get(ConfigConstants.TRUSTSTORE_PATH_PROP);
		truststorePass = map.get(ConfigConstants.TRUSTSTORE_PASS_PROP);
		truststoreAlias = map.get(ConfigConstants.TRUSTSTORE_ALIAS_PROP);

		while ((line = br.readLine()) != null) {
			count++;
			if (count == 1) {
				line = line.replace(line, "server.keystore = " + keystorePath);
			}
			if (count == 2) {
				line = line.replace(line, "server.Keystore.password = "
						+ keystorePass);
			}
			if (count == 3) {
				line = line.replace(line, "server.keystore.alias = "
						+ keystoreAlias);
			}
			if (count == 4) {
				line = line.replace(line, "server.truststore = "
						+ truststorePath);
			}
			if (count == 5) {
				line = line.replace(line, "server.truststore.password = "
						+ truststorePass);
			}
			if (count == 6) {
				line = line.replace(line, "server.truststore.alias = "
						+ truststoreAlias);
			}

			pw.write(line + "\n");
			pw.flush();

		}

		br.close();
		fis.close();
		dis.close();
		fw.close();
		pw.close();

		if (targetFile.delete()) {
			newPropsFile.renameTo(new File(backupPath));
		}

	}

	/**
	 * Replace the content of the deployment descriptor file web.xml in server
	 * project.
	 * 
	 * @param srcDir
	 *            Full path in which src folder is located.
	 * @param targetXML
	 *            File that will be modified.
	 * @param map
	 *            Map containing the properties.
	 * @throws IOException
	 *             Exception while reading or writing the new content of the
	 *             file.
	 */
	private static void executeWebXMLReplacement(File srcDir, File targetXML,
			Hashtable<String, String> map) throws IOException {

		String line;
		String inboxPath = null;
		StringBuilder sb = null;
		FileInputStream fis = new FileInputStream(targetXML);
		DataInputStream dis = new DataInputStream(fis);
		BufferedReader br = new BufferedReader(new InputStreamReader(dis));
		File newWebXML = new File(srcDir + ConfigConstants.TEMP_FILE);
		FileWriter fw = new FileWriter(newWebXML);
		PrintWriter pw = new PrintWriter(fw);
		String backupPath = targetXML.getAbsolutePath();

		inboxPath = map.get(ConfigConstants.INBOX_PATH_PROP);
		sb = new StringBuilder(ConfigConstants.DD_WEB_PATTERN);
		sb.append(inboxPath);
		sb.append(ConfigConstants.DD_WEB_PATTERN_END);

		while ((line = br.readLine()) != null) {

			if (line.contains(ConfigConstants.DD_WEB_PATTERN)) {
				line = line.replace(line, sb.toString());
			}

			pw.write(line + "\n");
			pw.flush();
		}

		br.close();
		fis.close();
		dis.close();
		fw.close();
		pw.close();

		if (targetXML.delete()) {
			newWebXML.renameTo(new File(backupPath));

		}

	}

	/**
	 * Execute the replacement of the WSDL files content.
	 * 
	 * @param srcDir
	 *            Full path in which src folder is located.
	 * @param targetWSDL
	 *            File that will be modified.
	 * @param map
	 *            Map containing the properties.
	 * @param wsdlName
	 *            Name of the wsdl file that will be modified.
	 * @param isWSIT
	 *            Boolean value, true if the wsdl is for wsit process, false if
	 *            not.
	 * @throws IOException
	 *             Exception while reading or writing the new content of the
	 *             file.
	 */
	private static void executeWSDLReplacement(File srcDir, File targetWSDL,
			Hashtable<String, String> map, String wsdlName, boolean isWSIT)
			throws IOException {

		String line;
		String apHost = null;
		String apPort = null;
		String apName = null;
		StringBuilder sb = null;
		FileInputStream fis = new FileInputStream(targetWSDL);
		DataInputStream dis = new DataInputStream(fis);
		BufferedReader br = new BufferedReader(new InputStreamReader(dis));
		File newClientWSDL = new File(srcDir + ConfigConstants.TEMP_FILE);
		FileWriter fw = new FileWriter(newClientWSDL);
		PrintWriter pw = new PrintWriter(fw);
		String backupPath = targetWSDL.getAbsolutePath();
		apHost = map.get(ConfigConstants.HOST_PROP);
		apPort = map.get(ConfigConstants.PORT_PROP);
		apName = map.get(ConfigConstants.AP_NAME_PROP);
		sb = new StringBuilder(ConfigConstants.WSDL_START_TAG_CONFIG_PATTERN);
		sb.append(ConfigConstants.WSDL_HTTPS_STRING_PART);
		sb.append(apHost);
		sb.append(":");
		sb.append(apPort);
		sb.append("/");
		sb.append(apName);
		sb.append("\"");
		sb.append("/>");
		while ((line = br.readLine()) != null) {

			if (line.contains(ConfigConstants.WSDL_START_TAG_CONFIG_PATTERN)) {
				line = line.replace(line, sb.toString());
			}

			pw.write(line + "\n");
			pw.flush();
		}

		br.close();
		fis.close();
		dis.close();
		fw.close();
		pw.close();
		
		if (targetWSDL.delete()) {
			newClientWSDL.renameTo(new File(backupPath));
		}
	}
	
	/**
	 * Prepare the main class to run and send a message based on the parameters set.
	 * @param srcDir
	 * 				Directory of the sources.
	 * @param targetFile
	 * 				Main.java class which will be modified.
	 * @param map
	 * 				Hashtable containing the properties values.
	 * @throws IOException
	 * 				Exception thrown if the IO process fails.
	 */
	private static void executeMainClassReplacement(File srcDir,
			File targetFile, Hashtable<String, String> map) throws IOException {
		String line;		
		String xmlFile = null;
		String docIdValue = null;
		String docIdScheme = null;
		String processIdValue = null;
		String processIdScheme = null;
		String participantIdScheme = null;
		String senderIdValue = null;
		String recipientIdValue = null;
		String apServiceUrl = null;
		StringBuilder sb = null;
		FileInputStream fis = new FileInputStream(targetFile);
		DataInputStream dis = new DataInputStream(fis);
		BufferedReader br = new BufferedReader(new InputStreamReader(dis));
		File newMainClass = new File(srcDir + ConfigConstants.TEMP_FILE);
		System.out.println("Temp file: " + newMainClass.getAbsolutePath());
		FileWriter fw = new FileWriter(newMainClass);
		PrintWriter pw = new PrintWriter(fw);
		String backupPath = targetFile.getAbsolutePath();
		sb = new StringBuilder();

		xmlFile = map.get(ConfigConstants.XML_FILE_PROP);
		docIdValue = map.get(ConfigConstants.DOC_ID_VALUE_PROP);
		docIdScheme = map.get(ConfigConstants.DOC_ID_SCHEME_PROP);
		processIdValue = map.get(ConfigConstants.PROC_ID_VALUE_PROP);
		processIdScheme = map.get(ConfigConstants.PROC_ID_SCHEME_PROP);
		participantIdScheme = map.get(ConfigConstants.PARTICIPANT_ID_SCHEME);
		senderIdValue = map.get(ConfigConstants.SENDER_ID_VALUE_PROP);
		recipientIdValue = map.get(ConfigConstants.RECIPIENT_ID_VALUE_PROP);
		apServiceUrl = map.get(ConfigConstants.AP_SERVICE_URL_PROP);

		while ((line = br.readLine()) != null) {

			if (line.contains(ConfigConstants.XML_FILE_PATTERN)) {
				sb.append(ConfigConstants.XML_FILE_PATTERN);
				sb.append(" = ");
				sb.append("\"");
				sb.append(xmlFile);
				sb.append("\"");
				sb.append(";");
				line = line.replace(line, sb.toString());
				sb.setLength(0);
				System.out.println("xml");
			} if (line.contains(ConfigConstants.DOC_ID_VALUE_PATTERN)) {
				sb.append(ConfigConstants.DOC_ID_VALUE_PATTERN);
				sb.append(" = ");
				sb.append("\"");
				sb.append(docIdValue);
				sb.append("\"");
				sb.append(";");
				line = line.replace(line, sb.toString());
				sb.setLength(0);
				System.out.println("doc");
			} if (line.contains(ConfigConstants.DOC_ID_SCHEME_PATTERN)) {
				sb.append(ConfigConstants.DOC_ID_SCHEME_PATTERN);
				sb.append(" = ");
				sb.append("\"");
				sb.append(docIdScheme);
				sb.append("\"");
				sb.append(";");
				line = line.replace(line, sb.toString());
				sb.setLength(0);
				System.out.println("doc2");
			}  if (line.contains(ConfigConstants.PROC_ID_VALUE_PATTERN)) {
				sb.append(ConfigConstants.PROC_ID_VALUE_PATTERN);
				sb.append(" = ");
				sb.append("\"");
				sb.append(processIdValue);
				sb.append("\"");
				sb.append(";");
				line = line.replace(line, sb.toString());
				sb.setLength(0);
				System.out.println("process");
			} if (line.contains(ConfigConstants.PROC_ID_SCHEME_PATTERN)) {
				sb.append(ConfigConstants.PROC_ID_SCHEME_PATTERN);
				sb.append(" = ");
				sb.append("\"");
				sb.append(processIdScheme);
				sb.append("\"");
				sb.append(";");
				line = line.replace(line, sb.toString());
				sb.setLength(0);
				System.out.println("process2");
			} if (line.contains(ConfigConstants.PARTICIPANT_ID_PATTERN)) {
				sb.append(ConfigConstants.PARTICIPANT_ID_PATTERN);
				sb.append(" = ");
				sb.append("\"");
				sb.append(participantIdScheme);
				sb.append("\"");
				sb.append(";");
				line = line.replace(line, sb.toString());
				sb.setLength(0);
				System.out.println("participant");
			} if (line.contains(ConfigConstants.SENDER_ID_PATTERN)) {
				sb.append(ConfigConstants.SENDER_ID_PATTERN);
				sb.append(" = ");
				sb.append("\"");
				sb.append(senderIdValue);
				sb.append("\"");
				sb.append(";");
				line = line.replace(line, sb.toString());
				sb.setLength(0);
				System.out.println("sender");
			} if (line.contains(ConfigConstants.RECIPIENT_ID_PATTERN)) {
				sb.append(ConfigConstants.RECIPIENT_ID_PATTERN);
				sb.append(" = ");
				sb.append("\"");
				sb.append(recipientIdValue);
				sb.append("\"");
				sb.append(";");
				line = line.replace(line, sb.toString());
				sb.setLength(0);
				System.out.println("recipient");
			} if (line.contains(ConfigConstants.AP_URL_PATTERN)) {
				sb.append(ConfigConstants.AP_URL_PATTERN);
				sb.append(" = ");
				sb.append("\"");
				sb.append(apServiceUrl);
				sb.append("\"");
				sb.append(";");
				line = line.replace(line, sb.toString());
				sb.setLength(0);
				System.out.println("apurl");
			} 

			pw.write(line + "\n");
			pw.flush();
		}
		System.out.println("Replace done");
		br.close();
		fis.close();
		dis.close();
		fw.close();
		pw.close();
		System.out.println("Out");
		
		if (targetFile.delete()) {
			System.out.println("delete...");
			newMainClass.renameTo(new File(backupPath));
		}

	}

}
