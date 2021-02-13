// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryOperationSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// <para>(5.2.2.1) Validate that when an anon operation is included it exists by itself.</para>
    /// <para>Reference: https://graphql.github.io/graphql-spec/June2018/#sec-Operation-Name-Uniqueness" .</para>
    /// </summary>
    internal class Rule_5_2_2_1_LoneAnonymousOperation : DocumentPartValidationRuleStep<QueryOperation>
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context) &&
                   context.ActivePart is QueryOperation operation && operation.Name == string.Empty;
        }

        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode"/> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentValidationContext context)
        {
            // anonymous operations will all present as ReadOnlyMemory<char>.Empty
            var operation = (QueryOperation)context.ActivePart;

            if (context.DocumentContext.Operations.Count > 1)
            {
                this.ValidationError(
                    context,
                    operation.Node,
                    "A query document may declare an anonymous operation only if it exists by itself in a document. This document " +
                    $"contains {context.DocumentContext.Operations.Count} total operation(s). Remove the other operations are " +
                    "provide a name for every operation.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z").
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.2.2.1";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Lone-Anonymous-Operation";
    }
}