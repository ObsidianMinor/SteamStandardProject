using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Steam.Rest;

namespace Steam.Web
{
    public interface ISteamUserAuth
    {
        /// <summary>
        /// Authenticates the user on the Web API
        /// </summary>
        /// <param name="steamid">Should be the users steamid, unencrypted.</param>
        /// <param name="sessionkey">Should be a 32 byte random blob of data, which is then encrypted with RSA using the Steam system's public key. Randomness is important here for security</param>
        /// <param name="encrypted_loginkey">Should be the users hashed loginkey, AES encrypted with the sessionkey</param>
        /// <returns></returns>
        [WebMethod(Method = HttpMethod.Post)]
        Task<JToken> AuthenticateUser(SteamId steamid, byte[] sessionkey, byte[] encrypted_loginkey);
    }
}
