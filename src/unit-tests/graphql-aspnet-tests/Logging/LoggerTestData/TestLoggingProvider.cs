// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Logging.LoggerTestData
{
    using Microsoft.Extensions.Logging;

    public class TestLoggingProvider : ILoggerProvider
    {
        private readonly TestLogger _loggerInstance;

        public TestLoggingProvider(TestLogger loggerInstance)
        {
            _loggerInstance = loggerInstance;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggerInstance;
        }

        public void Dispose()
        {
        }
    }
}