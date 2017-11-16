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
using System.Text;
using System.ServiceModel.Channels;
using System.IO;
using System.ServiceModel.Description;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Collections.Generic;

namespace STARTLibrary.src.eu.peppol.start.security.encoder
{
    /// <summary>
    /// A custom message encoder which accepts both messages with MTOM and with MTOM encoding and emits
    /// text encoded messages.
    /// </summary>
    /// <remarks>
    /// According to the START specification, 
    /// 
    ///   "Access Points MUST support MTOM when acting as a service endpoint – that is while receiving 
    ///    messages from another Access Point. APs MAY send messages using MTOM."
    ///
    /// So APs may or may not send messages using MTOM.
    /// 
    /// With the current architecture we can only have one endpoint for each combination of id, document 
    /// type and process type, therefore it is not possible to simply provide different endpoints for 
    /// different encodings. Instead, an AP's endpoint must accept both messages with MTOM encoding and 
    /// messages with regular text encoding.
    /// 
    /// This class defines a new custom encoder which wraps both types of encoders (MTOM and regular text)
    /// and delegates to the appropiate encoder when reading and writing messages.
    /// 
    /// Ideally, responses should be sent with the same encoding. This was partly implemented (in 
    /// revision 3042), but requires some workarounds (because the .NET framework classes clearly expect
    /// all encoders to use only one content type for outbound messages).
    /// 
    /// Based on http://social.msdn.microsoft.com/Forums/en-US/wcf/thread/480f1bc4-1fc4-40e9-a2ed-efcf3009d6ef
    /// Additional implementation from http://msdn.microsoft.com/en-us/library/ms751486(VS.85).aspx (If you 
    /// download the samples for Windows Communication Foundation (WCF) and Windows CardSpace, the source 
    /// code can be found in C:\Samples\WCFWFCardSpace\WCF\Extensibility\MessageEncoder\Text\CS)
    /// </remarks>
    /// <example>
    /// When specifying the custom encoder in the configuration file ...
    /// 
    ///     &lt;mixedContentTypeMessageEncoding messageVersion="Soap11WSAddressing10" writeEncoding="utf-8" /&gt;
    /// 
    /// ... a configuration handler must be implemented and registered in the "extensions" configuration 
    /// element:
    /// 
    ///     &lt;extensions&gt;
    ///       &lt;bindingElementExtensions&gt;
    ///         &lt;add name="mixedContentTypeMessageEncoding"
    ///           type="eu.peppol.start.src.eu.peppol.start.common.MixedContentTypeMessageEncodingExtensionElement, STARTLibrary" /&gt;
    ///       &lt;/bindingElementExtensions&gt;
    ///     &lt;/extensions&gt;
    /// </example>
    public class MixedContentTypeMessageEncoder : MessageEncoder
    {
        private readonly MessageEncoder textEncoder;
        private readonly MessageEncoder mtomEncoder;

        public MixedContentTypeMessageEncoder(MessageVersion messageVersion, Encoding writeEncoding)
        {
            textEncoder = new TextMessageEncodingBindingElement(messageVersion, writeEncoding).CreateMessageEncoderFactory().Encoder;
            mtomEncoder = new MtomMessageEncodingBindingElement(messageVersion, writeEncoding).CreateMessageEncoderFactory().Encoder;
        }

        /// <summary>
        /// The MIME content type that is supported by the message encoder.
        /// </summary>
        public override string ContentType
        {
            // Outbound messages always use text encoding
            get { return textEncoder.ContentType; }
        }

        /// <summary>
        /// The media type that is supported by the message encoder.
        /// </summary>
        public override string MediaType
        {
            get { return mtomEncoder.MediaType; }
        }

        /// <summary>
        /// The System.ServiceModel.Channels.MessageVersion that is used by the encoder.
        /// </summary>
        public override MessageVersion MessageVersion
        {
            get { return mtomEncoder.MessageVersion; }
        }

        /// <summary>
        /// Returns a value that indicates whether a specified message-level content-type 
        /// value is supported by the message encoder.
        /// </summary>
        /// <param name="contentType">The message-level content-type being tested.</param>
        /// <returns>true if the message-level content-type specified is supported; otherwise false.</returns>
        public override bool IsContentTypeSupported(string contentType)
        {
            // The MTOM encoder also supports "text/xml; charset=utf-8", so it
            // is sufficient to ask this encoder:
            return mtomEncoder.IsContentTypeSupported(contentType);
        }

        /// <summary>
        /// Returns a typed object requested, if present, from the appropriate layer 
        /// in the channel stack.
        /// </summary>
        /// <typeparam name="T">The typed object for which the method is querying.</typeparam>
        /// <returns>The typed object T requested if it is present or null if it is not.</returns>
        public override T GetProperty<T>()
        {
            T result = textEncoder.GetProperty<T>();

            if (result == null)
            {
                result = mtomEncoder.GetProperty<T>();
            }

            if (result == null)
            {
                result = base.GetProperty<T>();
            }

            return result;
        }

        /// <summary>
        /// Reads a message from a specified buffer.
        /// </summary>
        /// <param name="buffer">A System.ArraySegment of type System.Byte that provides the 
        /// buffer from which the message is deserialized.</param>
        /// <param name="bufferManager">The System.ServiceModel.Channels.BufferManager that manages 
        /// the buffer from which the message is deserialized.</param>
        /// <param name="contentType">The Multipurpose Internet Mail Extensions (MIME) 
        /// message-level content-type.</param>
        /// <returns>The System.ServiceModel.Channels.Message that is read from the buffer 
        /// specified.</returns>
        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            return GetEncoderForContentType(contentType).ReadMessage(buffer, bufferManager, contentType);
        }

        /// <summary>
        /// Reads a message from a specified stream.
        /// </summary>
        /// <param name="stream">The System.IO.Stream object from which the message is read.</param>
        /// <param name="maxSizeOfHeaders">The maximum size of the headers that can be read from 
        /// the message.</param>
        /// <param name="contentType">The Multipurpose Internet Mail Extensions (MIME) 
        /// message-level content-type.</param>
        /// <returns>The System.ServiceModel.Channels.Message that is read from the stream specified.</returns>
        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            return GetEncoderForContentType(contentType).ReadMessage(stream, maxSizeOfHeaders, contentType);
        }

        /// <summary>
        /// Writes a message of less than a specified size to a byte array buffer at the 
        /// specified offset.
        /// </summary>
        /// <param name="message">The System.ServiceModel.Channels.Message to write to the 
        /// message buffer.</param>
        /// <param name="maxMessageSize">The maximum message size that can be written.</param>
        /// <param name="bufferManager">The System.ServiceModel.Channels.BufferManager 
        /// that manages the buffer to which the message is written.</param>
        /// <param name="messageOffset">The offset of the segment that begins from the 
        /// start of the byte array that provides the buffer.</param>
        /// <returns>A System.ArraySegment of type byte that provides the buffer to which 
        /// the message is serialized.</returns>
        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            // Outbound messages always use text encoding
            return textEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
        }

        /// <summary>
        /// Writes a message to a specified stream.
        /// </summary>
        /// <param name="message">The System.ServiceModel.Channels.Message to write to 
        /// the stream.</param>
        /// <param name="stream">The System.IO.Stream object to which the message is 
        /// written.</param>
        public override void WriteMessage(Message message, Stream stream)
        {
            // Outbound messages always use text encoding
            textEncoder.WriteMessage(message, stream);
        }

        /// <summary>
        /// Determines the encoder to use with the given content type.
        /// </summary>
        /// <param name="contentType">
        /// The Multipurpose Internet Mail Extensions (MIME) message-level content-type.</param>
        /// <returns>
        /// The text encoder if it supports the given content type, the MTOM encoder otherwise.
        /// </returns>
        private MessageEncoder GetEncoderForContentType(string contentType)
        {
            return textEncoder.IsContentTypeSupported(contentType) ? textEncoder : mtomEncoder;
        }
    }

    /// <summary>
    /// A MessageEncodingBindingElement using the MixedContentTypeMessageEncoder.
    /// </summary>
    public class MixedContentTypeMessageEncodingBindingElement : MessageEncodingBindingElement, IWsdlExportExtension
    {
        private MessageVersion messageVersion = MessageVersion.Default;
        private Encoding writeEncoding = Encoding.UTF8;

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new MixedContentTypeMessageEncoderFactory(this.messageVersion, this.writeEncoding);
        }

        public override MessageVersion MessageVersion
        {
            get { return messageVersion; }
            set { messageVersion = value; }
        }

        public Encoding WriteEncoding
        {
            get { return writeEncoding; }
            set { writeEncoding = value; }
        }

        public override BindingElement Clone()
        {
            return this;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        //From http://msdn.microsoft.com/en-us/library/ms751486(VS.85).aspx
        #region IWsdlExportExtension Members
        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
        }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            // The MessageEncodingBindingElement is responsible for ensuring that the WSDL has the correct
            // SOAP version. We can delegate to the WCF implementation of TextMessageEncodingBindingElement for this.
            TextMessageEncodingBindingElement mebe = new TextMessageEncodingBindingElement();
            mebe.MessageVersion = messageVersion;
            mebe.WriteEncoding = WriteEncoding;
            ((IWsdlExportExtension)mebe).ExportEndpoint(exporter, context);
        }
        #endregion
    }

    /// <summary>
    /// A factory for producing message encoders that can read both MTOM encoded messages and 
    /// messages with text encoding.
    /// Used by the MixedContentTypeMessageEncodingBindingElement.
    /// </summary>
    public class MixedContentTypeMessageEncoderFactory : MessageEncoderFactory
    {
        private MessageVersion messageVersion;
        private Encoding writeEncoding;
        private MixedContentTypeMessageEncoder encoder;

        public MixedContentTypeMessageEncoderFactory(MessageVersion messageVersion, Encoding writeEncoding)
        {
            this.messageVersion = messageVersion;
            this.writeEncoding = writeEncoding;
            this.encoder = new MixedContentTypeMessageEncoder(messageVersion, writeEncoding);
        }

        public override MessageEncoder Encoder { get { return this.encoder; } }
        public Encoding WriteEncoding { get { return writeEncoding; } }
        public override MessageVersion MessageVersion { get { return this.encoder.MessageVersion; } }
    }

    /// <summary>
    /// Enables the use of the MixedContentTypeMessageEncodingBindingElement implementation 
    /// from a configuration file.
    /// </summary>
    /// <remarks>
    /// From http://msdn.microsoft.com/en-us/library/ms751486(VS.85).aspx
    /// </remarks>
    public class MixedContentTypeMessageEncodingExtensionElement : BindingElementExtensionElement
    {
        public MixedContentTypeMessageEncodingExtensionElement()
        {
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            MixedContentTypeMessageEncodingBindingElement binding = (MixedContentTypeMessageEncodingBindingElement)bindingElement;
            binding.MessageVersion = MessageVersion;
            binding.WriteEncoding = WriteEncoding;
        }

        public override Type BindingElementType
        {
            get
            {
                return typeof(MixedContentTypeMessageEncodingBindingElement);
            }
        }

        protected override BindingElement CreateBindingElement()
        {
            MixedContentTypeMessageEncodingBindingElement binding = new MixedContentTypeMessageEncodingBindingElement();
            this.ApplyConfiguration(binding);
            return binding;
        }

        [ConfigurationProperty(ConfigurationStrings.MessageVersion,
            DefaultValue = ConfigurationStrings.DefaultMessageVersion)]
        [TypeConverter(typeof(MessageVersionConverter))]
        public MessageVersion MessageVersion
        {
            get { return (MessageVersion)base[ConfigurationStrings.MessageVersion]; }
            set { base[ConfigurationStrings.MessageVersion] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.WriteEncoding,
           DefaultValue = ConfigurationStrings.DefaultWriteEncoding)]
        [TypeConverter(typeof(WriteEncodingConverter))]
        public Encoding WriteEncoding
        {
            get { return (Encoding)base[ConfigurationStrings.WriteEncoding]; }
            set { base[ConfigurationStrings.WriteEncoding] = value; }
        }

        class ConfigurationStrings
        {
            internal const string MessageVersion = "messageVersion";
            internal const string WriteEncoding = "writeEncoding";

            internal const string None = "None";
            internal const string Default = "Default";
            internal const string Soap11 = "Soap11";
            internal const string Soap11WSAddressing10 = "Soap11WSAddressing10";
            internal const string Soap11WSAddressingAugust2004 = "Soap11WSAddressingAugust2004";
            internal const string Soap12 = "Soap12";
            internal const string Soap12WSAddressing10 = "Soap12WSAddressing10";
            internal const string Soap12WSAddressingAugust2004 = "Soap12WSAddressingAugust2004";

            internal const string UTF8 = "UTF-8";
            internal const string UTF7 = "UTF-7";
            internal const string UTF32 = "UTF-32";
            // Only the common UTF encodings are defined at the moment; if you 
            // need more, you can extend this list and adjust the ConvertFrom 
            // and ConvertTo implementations in WriteEncodingConverter

            internal const string DefaultMessageVersion = Soap12WSAddressing10;
            internal const string DefaultWriteEncoding = UTF8;
        }

        class MessageVersionConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (typeof(string) == sourceType)
                {
                    return true;
                }
                return base.CanConvertFrom(context, sourceType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (typeof(InstanceDescriptor) == destinationType)
                {
                    return true;
                }
                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                {
                    string messageVersion = (string)value;
                    MessageVersion result;
                    switch (messageVersion)
                    {
                        case ConfigurationStrings.Soap11WSAddressing10:
                            result = MessageVersion.Soap11WSAddressing10;
                            break;
                        case ConfigurationStrings.Soap12WSAddressing10:
                            result = MessageVersion.Soap12WSAddressing10;
                            break;
                        case ConfigurationStrings.Soap11WSAddressingAugust2004:
                            result = MessageVersion.Soap11WSAddressingAugust2004;
                            break;
                        case ConfigurationStrings.Soap12WSAddressingAugust2004:
                            result = MessageVersion.Soap12WSAddressingAugust2004;
                            break;
                        case ConfigurationStrings.Soap11:
                            result = MessageVersion.Soap11;
                            break;
                        case ConfigurationStrings.Soap12:
                            result = MessageVersion.Soap12;
                            break;
                        case ConfigurationStrings.None:
                            result = MessageVersion.None;
                            break;
                        case ConfigurationStrings.Default:
                            result = MessageVersion.Default;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("value");
                    }
                    return result;
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (typeof(string) == destinationType && value is MessageVersion)
                {
                    string result;
                    MessageVersion messageVersion = (MessageVersion)value;
                    if (messageVersion == MessageVersion.Default)
                    {
                        result = ConfigurationStrings.Default;
                    }
                    else if (messageVersion == MessageVersion.Soap11WSAddressing10)
                    {
                        result = ConfigurationStrings.Soap11WSAddressing10;
                    }
                    else if (messageVersion == MessageVersion.Soap12WSAddressing10)
                    {
                        result = ConfigurationStrings.Soap12WSAddressing10;
                    }
                    else if (messageVersion == MessageVersion.Soap11WSAddressingAugust2004)
                    {
                        result = ConfigurationStrings.Soap11WSAddressingAugust2004;
                    }
                    else if (messageVersion == MessageVersion.Soap12WSAddressingAugust2004)
                    {
                        result = ConfigurationStrings.Soap12WSAddressingAugust2004;
                    }
                    else if (messageVersion == MessageVersion.Soap11)
                    {
                        result = ConfigurationStrings.Soap11;
                    }
                    else if (messageVersion == MessageVersion.Soap12)
                    {
                        result = ConfigurationStrings.Soap12;
                    }
                    else if (messageVersion == MessageVersion.None)
                    {
                        result = ConfigurationStrings.None;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    return result;
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        class WriteEncodingConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (typeof(string) == sourceType)
                {
                    return true;
                }
                return base.CanConvertFrom(context, sourceType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (typeof(InstanceDescriptor) == destinationType)
                {
                    return true;
                }
                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                {
                    string writeEncoding = (string)value;
                    Encoding result;
                    switch (writeEncoding.ToUpper())
                    {
                        case ConfigurationStrings.UTF8:
                            result = Encoding.UTF8;
                            break;
                        case ConfigurationStrings.UTF7:
                            result = Encoding.UTF7;
                            break;
                        case ConfigurationStrings.UTF32:
                            result = Encoding.UTF32;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("value");
                    }
                    return result;
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (typeof(string) == destinationType && value is Encoding)
                {
                    string result;
                    Encoding writeEncoding = (Encoding)value;

                    if (Encoding.UTF8.Equals(writeEncoding))
                    {
                        result = ConfigurationStrings.UTF8;
                    }
                    else if (Encoding.UTF7.Equals(writeEncoding))
                    {
                        result = ConfigurationStrings.UTF7;
                    }
                    else if (Encoding.UTF32.Equals(writeEncoding))
                    {
                        result = ConfigurationStrings.UTF32;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    return result;
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}