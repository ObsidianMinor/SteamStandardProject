using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Steam.Web
{
    [WebInterface(Name = "IEconItems_440")]
    public interface ITf2Economy
    {
        Task<JToken> GetPlayerItems(SteamId steamid);
        Task<JToken> GetSchema([WebParameter(Optional = true)] Language? language = null);
        Task<JToken> GetSchemaURL();
        Task<JToken> GetStoreMetaData([WebParameter(Optional = true)] Language? language = null);
        Task<JToken> GetStoreStatus(); 
    }
}
