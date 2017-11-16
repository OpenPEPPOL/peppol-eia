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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Web;
using System.IO;

namespace STARTLibrary.src.eu.peppol.start.xml.request
{
    public class StartRequest
    {
        /// <summary>
        /// Get response content from Url
        /// </summary>
        public string GetContent(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                ForceCanonicalUrl(uri);
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                httpWebRequest.Method = "GET";
                httpWebRequest.ContentType = "text/xml; encoding='utf-8'";
                httpWebRequest.UserAgent = "Mozilla/4.0+";
                httpWebRequest.KeepAlive = true;

                using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    StreamReader streamReader = new StreamReader(response.GetResponseStream());
                    return streamReader.ReadToEnd();
                }
            }
            catch (Exception)
            { return null; }
        }

        /// <summary>
        /// Force canonical Url structure
        /// </summary>
        public void ForceCanonicalUrl(Uri uri)
        {
            string paq = uri.PathAndQuery;
            System.Reflection.FieldInfo flagsFieldInfo = typeof(Uri).GetField("m_Flags", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            ulong flags = (ulong)flagsFieldInfo.GetValue(uri);
            flags &= ~((ulong)0x30);
            flagsFieldInfo.SetValue(uri, flags);
        }
    }
}
