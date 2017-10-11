using Newtonsoft.Json.Linq;
using Steam.Rest;
using Steam.Web;
using System.Threading.Tasks;

namespace Steam.Tests.Data
{
    [WebInterface(Name = "TestInterface")]
    public interface ITestInterface
    {
        Task<JToken> TestMethod(string parameter, RequestOptions options = null);
    }
}
