// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryFragmentSteps
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// A base class providing common functionality for the 5.5.2.3 rules.
    /// </summary>
    internal abstract class Rule_5_5_2_3_BaseFragmentCanSpreadInContext
        : DocumentPartValidationRuleStep
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            if (!(context.ActivePart is IFragmentSpreadDocumentPart) && !(context.ActivePart is IInlineFragmentDocumentPart))
                return false;

            if (context.ActivePart?.GraphType == null || context.ActivePart.Parent?.GraphType == null)
                return false;

            if (!this.AllowedTargetGraphTypeKinds.Contains(context.ActivePart.GraphType.Kind))
                return false;

            if (!this.AllowedFieldSetGraphTypeKinds.Contains(context.ActivePart.Parent.GraphType.Kind))
                return false;

            return true;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var str = this.RuleNumber;

            // both objects should exist if the rule chain is followed
            // but do a null check just in case
            var targetGraphType = context.ActivePart.GraphType;
            IGraphType spreadInGraphType = null;

            // this should always be true
            if (context.ActivePart.Parent is IFieldSelectionSetDocumentPart fs)
                spreadInGraphType = fs.GraphType;

            if (targetGraphType == null || spreadInGraphType == null)
                return false;

            var rulePassed = this.CanAcceptGraphType(context.Schema, spreadInGraphType, targetGraphType);
            if (!rulePassed)
            {
                string desc;
                if (context.ActivePart is IFragmentSpreadDocumentPart fsdp)
                    desc = $"named fragment '{fsdp.FragmentName.ToString()}'";
                else if (context.ActivePart is IInlineFragmentDocumentPart inlineFrag)
                    desc = $"inline fragment";
                else
                    desc = "~unknown item~"; // technically impossible based on ShouldExecute

                this.ValidationError(
                    context,
                    $"The {desc} has a target graph type " +
                    $"named '{targetGraphType.Name}' (Kind: '{targetGraphType.Kind.ToString()}') which cannot be coerced " +
                    $"into the current selection set's target graph type of '{spreadInGraphType?.Name}'.");
            }

            return rulePassed;
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
        /// Gets the set of type kinds for the parent field set graph type
        /// that this rule can validate for.
        /// </summary>
        /// <value>A list of type kinds.</value>
        protected abstract HashSet<TypeKind> AllowedFieldSetGraphTypeKinds { get; }
    }
}