// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common.CommonHelpers.JsonComparing
{
    /// <summary>
    /// A result containing the details of a json string comparrison.
    /// </summary>
    public class JsonComparrisonResult
    {
        /// <summary>
        /// Generates a new comparrison result indicating that the elements were not
        /// equal. Optionally attaches the provided error message.
        /// </summary>
        /// <param name="errorMessage">A message indicating what about the comparrison
        /// was determined to not be in equality.</param>
        /// <returns>JsonComparrisonResult.</returns>
        public static JsonComparrisonResult Failed(string errorMessage = null)
        {
            return new JsonComparrisonResult(false, errorMessage);
        }

        /// <summary>
        /// Gets a singleton instance of a comparrison result that indicates a valid state.
        /// </summary>
        /// <value>The valid.</value>
        public static JsonComparrisonResult ElementsEqual { get; }

        /// <summary>
        /// Initializes static members of the <see cref="JsonComparrisonResult"/> class.
        /// </summary>
        static JsonComparrisonResult()
        {
            ElementsEqual = new JsonComparrisonResult(true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonComparrisonResult" /> class.
        /// </summary>
        /// <param name="elementsAreEqual">if set to <c>true</c> the comparrison test passed successfully.</param>
        /// <param name="location">The location in the expected document that was
        /// being tested when equality failed.</param>
        /// <param name="errorMessage">An optional error message that may be
        /// returned when a failure occurs.</param>
        public JsonComparrisonResult(bool elementsAreEqual, string location = null, string errorMessage = null)
        {
            this.ElementsAreEqual = elementsAreEqual;
            this.ErrorMessage = errorMessage;
            this.Location = location;
        }

        /// <summary>
        /// Gets a value indicating whether the compared json elements are considered equal
        /// or not.
        /// </summary>
        /// <value><c>true</c> if equality was determined to be true; otherwise, <c>false</c>.</value>
        public bool ElementsAreEqual { get; }

        /// <summary>
        /// Gets an optiona error message. This message, if available, will contain details
        /// about what failed.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; }

        /// <summary>
        /// Gets the location in the expected document that was being evalulated
        /// against the actual document when equality failed.
        /// </summary>
        /// <value>The last tested location in the expected document.</value>
        public string Location { get; }
    }
}