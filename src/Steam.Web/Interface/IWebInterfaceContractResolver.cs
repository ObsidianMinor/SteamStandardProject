using System;
using System.Reflection;

namespace Steam.Web.Interface
{
    /// <summary>
    /// Resolves interfaces provided to <see cref="SteamWebClient.GetInterface{T}"/>
    /// </summary>
    public interface IWebInterfaceContractResolver
    {
        /// <summary>
        /// Resolves the provided interface type
        /// </summary>
        /// <param name="type">The type of the interface</param>
        /// <returns>A contract for the provided type</returns>
        WebInterfaceContract ResolveInterface(Type type);

        WebMethodContract ResolveMethod(MethodInfo method);
    }
}
