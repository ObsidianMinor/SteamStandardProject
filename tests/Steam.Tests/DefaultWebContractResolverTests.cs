using Steam.Web.Interface;
using Xunit;
using Steam.Tests.Data;
using Steam.Tests.Mocks;
using Steam.Rest;
using Steam.Web;

namespace Steam.Tests
{
    [Trait("Category", "DefaultWebContractResolverTests")]
    public class DefaultWebContractResolverTests
    {
        [Fact(DisplayName = "Resolve interface")]
        public void ResolveInterface()
        {
            var contract = DefaultWebInterfaceResolver.Instance.ResolveInterface(typeof(ITestInterface));
            Assert.Equal("TestInterface", contract.Name);
            Assert.False(contract.IsService);
        }

        [Fact(DisplayName = "Resolve method")]
        public void ResolveMethod()
        {
            var contract = DefaultWebInterfaceResolver.Instance.ResolveMethod(typeof(ITestInterface).GetMethod(nameof(ITestInterface.TestMethod)));
            Assert.Equal("TestMethod", contract.Name);
            Assert.Equal(HttpMethod.Get, contract.Method);
        }

        [Fact(DisplayName = "Send")]
        public async void SendOnMethod()
        {
            AssertingRestClient client = new AssertingRestClient(HttpMethod.Get, "https://api.steampowered.com/TestInterface/TestMethod/v1/?format=json&parameter=thing", "{response:{success:true}}");
            SteamWebClient webClient = new SteamWebClient(new SteamWebConfig { RestClient = () => client });
            var intface = webClient.GetInterface<ITestInterface>();
            var response = await intface.TestMethod("thing");
            Assert.True((bool)response["response"]["success"]);
        }
    }
}
