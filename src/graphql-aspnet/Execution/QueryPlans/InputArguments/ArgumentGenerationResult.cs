// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.InputArguments
{
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// The result of attempting to resolve an input argument in a query document.
    /// </summary>
    internal class ArgumentGenerationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentGenerationResult" /> class.
        /// </summary>
        /// <param name="argument">The argument that was successfully resolved.</param>
        public ArgumentGenerationResult(InputArgument argument)
        {
            this.Argument = argument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentGenerationResult"/> class.
        /// </summary>
        /// <param name="message">A generated message, usually an error, in lue of
        /// an argument being resolved.</param>
        public ArgumentGenerationResult(IGraphMessage message)
        {
            this.Message = message;
        }

        /// <summary>
        /// Gets the argument that was generated.
        /// </summary>
        /// <value>The argument that was resolved.</value>
        public InputArgument Argument { get; }

        /// <summary>
        /// Gets a message that was generated due to a failure in creating the argument.
        /// </summary>
        /// <value>The message that was generated, if any.</value>
        public IGraphMessage Message { get; }

        /// <summary>
        /// Gets a value indicating whether the result contains a valid argument.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid => this.Message == null && this.Argument != null;
    }
}