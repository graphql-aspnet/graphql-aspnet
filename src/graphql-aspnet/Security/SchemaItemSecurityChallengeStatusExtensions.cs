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
    /// <summary>
    /// Extension methods for <see cref="SchemaItemSecurityChallengeStatus"/>.
    /// </summary>
    public static class SchemaItemSecurityChallengeStatusExtensions
    {
        /// <summary>
        /// Determines whether the specified status is considered a binary "authorized" vs. "unauthorized" state without
        /// the details of the type of authorization.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns><c>true</c> if the specified status is authorized; otherwise, <c>false</c>.</returns>
        public static bool IsAuthorized(this SchemaItemSecurityChallengeStatus status)
        {
            switch (status)
            {
                case SchemaItemSecurityChallengeStatus.Authorized:
                case SchemaItemSecurityChallengeStatus.Skipped:
                    return true;

                default:
                    return false;
            }
        }
    }
}