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
    /// Extension methods for <see cref="FieldSecurityChallengeStatus"/>.
    /// </summary>
    public static class FieldSecurityChallengeStatusExtensions
    {
        /// <summary>
        /// Determines whether the specified status is considered a binary "authorized" vs. "unauthorized" state without
        /// the details of the type of authorization.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns><c>true</c> if the specified status is authorized; otherwise, <c>false</c>.</returns>
        public static bool IsAuthorized(this FieldSecurityChallengeStatus status)
        {
            switch (status)
            {
                case FieldSecurityChallengeStatus.Authorized:
                case FieldSecurityChallengeStatus.Skipped:
                    return true;

                default:
                    return false;
            }
        }
    }
}