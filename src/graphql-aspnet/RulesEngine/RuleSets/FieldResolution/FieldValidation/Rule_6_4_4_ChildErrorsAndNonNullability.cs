// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution.FieldValidation
{
    using System.Linq;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution.Common;

    /// <summary>
    /// A rule that inspects a completed result against its completed children to ensure it conforms
    /// to the field's type expression requirements for non-nullability and lower level field error bubbling.
    /// </summary>
    internal class Rule_6_4_4_ChildErrorsAndNonNullability : FieldResolutionStep
    {
        /// <summary>
        /// Validates the completed field context to ensure it is "correct" against the specification before finalizing its reslts.
        /// </summary>
        /// <param name="context">The context containing the resolved field.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(FieldValidationContext context)
        {
            // during the execution of children to this context item
            // its possible that one of those fields (or child list items) resolved to null
            // either because the resolver returned null natively or because of an error
            // during processing. If this occurs and that child violates its type
            // expression requirements (handled in 6.4.3) then this item is also in
            // voliation of its type expression.
            //
            // note: each child is aware of its own type expression requirements
            //       meaning that if this item in scope is a list of required items (e.g. [SomeType!])
            //       then each of those child items would indicate their own errors
            //       if they were nulled since they understand that their type expression is SomeType!
            //       we only need to worry about carrying the errors up the chain
            //       to this item.
            var hasChildErrors = context
                .DataItem
                .Children
                .Any(x => x.Status.IndicatesAnError());

            // 6.4.4 does not actaully raise an error message. Its a rule
            // of propegating down stream errors up the responsibility chain
            // as null values are resolved
            if (hasChildErrors)
                context.DataItem.InvalidateResult();

            // a recorded validation error here doesnt necessarily indicate a complete
            // failure as an upstream field may successfully capture a null result
            // to allow some data to be resolved to the client
            return true;
        }
    }
}