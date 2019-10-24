// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    /// Extensions and helpful methods surrounding ASP.NET framework items.
    /// </summary>
    public static class AspNetExtensions
    {
        /// <summary>
        /// Retrieves the username of the first attached identity that has a non-null and not-empty username.
        /// Returns null if no suitable username was found.
        /// </summary>
        /// <param name="principal">The principal to inspect.</param>
        /// <param name="claimType">When provided, this method will search for the
        /// claim type and if found, return the value instead of a username.</param>
        /// <returns>System.String.</returns>
        public static string RetrieveUsername(this ClaimsPrincipal principal, string claimType = null)
        {
            IEnumerable<ClaimsIdentity> identities = null;
            if (principal == null)
            {
                identities = Enumerable.Empty<ClaimsIdentity>();
            }
            else
            {
                identities = principal.Identities;
                if (identities == null)
                {
                    if (principal.Identity is ClaimsIdentity ci)
                        identities = ci.AsEnumerable();
                    else
                        identities = Enumerable.Empty<ClaimsIdentity>();
                }
            }

            foreach (var identity in identities)
            {
                if (claimType != null)
                {
                    var claim = identity.FindFirst(claimType);
                    if (claim != null)
                        return claim.Value;
                }

                if (!string.IsNullOrEmpty(identity.Name))
                    return identity.Name;
            }

            return null;
        }
    }
}