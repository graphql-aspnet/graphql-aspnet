// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.FieldColletionNodeSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// For <see cref="OperationTypeNode"/> and <see cref="FieldNode"/> that have a set of requested child fields, generates
    /// a new <see cref="FieldSelectionSet"/> and sets it as the active <see cref="FieldSelectionSet"/> on the current context.
    /// </summary>
    internal class FieldCollection_GenerateFieldSelectionSet : DocumentConstructionStep<FieldCollectionNode>
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            // only generate a new selection set if we are in context of
            // an operation or a field with child fields
            if (!base.ShouldExecute(context))
                return false;

            // only executing a new collection as a child of an existing field or operation renders a new selection set in the documents
            // other areas just append to the current scope of fields already in context.
            return context.ActiveNode.ParentNode is FieldNode;
        }

        /// <summary>
        /// Determines where this context is in a state such that it should continue processing its children. Returning
        /// false will cease processing child nodes under the active node of this context. This can be useful
        /// if/when a situation in a parent disqualifies all other nodes in the tree. This step is always executed
        /// even if the primary execution is skipped.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if child node rulesets should be executed, <c>false</c> otherwise.</returns>
        public override bool ShouldAllowChildContextsToExecute(DocumentConstructionContext context)
        {
            // process the children of this collection when an active selection set
            // is ready to handle them
            return context.SelectionSet != null;
        }

        /// <summary>
        /// Executes the construction step the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            context.BeginNewFieldSelectionSet();
            return true;
        }
    }
}