/*
 * Copyright 2001-2005 The Apache Software Foundation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package org.sonatype.mavenbook.plugins;

import java.io.File;
import java.io.IOException;
import java.util.Hashtable;

import org.apache.maven.plugin.AbstractMojo;
import org.apache.maven.plugin.MojoExecutionException;

import com.alfa1lab.maven.plugins.util.APConfigurationManager;
import com.alfa1lab.maven.plugins.util.ConfigConstants;

/**
 * @author Marcelo Tataje (marcelo@alfa1lab.com) Goal which configure an ap
 *         according to user requirements.
 * 
 * @goal ap-config
 */
public class APConfigurationExecutor extends AbstractMojo {

	/**
	 * Represents the base directory in which all the source is located.
	 * 
	 * @parameter expression="${environment.base_dir}"
	 * @required
	 */
	private File baseDirectory;

	/**
	 * Represents the application type
	 * 
	 * @parameter expression="${apType.name}"
	 * @required
	 */
	private String module;

	/**
	 * Represents the file name of the properties file for the user.
	 * 
	 * @parameter expression="${properties.file}"
	 * @required
	 */
	private String wsdlName;

	/**
	 * Represents the file name of the properties file for the user.
	 * 
	 * @parameter expression="${properties.file}"
	 * @required
	 */
	private String file;

	/**
	 * Execute plugin to configure START AP for user.
	 */
	public void execute() throws MojoExecutionException {
		Hashtable<String, String> apPropsMap = null;
		StringBuilder sb = null;
		File apSrcDirectory = null;
		File propsFile = null;
		
		sb = new StringBuilder();
		sb.append(baseDirectory.getAbsolutePath());
		sb.append(ConfigConstants.SRC_PATH);
		apSrcDirectory = new File(sb.toString());
		String parentDir = baseDirectory.getParent();
		sb.setLength(0);
		
		if (!apSrcDirectory.exists()) {
			throw new MojoExecutionException(
					ConfigConstants.INVALID_DIRECTORY_EXCEPTION);
		}
		sb.append(parentDir);
		sb.append("/");
		sb.append(file);
		propsFile = new File(sb.toString());
		sb.setLength(0);
		if (!propsFile.exists()) {
			throw new MojoExecutionException(
					ConfigConstants.PROPS_FILE_EXCEPTION);
		}

		try {
			apPropsMap = APConfigurationManager.loadPropertiesMap(propsFile);						
			
			sb.append(ConfigConstants.PROPS_MESSAGE);
			sb.append(propsFile.getAbsolutePath());
			if (getLog().isInfoEnabled()) {
				getLog().info(sb.toString());
			}
			sb.setLength(0);
			
			APConfigurationManager.setupProject(apPropsMap, module,
					apSrcDirectory, wsdlName);

		} catch (IOException e) {
			throw new MojoExecutionException(ConfigConstants.MOJO_EXCEPTION);
		} catch (Exception e) {
			throw new MojoExecutionException(ConfigConstants.MOJO_EXCEPTION);
		}

		if (getLog().isInfoEnabled()) {
			getLog().info(ConfigConstants.LOAD_MESSAGE);
		}

	}

}
