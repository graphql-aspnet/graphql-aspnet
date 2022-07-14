// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Security.Web
{
    using System.Security.Claims;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// An authentication result served by the <see cref="EmptyUserSecurityContext"/>.
    /// </summary>
    public class EmptyUserAuthenticationResult : IAuthenticationResult
    {
        /// <inheritdoc />
        public bool Suceeded => false;

        /// <inheritdoc />
        public string AuthenticationScheme => string.Empty;

        /// <inheritdoc />
        public ClaimsPrincipal User => null;
    }
}