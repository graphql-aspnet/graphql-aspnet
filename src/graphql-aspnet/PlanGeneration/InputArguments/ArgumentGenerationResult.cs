// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.InputArguments
{
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration;

    /// <summary>
    /// The result of attempting to resolve an input argument in a query document.
    /// </summary>
    public class ArgumentGenerationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentGenerationResult"/> class.
        /// </summary>
        /// <param name="argument">The argument that was successfully resolved.</param>
        public ArgumentGenerationResult(IInputArgumentValue argument)
        {
            this.Argument = argument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentGenerationResult"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ArgumentGenerationResult(IGraphMessage message)
        {
            this.Message = message;
        }

        /// <summary>
        /// Gets the argument that was generated.
        /// </summary>
        /// <value>The argument.</value>
        public IInputArgumentValue Argument { get; }

        /// <summary>
        /// Gets a message that was generated due to a failure in creating the argument.
        /// </summary>
        /// <value>The message.</value>
        public IGraphMessage Message { get; }

        /// <summary>
        /// Gets a value indicating whether the result contains a valid argument.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid => this.Message == null && this.Argument != null;
    }
}