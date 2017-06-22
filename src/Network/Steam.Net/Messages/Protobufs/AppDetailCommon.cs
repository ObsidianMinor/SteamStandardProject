using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    internal class AppDetailCommon : IExtensible
    {
        #warning DO THIS YOU IDIOT
        
        private IExtension _extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref _extensionObject, createIfMissing);
        }
    }
}
