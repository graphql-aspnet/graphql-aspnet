// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common
{
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.Interfaces;

    /// <summary>
    /// A base step with commmon logic for all document validation steps.
    /// </summary>
    internal abstract class DocumentPartValidationStep : IRuleStep<DocumentValidationContext>
    {
        /// <summary>
        /// Registers a validation error with the local message collection as a critical error. The validation
        /// message will automatically be appended with the appropriate message extensions to reference the error being validated.
        /// </summary>
        /// <param name="context">The validation context in scope.</param>
        /// <param name="message">The error message.</param>
        protected virtual void ValidationError(DocumentValidationContext context, string message)
        {
            this.ValidationError(context, context.ActivePart.SourceLocation, message);
        }

        /// <summary>
        /// Registers a validation error with the local message collection as a critical error. The validation
        /// message will automatically be appended with the appropriate message extensions to reference the error being validated.
        /// </summary>
        /// <param name="context">The validation context in scope.</param>
        /// <param name="node">A custom node to use to indicate the area in the source document
        /// the error occured.</param>
        /// <param name="message">The error message.</param>
        protected virtual void ValidationError(DocumentValidationContext context, SourceLocation sourceLocation, string message)
        {
            context.Messages.Critical(
                message,
                this.ErrorCode,
                sourceLocation.AsOrigin());
        }

        /// <inheritdoc />
        public virtual bool ShouldAllowChildContextsToExecute(DocumentValidationContext context)
        {
            return true;
        }

        /// <inheritdoc />
        public abstract bool ShouldExecute(DocumentValidationContext context);

        /// <inheritdoc />
        public abstract bool Execute(DocumentValidationContext context);

        /// <summary>
        /// Gets the error code to associate with any validation messages recorded.
        /// </summary>
        /// <value>The error code.</value>
        public virtual string ErrorCode => Constants.ErrorCodes.INVALID_DOCUMENT;
    }
}