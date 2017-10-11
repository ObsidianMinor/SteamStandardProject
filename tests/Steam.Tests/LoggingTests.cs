using Steam.Logging;
using Xunit;

namespace Steam.Tests
{
    [Trait("Category", "Logging")]
    public class LoggingTests
    {
        [Fact(DisplayName = "Debug")]
        public void LogDebug()
        {
            LogManager manager = new LogManager(LogSeverity.Debug);
            manager.Log += (src, message) =>
            {
                Assert.Equal(LogSeverity.Debug, message.Level);
            };
            manager.LogDebug("TEST", "sample message");
        }

        [Fact(DisplayName = "Verbose")]
        public void LogVerbose()
        {
            LogManager manager = new LogManager(LogSeverity.Verbose);
            manager.Log += (src, message) =>
            {
                Assert.Equal(LogSeverity.Verbose, message.Level);
            };
            manager.LogVerbose("TEST", "sample message");
        }

        [Fact(DisplayName = "Info")]
        public void LogInfo()
        {
            LogManager manager = new LogManager(LogSeverity.Info);
            manager.Log += (src, message) =>
            {
                Assert.Equal(LogSeverity.Info, message.Level);
            };
            manager.LogInfo("TEST", "sample message");
        }

        [Fact(DisplayName = "Warning")]
        public void LogWarning()
        {
            LogManager manager = new LogManager(LogSeverity.Warning);
            manager.Log += (src, message) =>
            {
                Assert.Equal(LogSeverity.Warning, message.Level);
            };
            manager.LogWarning("TEST", "sample message");
        }

        [Fact(DisplayName = "Error")]
        public void LogError()
        {
            LogManager manager = new LogManager( LogSeverity.Error);
            manager.Log += (src, message) =>
            {
                Assert.Equal(LogSeverity.Error, message.Level);
            };
            manager.LogError("TEST", "sample message");
        }

        [Fact(DisplayName = "Critical")]
        public void LogCritical()
        {
            LogManager manager = new LogManager(LogSeverity.Critical);
            manager.Log += (src, message) =>
            {
                Assert.Equal(LogSeverity.Critical, message.Level);
            };
            manager.LogCritical("TEST", "sample message");
        }

        [Fact(DisplayName = "Does not log lower than severity")]
        public void DoesNotLogLowerThanSeverity()
        {
            LogManager manager = new LogManager(LogSeverity.Info);
            manager.Log += (src, message) =>
            {
                Assert.Equal(LogSeverity.Info, message.Level);
            };
            manager.LogDebug("TEST", "sample message");
        }
    }
}
