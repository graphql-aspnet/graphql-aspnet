// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using GraphQL.AspNet.Security;

    /// <summary>
    /// Configuration settings related to how a schema handles authorization at runtime.
    /// </summary>
    public class SchemaAuthorizationConfiguration
    {
        /// <summary>
        /// Gets or sets the authorization mode for this schema. This mode effects when GraphQL will invoke
        /// the authorization rules and how they will be applied to a request. (Default: PerField).
        /// </summary>
        /// <value>The mode.</value>
        public AuthorizationMethod Method { get; set; } = AuthorizationMethod.PerField;
    }
}