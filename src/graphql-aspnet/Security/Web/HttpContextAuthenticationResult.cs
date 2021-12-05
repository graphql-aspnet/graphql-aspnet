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
        /// <param name="authWasSuccessful">if set to <c>true</c>
        /// the authentication operation that produced this result is said to have been succeful.</param>
        /// <param name="scheme">The authentication scheme encapsulated by this result.</param>
        /// <param name="httpAuthTicket">The HTTP authentication ticket.</param>
        public HttpContextAuthenticationResult(bool authWasSuccessful, string scheme, AuthenticationTicket httpAuthTicket = null)
        {
            if (authWasSuccessful && httpAuthTicket == null)
            {
                this.Status = AuthenticationStatus.Skipped;
                this.User = null;
                this.AuthenticationScheme = scheme;
            }
            else
            {
                this.User = httpAuthTicket?.Principal;
                this.AuthenticationScheme = httpAuthTicket?.AuthenticationScheme;

                if (authWasSuccessful && this.User != null)
                    this.Status = AuthenticationStatus.Success;
                else
                    this.Status = AuthenticationStatus.Failed;
            }
        }

        /// <inheritdoc />
        public AuthenticationStatus Status { get; }

        /// <inheritdoc />
        public string AuthenticationScheme { get; }

        /// <inheritdoc />
        public ClaimsPrincipal User { get; }
    }
}