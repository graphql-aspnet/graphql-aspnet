// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Response
{
    using System.Text.Json;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration.Formatting;

    /// <summary>
    /// A naming policy to convert enums to the value required by a schema for serialization.
    /// </summary>
    public class GraphQLJsonEnumNamingPolicy : JsonNamingPolicy
    {
        private readonly GraphNameFormatter _namingFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLJsonEnumNamingPolicy"/> class.
        /// </summary>
        /// <param name="namingFormatter">The naming formatter.</param>
        public GraphQLJsonEnumNamingPolicy(GraphNameFormatter namingFormatter)
        {
            _namingFormatter = Validation.ThrowIfNullOrReturn(namingFormatter, nameof(namingFormatter));
        }

        /// <summary>
        /// When overridden in a derived class, converts the specified name according to the policy.
        /// </summary>
        /// <param name="name">The name to convert.</param>
        /// <returns>The converted name.</returns>
        public override string ConvertName(string name)
        {
            return _namingFormatter.FormatEnumValueName(name);
        }
    }
}