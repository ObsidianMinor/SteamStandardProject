using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Steam.Web
{
    [WebInterface(IsService = true)]
    public interface IEconMarketService
    {
        /// <summary>
        /// Checks whether or not an account is allowed to use the market
        /// </summary>
        /// <param name="steamid">The SteamID of the user to check</param>
        /// <returns></returns>
        [WebMethod(Name = "GetMarketEligibility", RequireKey = true)]
        Task<JToken> GetMarketEligibilityAsync(SteamId steamid);
    }
}
