using Steam.Common.Logging;
using System.Threading.Tasks;
using Xunit;

namespace Steam.Tests.Common
{
    [Trait("Category", "Logging")]
    public class LoggingTests
    {
        [Fact(DisplayName = "Debug")]
        public async void LogDebug()
        {
            LogManager manager = new LogManager("TEST", LogSeverity.Debug);
            manager.Log += (src, message) =>
            {
                Assert.Equal(message.Level, LogSeverity.Debug);
            };
            await manager.LogDebugAsync("sample message");
        }

        [Fact(DisplayName = "Verbose")]
        public async void LogVerbose()
        {
            LogManager manager = new LogManager("TEST", LogSeverity.Verbose);
            manager.Log += (src, message) =>
            {
                Assert.Equal(message.Level, LogSeverity.Verbose);
            };
            await manager.LogVerboseAsync("sample message");
        }

        [Fact(DisplayName = "Info")]
        public async void LogInfo()
        {
            LogManager manager = new LogManager("TEST", LogSeverity.Info);
            manager.Log += (src, message) =>
            {
                Assert.Equal(message.Level, LogSeverity.Info);
            };
            await manager.LogInfoAsync("sample message");
        }

        [Fact(DisplayName = "Warning")]
        public async void LogWarning()
        {
            LogManager manager = new LogManager("TEST", LogSeverity.Warning);
            manager.Log += (src, message) =>
            {
                Assert.Equal(message.Level, LogSeverity.Warning);
            };
            await manager.LogWarningAsync("sample message");
        }

        [Fact(DisplayName = "Error")]
        public async void LogError()
        {
            LogManager manager = new LogManager("TEST", LogSeverity.Error);
            manager.Log += (src, message) =>
            {
                Assert.Equal(message.Level, LogSeverity.Error);
            };
            await manager.LogErrorAsync("sample message");
        }

        [Fact(DisplayName = "Critical")]
        public async void LogCritical()
        {
            LogManager manager = new LogManager("TEST", LogSeverity.Critical);
            manager.Log += (src, message) =>
            {
                Assert.Equal(message.Level, LogSeverity.Critical);
            };
            await manager.LogCriticalAsync("sample message");
        }

        [Fact(DisplayName = "Does not log lower than severity")]
        public async void DoesNotLogLowerThanSeverity()
        {
            LogManager manager = new LogManager("TEST", LogSeverity.Info);
            manager.Log += (src, message) =>
            {
                Assert.Equal(message.Level, LogSeverity.Info);
            };
            await manager.LogDebugAsync("sample message");
        }

        [Fact(DisplayName = "Linked log managers")]
        public async void LinkedLogMovesUp()
        {
            LogManager rootManager = new LogManager("TEST", LogSeverity.Info);
            LogManager linkedManager = rootManager.CreateLinkedManager("LINKED");
            rootManager.Log += (src, message) =>
            {
                Assert.Equal(message.Source, "LINKED");
            };
            await linkedManager.LogInfoAsync("sample message");
        }
    }
}
