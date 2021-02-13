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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// The '__typename' field has a few special case needs around it concerning union graph types. process the __typename
    /// field, when requested, by itself in this rule.
    /// </summary>
    internal class TypeNameMetaField_SpecialCase : DocumentConstructionRuleStep<FieldNode>
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
                       .SequenceEqual(Constants.ReservedNames.TYPENAME_FIELD.AsSpan());
        }

        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (FieldNode)context.ActiveNode;

            // the '__typename' field is a known static quantity that requires no special rule processing or validation
            // it just exists or it doesn't and would be valid on any graph type returning it.
            // Group the appropriate logic for this metafield here to account for the allowed exception in spec rule 5.3.1
            // where unions can contain a field reference for '__typename'
            var allTypes = context.DocumentContext.Schema.KnownTypes.ExpandAbstractType(context.GraphType);
            foreach (var graphType in allTypes)
            {
                if (graphType == null)
                    continue;

                IGraphField field = null;
                if (graphType is IGraphFieldContainer fieldContainer)
                    field = fieldContainer.Fields.FindField(node.FieldName.ToString());

                if (field == null)
                {
                    // if the graph type doesnt contain fields (scalar or enum?) then its an error.
                    // this shouldnt happen because of schema building and validation but just in case...
                    this.ValidationError(
                        context,
                        $"The graph type '{graphType.Name}' does not contain a field named '{node.FieldName.ToString()}'.");

                    continue;
                }

                var fieldSelection = new FieldSelection(node, field, graphType);
                context.AddDocumentPart(fieldSelection);
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