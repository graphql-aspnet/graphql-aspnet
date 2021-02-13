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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution.Common;

    /// <summary>
    /// An extension to rule 6.4.3 for resolution completion to determine if the data retrieved
    /// can be used by the server side resolvers used by this schema.
    /// </summary>
    internal class Rule_6_4_3_ServerValueCompletion : FieldResolutionRuleStep
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the TContext
        /// if it cannot process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(FieldValidationContext context)
        {
            // this rule can only validate that a non-null result
            // is valid and "could" be used down stream
            // lists arent relevenat
            return !context.DataItem.IsListField && context.ResultData != null;
        }

        /// <summary>
        /// Validates the completed field context to ensure it is "correct" against the specification before finalizing its reslts.
        /// </summary>
        /// <param name="context">The context containing the resolved field.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(FieldValidationContext context)
        {
            var dataObject = context.ResultData;

            // did they forget to await a task and accidentally returned Task<T> from their resolver?
            if (dataObject is Task)
            {
                this.ValidationError(
                    context,
                    $"A field resolver for '{context.FieldPath}' yielded an invalid data object. See exception " +
                    "for details. ",
                    new GraphExecutionException(
                        $"The field '{context.FieldPath}' yielded a {nameof(Task)} as its result but expected a value. " +
                        "Did you forget to await an async method?"));

                context.DataItem.InvalidateResult();
            }
            else
            {
                // is the concrete type of the data item not known to the schema?
                var strippedSourceType = GraphValidation.EliminateWrappersFromCoreType(dataObject.GetType());
                var graphType = context.Schema.KnownTypes.FindGraphType(strippedSourceType);
                if (graphType == null)
                {
                    this.ValidationError(
                        context,
                        $"A field resolver for '{context.Field.Route.Path}' generated a result " +
                        "object type not known to the target schema. See exception for " +
                        "details",
                        new GraphExecutionException(
                            $"The class '{strippedSourceType.FriendlyName()}' is not not mapped to a graph type " +
                            "on the target schema."));

                    context.DataItem.InvalidateResult();
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "6.4.3";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Value-Completion";
    }
}