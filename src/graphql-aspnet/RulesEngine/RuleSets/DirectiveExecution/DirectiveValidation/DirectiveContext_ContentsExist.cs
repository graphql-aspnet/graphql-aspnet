// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.ValidationRules.RuleSets.DirectiveExecution.DirectiveValidation
{
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.ValidationRules.RuleSets.DirectiveExecution.Common;

    /// <summary>
    /// A general rule to ensure the context is structured correctly before
    /// being processed.
    /// </summary>
    internal class DirectiveContext_ContentsExist : DirectiveValidationStep
    {
        /// <inheritdoc />
        public override bool Execute(GraphDirectiveExecutionContext context)
        {
            var contentsAreValid = true;
            var phase = context?.Request?.DirectivePhase ?? DirectiveInvocationPhase.Unknown;
            if (phase == DirectiveInvocationPhase.Unknown)
            {
                context.Messages.Critical(
                    "Invalid directive request. No valid invocation phase " +
                    "was set on the request. " +
                    $"(Current Phase: {phase}).");

                contentsAreValid = false;
            }

            var location = context?.Request?.InvocationContext?.Location ?? DirectiveLocation.NONE;
            if (location == DirectiveLocation.NONE)
            {
                context.Messages.Critical(
                    $"Invalid directive request. No valid {nameof(DirectiveLocation)} " +
                    $"was set on the request's {nameof(IGraphDirectiveRequest.InvocationContext)}. " +
                    $"(Current Location: {location}).");

                contentsAreValid = false;
            }

            return contentsAreValid;
        }
    }
}