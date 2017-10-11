# Extending Steam.Net

Steam.Net hopes to be easier to use than existing libraries while still being as extendable. This article will specify how to extend Steam.Net and how to build your own systems on top of the provided client.

### Extending the client
Since the SteamNetworkClient is not sealed, you can easily inherit it and add your own methods on top.

#### Sending your own data
The client has two main methods:
 * SendAsync
 * SendJobAsync

Data is sent using the "NetworkMessage" class in Steam.Net.Messages. It contains 2 fields:
 * Header - with the relevant header info, this data is handled by the base client and can't be modified by the end user
 * Body - an object that has yet to be deserialized into a specified type. If this is a sending message it will be the type provided to one of the create methods. Otherwise you will need to deserialize it in a Deserialize method.

#### Receiving data
The client handles receiving data using the MessageReceiverAttribute class. When a method is marked with it in the SteamNetworkClient or derived type and has valid parameters it will be invoked when a message is received of the same type. Valid return types are void and Task. Valid parameters are:
 * NetworkMessage
 * object (must not be abstract)
 * Header
 * ClientHeader
 * ProtobufClientHeader
