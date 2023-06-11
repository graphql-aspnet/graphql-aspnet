// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.ServerBuilders
{
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Tests.Framework.Interfaces;

    /// <summary>
    /// Builds an authentication chain for a single user and policy setup.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this component is targeting.</typeparam>
    public class TestAuthenticationBuilder<TSchema> : IGraphQLTestFrameworkComponent<TSchema>, ITestAuthenticationBuilder
        where TSchema : class, ISchema
    {
        private string _defaultAuthenticationScheme;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAuthenticationBuilder{TSchema}" /> class.
        /// </summary>
        public TestAuthenticationBuilder()
        {
            _defaultAuthenticationScheme = TestFrameworkConstants.DEFAULT_AUTH_SCHEME;
        }

        /// <inheritdoc />
        public virtual ITestAuthenticationBuilder SetDefaultAuthenticationScheme(string scheme)
        {
            _defaultAuthenticationScheme = scheme;
            return this;
        }

        /// <inheritdoc />
        public string DefaultAuthenticationScheme => _defaultAuthenticationScheme;
    }
}