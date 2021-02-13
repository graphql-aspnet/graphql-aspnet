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
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>(5.3.3)A rule that dictates for any given graph type returned by a field if that graph type
    /// is a leaf type then the field cannot declare a selection set of fields to return.</para>
    /// <para>Reference: https://graphql.github.io/graphql-spec/June2018/#sec-Leaf-Field-Selections .</para>
    /// </summary>
    internal class Rule_5_3_3_LeafReturnMustNotHaveChildFields : DocumentConstructionRuleStep<FieldSelection>
    {
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
            var field = context.FindContextItem<FieldSelection>();
            if (field.GraphType.Kind.IsLeafKind() && context.ActiveNode.Children.Any<FieldCollectionNode>())
            {
                this.ValidationError(
                    context,
                    $"The graph type '{field.GraphType.Name}' is of kind '{field.GraphType.Kind.ToString()}'. It cannot declare a fieldset " +
                    "to return.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.3.3";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Leaf-Field-Selections";
    }
}