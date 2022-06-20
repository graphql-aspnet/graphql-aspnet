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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution.Common;

    /// <summary>
    /// Inspects the current context for a valid result and appropriately allows
    /// the resultant data item of the context to continue to exist or nulls it out.
    /// </summary>
    internal class GraphDataItem_FinalizeDataItem : FieldResolutionStep
    {
        /// <summary>
        /// Validates the completed field context to ensure it is "correct" against the specification before finalizing its reslts.
        /// </summary>
        /// <param name="context">The context containing the resolved field.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(FieldValidationContext context)
        {
            // attempt to complete the item on the context
            context.DataItem.Complete();
            return true;
        }
    }
}