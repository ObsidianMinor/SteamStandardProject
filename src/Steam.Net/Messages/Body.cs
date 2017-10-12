using ProtoBuf;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Steam.Net.Messages
{
    internal class Body
    {
        private readonly ArraySegment<byte>? _body;
        private readonly object _value;

        internal Body(ArraySegment<byte> body)
        {
            _body = body;
        }

        internal Body(object value)
        {
            _value = value;
        }

        internal byte[] Serialize()
        {
            if (_value == null && !_body.HasValue)
                return new byte[0];
            else if (_body.HasValue)
                return _body.Value.ToArray();

            Type type = _value.GetType();

            if (type.GetCustomAttribute<ProtoContractAttribute>() != null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    Serializer.NonGeneric.Serialize(stream, _value);
                    return stream.ToArray();
                }
            }
            else
            {
                int size = Marshal.SizeOf(_value);
                byte[] value = new byte[size];
                IntPtr ptr = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(_value, ptr, false);
                    Marshal.Copy(ptr, value, 0, size);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
                return value;
            }
        }

        internal object Deserialize(Type type)
        {
            if (_value != null && (_value.GetType().IsSubclassOf(type) || _value.GetType() == type))
                return _value;

            if (type.GetCustomAttribute<ProtoContractAttribute>() != null)
            {
                using (MemoryStream stream = new MemoryStream(_body.Value.Array, _body.Value.Offset, _body.Value.Count, false))
                    return Serializer.Deserialize(type, stream);
            }
            else
            {
                var data = _body?.ToArray();
                var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    return Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
                }
                finally
                {
                    handle.Free();
                }
            }
        }
    }
}
