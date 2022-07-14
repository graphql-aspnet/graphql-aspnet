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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Security;

    /// <summary>
    /// A security context representing no user.
    /// </summary>
    public class EmptyUserSecurityContext : IUserSecurityContext
    {
        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        /// <value>The singleton instance.</value>
        public static IUserSecurityContext Instance { get; } = new EmptyUserSecurityContext();

        private IAuthenticationResult _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyUserSecurityContext"/> class.
        /// </summary>
        private EmptyUserSecurityContext()
        {
            _result = new EmptyUserAuthenticationResult();
            this.DefaultUser = null;
        }

        /// <inheritdoc />
        public Task<IAuthenticationResult> Authenticate(string scheme, CancellationToken token = default)
        {
            return Task.FromResult(_result);
        }

        /// <inheritdoc />
        public Task<IAuthenticationResult> Authenticate(CancellationToken token = default)
        {
            return Task.FromResult(_result);
        }

        /// <inheritdoc />
        public ClaimsPrincipal DefaultUser { get; }
    }
}