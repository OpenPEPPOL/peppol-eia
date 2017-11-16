/*Version: MPL 1.1/EUPL 1.1
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
 * http://www.osor.eu/eupl/european-union-public-licence-eupl-v.1.1
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
 * thev provisions above, a recipient may use your version of this file
 * under either the MPL or the EUPL License.
 */

using System;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using STARTLibrary.accesspointService;

namespace STARTLibrary.src.eu.peppol.start.io
{
    public class IOLayer
    {
        private const string DOCUMENTEXTENSION = "payload.xml";
        private const string METADATAEXTENSION = "metadata.xml";
        private const string _NamePrefix = "uuid";

        private DirectoryInfo applicationData;
        private DirectoryInfo peppolFolder;
        private DirectoryInfo messageStoreFolder;
        private DirectoryInfo inboxDirectory;

        public IOLayer()
        {
            string applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            applicationData = new DirectoryInfo(applicationDataPath);
            peppolFolder = new DirectoryInfo(applicationData.FullName + Path.DirectorySeparatorChar + "Peppol");
            if (!peppolFolder.Exists) peppolFolder.Create();
            messageStoreFolder = new DirectoryInfo(peppolFolder.FullName + Path.DirectorySeparatorChar + "MessageStore");
            if (!messageStoreFolder.Exists) messageStoreFolder.Create();
            inboxDirectory = new DirectoryInfo(messageStoreFolder.FullName + @"\Inbox");
            if (!inboxDirectory.Exists) inboxDirectory.Create();
        }

        /// <summary>
        /// Check for illegal characters in identifier for future inbox directory creation
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        private void CheckForIllegalIdentifierCharacters(string identifier)
        {
            if (identifier.Contains('/')
                || identifier.Contains('\\')
                || identifier.Contains("..")
                || identifier.Contains('*'))
            {
                throw new Exception("Identifier '" + identifier + "' contains illegal, potentially unsecure characters");
            }
        }

        /// <summary>
        /// Get the specific channel folder of the inbox directory
        /// </summary>
        /// <param name="channelIdentifier"></param>
        /// <returns></returns>
        public DirectoryInfo GetInboxChannelDirectory(string channelIdentifier)
        {
            CheckForIllegalIdentifierCharacters(channelIdentifier);

            channelIdentifier = channelIdentifier.Replace(":", "_");
            DirectoryInfo channelDirectory = new DirectoryInfo(inboxDirectory.FullName + @"\" + channelIdentifier);

            if (!channelDirectory.Exists) channelDirectory.Create();

            return channelDirectory;
        }

        /// <summary>
        /// Get associated document file
        /// </summary>
        /// <returns>File info for the document</returns>
        public FileInfo GetDocumentFile(string messageIdentifier, DirectoryInfo channelDirectory)
        {
            CheckForIllegalIdentifierCharacters(messageIdentifier);

            StringBuilder pathWithoutExtension = CreatePathWithoutExtension(channelDirectory, messageIdentifier);
            pathWithoutExtension.Append(DOCUMENTEXTENSION);
            FileInfo fileInfo = new FileInfo(pathWithoutExtension.ToString());
            return fileInfo;
        }

        /// <summary>
        /// Get associated metadata file
        /// </summary>
        /// <returns>File info object of the metadata file</returns>
        public FileInfo GetMetadataFile(string messageIdentifier, DirectoryInfo channelDirectory)
        {
            StringBuilder sb = CreatePathWithoutExtension(channelDirectory, messageIdentifier);
            sb.Append(METADATAEXTENSION);
            string path = sb.ToString();
            FileInfo fileInfo = new FileInfo(path);
            return fileInfo;
        }

        /// <summary>
        /// Get path from ap
        /// </summary>
        /// <returns>Path from ap</returns>
        private StringBuilder CreatePathWithoutExtension(DirectoryInfo channelDirectory, string messageIdentifier)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(channelDirectory.FullName);
            sb.Append(Path.DirectorySeparatorChar);
            sb.Append(_NamePrefix);
            sb.Append("_");
            sb.Append(messageIdentifier);
            sb.Append(".");
            return sb;
        }

        /// <summary>
        /// Register metadata and document sended
        /// </summary>        
        public void Insert(IMessage message)
        {
            DirectoryInfo currentChannelDirectory = this.GetInboxChannelDirectory(message.ChannelIdentifier);
            FileInfo documentFile = GetDocumentFile(message.MessageIdentifier.ToString(), currentChannelDirectory);
            FileInfo metadataFile = GetMetadataFile(message.MessageIdentifier.ToString(), currentChannelDirectory);
            WriteXmlDocument(message.Document, documentFile);
            WriteMetadata(message.Metadata, metadataFile);
        }

        /// <summary>
        /// Writes metadata to filesystem in xml format
        /// </summary>
        public void WriteMetadata(IMetadata metadata, FileInfo metadataFile)
        {
            StreamWriter streamWriter = null;
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(metadata.GetType());
                streamWriter = new StreamWriter(metadataFile.FullName, false, Encoding.UTF8);
                xmlSerializer.Serialize(streamWriter, metadata);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save metadata document due to error: " + ex.Message);
            }
            finally
            {
                if (streamWriter != null) streamWriter.Close();
            }
        }

        /// <summary>
        /// Deserialize the xml document on the filesystem into the metadata object
        /// </summary>
        public Metadata GetMetadata(FileInfo metadataFile)
        {
            XmlReader xmlReader = null;
            try
            {
                xmlReader = XmlReader.Create(metadataFile.FullName);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Metadata));
                Metadata metadata = (Metadata)xmlSerializer.Deserialize(xmlReader);
                return metadata;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get metadata document due to error: " + ex.Message);
            }
            finally
            {
                if (xmlReader != null) xmlReader.Close();
            }
        }

        /// <summary>
        /// Write an xml document to a file.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="documentFile"></param>
        private void WriteXmlDocument(XmlDocument document, FileInfo documentFile)
        {
            try
            {
                if (string.IsNullOrEmpty(document.InnerXml))
                {
                    document.LoadXml("<EmptyMessage />");
                }

                //Remove White spaces
                document.PreserveWhitespace = false;

                //Create an Xml declaration.
                XmlDeclaration xmldecl;
                xmldecl = document.CreateXmlDeclaration("1.0", "utf-8", null);

                //Add the new node to the document.
                XmlElement root = document.DocumentElement;
                document.InsertBefore(xmldecl, root);

                document.Save(documentFile.FullName);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save xml document due to error: " + ex.Message);
            }
        }

        /// <summary>
        /// Verifies that a document and metadata with known document identifier exists on inbox, 
        /// for a specific channel
        /// </summary>
        /// <param name="channelIdentifier">The channel identifier</param>
        /// <param name="messageIdentifier">The message identifier</param>
        /// <returns></returns>
        public bool DocumentAndMetadataExist(string channelIdentifier, string messageIdentifier)
        {
            DirectoryInfo channelDir = GetInboxChannelDirectory(channelIdentifier);
            FileInfo docFile = this.GetDocumentFile(messageIdentifier, channelDir);
            FileInfo metaFile = this.GetMetadataFile(messageIdentifier, channelDir);
            if (docFile == null || metaFile == null) return false;
            if (!docFile.Exists || !metaFile.Exists) return false;
            else return true;
        }
    }
}

