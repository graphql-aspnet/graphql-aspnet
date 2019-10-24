// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.FragmentSpreadNodeSteps
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A base class providing common functionality for the 5.5.2.3 rules.
    /// </summary>
    internal abstract class RuleBase_5_5_2_3_FragmentCanSpreadInContext
        : DocumentConstructionRuleStep<FragmentSpreadNode>
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            if (!base.ShouldExecute(context))
                return false;

            if (context.GraphType == null)
                return false;

            if (!this.AllowedContextGraphTypeKinds.Contains(context.GraphType.Kind))
                return false;

            var targetGraphType = this.ExtractTargetGraphType(context);
            if (targetGraphType == null)
                return false;

            if (!this.AllowedTargetGraphTypeKinds.Contains(targetGraphType.Kind))
                return false;

            return true;
        }

        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode"/> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            // both objects should exist if the rule chain is followed
            // but do a null check just in case
            var fragmentPointer = (FragmentSpreadNode)context.ActiveNode;
            var targetGraphType = this.ExtractTargetGraphType(context);
            var contextGraphType = context.GraphType;

            if (targetGraphType == null || contextGraphType == null)
                return false;

            var rulePassed = this.CanAcceptGraphType(context.DocumentContext.Schema, contextGraphType, targetGraphType);
            if (!rulePassed)
            {
                this.ValidationError(
                    context,
                    $"The named fragment '{fragmentPointer.PointsToFragmentName.ToString()}' has a target graph type " +
                    $"named '{targetGraphType.Name}' (Kind: '{targetGraphType.Kind.ToString()}') which cannot be coerced " +
                    $"into the current selection set's target graph type of '{contextGraphType.Name}'.");
            }

            return rulePassed;
        }

        /// <summary>
        /// Attempts to find the target graph type of the active spread node on the context.
        /// </summary>
        /// <param name="context">The context to extract from.</param>
        /// <returns>The found graph type or null if the fragment is not found or
        /// has no defined graph type.</returns>
        private IGraphType ExtractTargetGraphType(DocumentConstructionContext context)
        {
            var node = context.ActiveNode as FragmentSpreadNode;
            if (node == null)
                return null;

            var namedFragment = context.DocumentContext.Fragments.FindFragment(node.PointsToFragmentName.ToString());
            return namedFragment?.GraphType;
        }

        /// <summary>
        /// Determines if the target graph type COULD BE spread into the active context graph type.
        /// </summary>
        /// <param name="schema">The target schema in case any additional graph types need to be accessed.</param>
        /// <param name="typeInContext">The graph type currently active on the context.</param>
        /// <param name="targetGraphType">The target graph type of the spread named fragment.</param>
        /// <returns><c>true</c> if the target type can be spread in context; otherwise, false.</returns>
        protected abstract bool CanAcceptGraphType(ISchema schema, IGraphType typeInContext, IGraphType targetGraphType);

        /// <summary>
        /// Gets the set of type kinds for the pointed at named fragment
        /// that this rule can validate for.
        /// </summary>
        /// <value>A list of type kinds.</value>
        protected abstract HashSet<TypeKind> AllowedTargetGraphTypeKinds { get; }

        /// <summary>
        /// Gets the set of type kinds for the "in context" graph type
        /// that this rule can validate for.
        /// </summary>
        /// <value>A list of type kinds.</value>
        protected abstract HashSet<TypeKind> AllowedContextGraphTypeKinds { get; }
    }
}