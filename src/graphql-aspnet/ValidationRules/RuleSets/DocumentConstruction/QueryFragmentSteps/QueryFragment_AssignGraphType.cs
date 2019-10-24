// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.QueryFragmentSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Inspects the named fragment for a type restriction and adds it to the active query fragment. If the
    /// named fragment does not have a type restriction fail the fragment but do so silently. Rule 5.5.1.2 will
    /// handle error messages on mismatched graph types.
    /// </summary>
    internal class QueryFragment_AssignGraphType : DocumentConstructionStep<QueryFragment>
    {
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var queryFragment = context.FindContextItem<QueryFragment>();

            // ensure that the named fragment's target type (if there is one) exists in the schema nad assign it to the query
            // fragment object if it doesnt exist mark this fragment as failed
            var graphtype = context.DocumentContext.Schema.KnownTypes.FindGraphType(queryFragment.TargetGraphTypeName);
            if (graphtype == null)
                return false;

            queryFragment.GraphType = graphtype;
            context.DocumentScope?.RestrictFieldsToGraphType(queryFragment.GraphType);
            return true;
        }
    }
}