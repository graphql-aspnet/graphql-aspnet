// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.FieldNodeSteps
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Ensures that for the active <see cref="FieldNode"/> that the currently scoped <see cref="IGraphType"/>
    /// the field exists on said <see cref="IGraphType"/>.
    /// </summary>
    internal class Rule_5_3_1_FieldMustExistOnTargetGraphType : DocumentConstructionRuleStep<FieldNode>
    {
        /// <summary>
        /// Creates a common message to indicate and invalid or missing field.
        /// </summary>
        /// <param name="graphType">The graph type expected to contain the field.</param>
        /// <param name="fieldName">Name of the non-existent field.</param>
        /// <returns>System.String.</returns>
        public static string InvalidFieldMessage(string graphType, string fieldName)
        {
            return $"The graph type '{graphType}' does not contain a field named '{fieldName}'.";
        }

        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return base.ShouldExecute(context) && ((FieldNode)context.ActiveNode)
                       .FieldName
                       .Span
                       .SequenceNotEqual(Constants.ReservedNames.TYPENAME_FIELD.AsSpan());
        }

        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (FieldNode)context.ActiveNode;
            var searchContainer = context.GraphType as IGraphFieldContainer;

            if (searchContainer == null)
                return false;

            // if a fragment spread is in context that is performing a restriction on this field
            // use that graph type restriction to check for a valid field
            // otherwise revert back to the master selection set where the field is contained
            if (!searchContainer.Fields.ContainsKey(node.FieldName.ToString()))
            {
                this.ValidationError(
                    context,
                    InvalidFieldMessage(searchContainer.Name, node.FieldName.ToString()));

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.3.1";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Field-Selections-on-Objects-Interfaces-and-Unions-Types";
    }
}