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
    using System.Linq;
    using System.Security.Claims;
    using GraphQL.AspNet.Interfaces.Security;
    using Microsoft.AspNetCore.Authentication;

    /// <summary>
    /// An implementation of <see cref="IAuthenticationResult"/> that encapsulates
    /// a HTTP <see cref="AuthenticationTicket"/>.
    /// </summary>
    public class HttpContextAuthenticationResult : IAuthenticationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextAuthenticationResult" /> class.
        /// </summary>
        /// <param name="scheme">The authentication scheme encapsulated by this result.</param>
        /// <param name="httpAuthResult">The HTTP authentication result.</param>
        public HttpContextAuthenticationResult(string scheme, AuthenticateResult httpAuthResult)
        {
            this.Suceeded = httpAuthResult?.Succeeded ?? false;
            this.User = httpAuthResult?.Ticket?.Principal;
            this.AuthenticationScheme = scheme;
        }

        /// <inheritdoc />
        public string AuthenticationScheme { get; }

        /// <inheritdoc />
        public ClaimsPrincipal User { get; }

        /// <inheritdoc />
        public bool Suceeded { get; }
    }
}