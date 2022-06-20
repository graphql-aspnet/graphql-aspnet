// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution.FieldCompletion
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution.Common;

    /// <summary>
    /// Updates the status of the data item on the context based on its current state.
    /// </summary>
    internal class GraphDataItem_ResolveFieldStatus : FieldResolutionStep
    {
        /// <summary>
        /// Validates the completed field context to ensure it is "correct" against the specification before finalizing its reslts.
        /// </summary>
        /// <param name="context">The context containing the resolved field.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(FieldValidationContext context)
        {
            if (!context.DataItem.Status.IsFinalized())
            {
                // any status other ResultAssigned or NotStarted indicates a non-processing such as complete or failed etc.
                // and should be left in tact and unedited.
                if (context.DataItem.Status == FieldItemResolutionStatus.ResultAssigned)
                {
                    // if a data value was set ensure any potnetial children are processed
                    if (context.DataItem.ResultData == null || context.DataItem.FieldContext.Field.IsLeaf)
                        context.DataItem.Complete();
                    else
                        context.DataItem.RequireChildResolution();
                }
                else
                {
                    // this rule can only run against field resolutions being validated
                    // as complete. Something that hasnt been started yet has missed its chance
                    context.DataItem.Fail();
                }
            }

            return true;
        }
    }
}