using ProtoBuf;
using Steam.Net.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Steam.Net.Messages.Serialization
{
    internal static class MessageSerializer
    {
        internal static async Task<byte[]> SerializeMessageAsync(IMessage message)
        {
            Type messageType = message.GetType();
            uint type = MessageTypeUtils.MergeMessage(message.Type, message.IsProtobuf);
            
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // write header
                    Type headerType = GetImplementedInterfaceType(messageType, typeof(IHeader<>));
                    if(headerType == typeof(ProtobufHeader))
                    {
                        writer.Write(type);
                        await WritePropertiesAsync(messageType.GetProperty("Header").GetValue(message), DiscoverFieldProperties(headerType).Skip(1), writer);
                    }
                    else
                    {
                        await WritePropertiesAsync(messageType.GetProperty("Header").GetValue(message), DiscoverFieldProperties(headerType), writer);
                    }
                    
                    // write body
                    if (message.IsProtobuf)
                        Serializer.NonGeneric.Serialize(writer.BaseStream, messageType.GetProperty("Body").GetValue(message));
                    else
                    {
                        Type bodyType = GetImplementedInterfaceType(messageType, typeof(IStructBody<>));
                        await WritePropertiesAsync(messageType.GetProperty("Body").GetValue(message), DiscoverFieldProperties(bodyType), writer);
                    }

                    // write remaining payload
                    IPayload payload = message as IPayload;
                    if(payload.Payload != null)
                        writer.Write(payload.Payload);
                }

                return stream.ToArray();
            }
        }

        internal static async Task<T> DeserializeProtobufMessageAsync<T, TBody>(byte[] data) where T : IProtobufBody<TBody>, new() where TBody : IExtensible, new()
        {
            Type returnObjectType = typeof(T);
            T returnObject = new T();
            if (!(returnObject is IMessage message))
                throw new InvalidCastException("Type T is not a message type");

            if (!(returnObject is IProtobufBody<TBody> protobuf))
                throw new InvalidCastException("Type T isn't protobuf backed");

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                reader.ReadUInt32(); // read and discard the type

                Type headerType = GetImplementedInterfaceType(returnObjectType, typeof(IHeader<>));
                returnObjectType.GetProperty("Header").SetValue(returnObject, await PopulatePropertiesAsync(headerType, DiscoverFieldProperties(headerType).Skip(1), reader));
                protobuf.Body = Serializer.Deserialize<TBody>(reader.BaseStream);

                IPayload payloadObject = (returnObject as IPayload);
                payloadObject.Payload = reader.ReadToEnd();
            }

            return returnObject;
        }

        internal static async Task<T> DeserializeStructMessageAsync<T, TBody>(byte[] data) where T : IStructBody<TBody>, new() where TBody : new()
        {
            Type returnObjectType = typeof(T);
            T returnObject = new T();
            if (!(returnObject is IMessage message))
                throw new InvalidCastException("Type T is not a message type");

            if (!(returnObject is IStructBody<TBody> structBody))
                throw new InvalidCastException("Type T isn't protobuf backed");

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Type headerType = GetImplementedInterfaceType(returnObjectType, typeof(IHeader<>));
                Task<object> populatePropertiesTask = PopulatePropertiesAsync(headerType, DiscoverFieldProperties(headerType), reader);

                Type bodyType = typeof(TBody);
                var bodyProperties = DiscoverFieldProperties(bodyType);
                returnObjectType.GetProperty("Header").SetValue(returnObject, await populatePropertiesTask);

                structBody.Body = (TBody)await PopulatePropertiesAsync(bodyType, bodyProperties, reader);

                IPayload payloadObject = (returnObject as IPayload);
                payloadObject.Payload = reader.ReadToEnd();
            }

            return returnObject;
        }
        
        internal static async Task<T> DeserializeMessageAsync<T>(byte[] data) where T : new()
        {
            Type returnObjectType = typeof(T);
            T returnObject = new T();
            if (!(returnObject is IMessage message))
                throw new InvalidCastException("Type T is not a message type");

            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Type headerType = GetImplementedInterfaceType(returnObjectType, typeof(IHeader<>));
                returnObjectType.GetProperty("Header").SetValue(returnObject, await PopulatePropertiesAsync(headerType, DiscoverFieldProperties(headerType), reader));

                IPayload payloadObject = (returnObject as IPayload);
                payloadObject.Payload = reader.ReadToEnd();
            }

            return returnObject;
        }

        private static Type GetImplementedInterfaceType(Type rootType, Type interfaceType)
        {
            Type headerInterface = rootType.GetInterfaces().FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == interfaceType);
            if (headerInterface == null)
                throw new InvalidCastException("Type T does not implement IHeader");

            return headerInterface.GenericTypeArguments.Single();
        }

        private static IEnumerable<PropertyInfo> DiscoverFieldProperties(Type returnType)
        {
            return returnType.GetRuntimeProperties()
                .Select(prop => (prop, prop.GetCustomAttribute<PacketFieldOrderAttribute>()))
                .Where(attribute => attribute.Item2 != null)
                .OrderBy(propInfo => propInfo.Item2.Position)
                .Select(prop => prop.Item1);
        }

        private static async Task WritePropertiesAsync(object instance, IEnumerable<PropertyInfo> properties, BinaryWriter writer)
        {
            await Task.Factory.StartNew(() => WritePropertiesInternal(instance, properties, writer)).ConfigureAwait(false);
        }

        private static void WritePropertiesInternal(object instance, IEnumerable<PropertyInfo> properties, BinaryWriter writer)
        {
            List<PropertyInfo> propertiesList = properties.ToList();
            for(int i = 0; i < propertiesList.Count; i++)
            {
                PropertyInfo prop = propertiesList[i];
                if (propertiesList[i + (i == propertiesList.Count - 1 ? 0 : 1)].PropertyType == typeof(Protobufs.ProtobufHeader))
                {
                    byte[] streamBytes = null;
                    using (MemoryStream stream = new MemoryStream())
                    {
                        Serializer.NonGeneric.Serialize(stream, propertiesList[i + 1].GetValue(instance));
                        streamBytes = stream.ToArray();
                    }
                    writer.Write(streamBytes.Length);
                    writer.Write(streamBytes);
                    i++;
                }
                else
                    writer.Write(prop.GetValue(instance));
            }
        }

        private static async Task<object> PopulatePropertiesAsync(Type type, IEnumerable<PropertyInfo> properties, BinaryReader reader)
        {
            return await Task.Factory.StartNew(() => PopulatePropertiesInternal(type, properties, reader)).ConfigureAwait(false);
        }

        private static object PopulatePropertiesInternal(Type type, IEnumerable<PropertyInfo> properties, BinaryReader reader)
        {
            object headerInstance = Activator.CreateInstance(type);
            List<PropertyInfo> propertiesList = properties.ToList();
            for(int i = 0; i < propertiesList.Count; i++)
            {
                PropertyInfo prop = propertiesList[i];
                if (propertiesList[i + (i == propertiesList.Count - 1 ? 0 : 1)].PropertyType == typeof(Protobufs.ProtobufHeader))
                {
                    int length = reader.ReadInt32();
                    prop.SetValue(headerInstance, length);
                    using (MemoryStream stream = new MemoryStream(reader.ReadBytes(length)))
                    {
                        PropertyInfo protobufProp = propertiesList[i + 1];
                        object deserialized = Serializer.NonGeneric.Deserialize(protobufProp.PropertyType, stream);
                        protobufProp.SetValue(headerInstance, deserialized);
                    }
                    i++;
                }
                else
                    SetProperty(headerInstance, prop, reader);
            }
            return headerInstance;
        }

        private static void SetProperty(object instance, PropertyInfo prop, BinaryReader reader)
        {
            Type propType = prop.PropertyType;
            if (propType == typeof(byte))
                prop.SetValue(instance, reader.ReadByte());
            else if (propType == typeof(short))
                prop.SetValue(instance, reader.ReadInt16());
            else if (propType == typeof(ushort))
                prop.SetValue(instance, reader.ReadUInt16());
            else if (propType == typeof(int))
                prop.SetValue(instance, reader.ReadInt32());
            else if (propType == typeof(uint))
                prop.SetValue(instance, reader.ReadUInt32());
            else if (propType == typeof(long))
                prop.SetValue(instance, reader.ReadInt64());
            else if (propType == typeof(ulong))
                prop.SetValue(instance, reader.ReadUInt64());
            else if (propType.GetTypeInfo().IsEnum)
                prop.SetValue(instance, Enum.ToObject(propType, ReadEnumValue(propType, reader)));
            else if (propType == typeof(byte[]))
            {
                byte[] bytes = (byte[])prop.GetValue(instance);
                prop.SetValue(instance, reader.ReadBytes(bytes.Length));
            }
            else
                throw new InvalidOperationException($"Unknown property type: {propType}");
        }

        private static object ReadEnumValue(Type enumType, BinaryReader reader)
        {
            Type underlyingType = Enum.GetUnderlyingType(enumType);
            if (underlyingType == typeof(byte))
                return reader.ReadByte();
            else if (underlyingType == typeof(sbyte))
                return reader.ReadSByte();
            else if (underlyingType == typeof(short))
                return reader.ReadInt16();
            else if (underlyingType == typeof(ushort))
                return reader.ReadUInt16();
            else if (underlyingType == typeof(int))
                return reader.ReadInt32();
            else if (underlyingType == typeof(uint))
                return reader.ReadUInt32();
            else if (underlyingType == typeof(long))
                return reader.ReadInt64();
            else
                return reader.ReadUInt64();
        }
    }
}
