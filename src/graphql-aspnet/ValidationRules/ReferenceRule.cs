// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules
{
    using System.Text;

    /// <summary>
    /// A helper method to build a url that points to a rule in
    /// a specific graphql specification document.
    /// </summary>
    internal static class ReferenceRule
    {
        /// <summary>
        /// Creates a url to reference rule to the graphql specification document
        /// this library supports at the indicated place on the web page.
        /// </summary>
        /// <param name="anchorTag">The anchor tag to append to the
        /// specification document link. If null the root of the document
        /// page is returned.</param>
        /// <returns>System.String.</returns>
        public static string Create(string anchorTag)
        {
            var builder = new StringBuilder();
            builder.Append(Constants.SPECIFICATION_URL);

            if (!string.IsNullOrWhiteSpace(anchorTag))
            {
                if (!anchorTag.StartsWith("#"))
                    builder.Append("#");

                builder.Append(anchorTag);
            }

            return builder.ToString();
        }
    }
}