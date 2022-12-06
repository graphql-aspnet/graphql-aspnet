// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Security
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// An object indicating what authentication methods are allowed
    /// for a particular entity.
    /// </summary>
    internal class AuthenticationDigest
    {
        /// <summary>
        /// Gets a empty digets that denies anonymous access and allows no
        /// auth schemes.
        /// </summary>
        /// <value>The singleton empty digest.</value>
        public static AuthenticationDigest Empty { get; private set; }

        /// <summary>
        /// Initializes static members of the <see cref="AuthenticationDigest"/> class.
        /// </summary>
        static AuthenticationDigest()
        {
            Empty = new AuthenticationDigest();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationDigest"/> class.
        /// </summary>
        /// <param name="allowAnonymous">if set to <c>true</c> the entity will allow
        /// anonymous users access.</param>
        /// <param name="allowedSchemes">The monikers of the authentication scheme
        /// that are allowed to access the entity.</param>
        public AuthenticationDigest(bool allowAnonymous = false, IEnumerable<string> allowedSchemes = null)
        {
            this.AllowAnonymous = allowAnonymous;

            allowedSchemes = allowedSchemes ?? Enumerable.Empty<string>();
            this.AllowedSchemes = allowedSchemes.ToImmutableHashSet();
        }

        /// <summary>
        /// Gets a value indicating whether the entity allows anonymous users
        /// access.
        /// </summary>
        /// <value><c>true</c> if anonymous users are allowed access; otherwise, <c>false</c>.</value>
        public bool AllowAnonymous { get; }

        /// <summary>
        /// Gets the monikers of hte authentication schemes that are allowed to
        /// access the entity.
        /// </summary>
        /// <value>The allowed schemes.</value>
        public ImmutableHashSet<string> AllowedSchemes { get; }
    }
}