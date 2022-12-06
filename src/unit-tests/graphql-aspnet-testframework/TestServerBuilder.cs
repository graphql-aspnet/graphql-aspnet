// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework
{
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A builder class to configure a scenario and generate a test server to execute unit tests against for the
    /// default schema.
    /// </summary>
    public class TestServerBuilder : TestServerBuilder<GraphSchema>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestServerBuilder"/> class.
        /// </summary>
        /// <param name="initialSetup">A set of flags for common preconfigured settings for the test server.</param>
        public TestServerBuilder(TestOptions initialSetup = TestOptions.None)
            : base(initialSetup)
        {
        }
    }
}