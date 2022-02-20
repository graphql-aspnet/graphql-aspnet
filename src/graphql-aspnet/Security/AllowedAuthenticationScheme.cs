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
    using System.Diagnostics;
    using GraphQL.AspNet.Common;

    /// <summary>
    /// An instance of an allowed authentication scheme.
    /// </summary>
    [DebuggerDisplay("{AuthScheme}")]
    public class AllowedAuthenticationScheme
    {
        /// <summary>
        /// The default equality comparer for this object.
        /// </summary>
        public static IEqualityComparer<AllowedAuthenticationScheme> DefaultComparer = new AllowedAuthenticationSchemeComparer();

        /// <summary>
        /// Initializes a new instance of the <see cref="AllowedAuthenticationScheme" /> class.
        /// </summary>
        /// <param name="name">The name of the allowed auth scheme.</param>
        public AllowedAuthenticationScheme(string name)
        {
            this.AuthScheme = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
        }

        /// <summary>
        /// Gets the authentication scheme allowed via this instnace.
        /// </summary>
        /// <value>The authentication scheme.</value>
        public string AuthScheme { get; }

        /// <summary>
        /// A comparer for <see cref="AllowedAuthenticationScheme"/>.
        /// </summary>
        private class AllowedAuthenticationSchemeComparer : IEqualityComparer<AllowedAuthenticationScheme>
        {
            /// <inheritdoc />
            public bool Equals(AllowedAuthenticationScheme x, AllowedAuthenticationScheme y)
            {
                return x.AuthScheme == y.AuthScheme;
            }

            /// <inheritdoc />
            public int GetHashCode(AllowedAuthenticationScheme obj)
            {
                return obj.AuthScheme.GetHashCode();
            }
        }
    }
}