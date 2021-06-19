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
    using System.Collections.Generic;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Ensures that for those fragments that do declare a target graph type that that graph type is
    /// of the kinds allowed by the specificiation (INTERFACE, UNION, OBJECT).
    /// </summary>
    internal class Rule_5_5_1_3_FragmentTargetTypeMustBeOfAllowedKind : DocumentConstructionRuleStep<QueryFragment>
    {
        private static readonly HashSet<TypeKind> ALLOWED_TYPE_KINDS;
        private static readonly string ALLOWED_TYPE_KIND_STRING;

        static Rule_5_5_1_3_FragmentTargetTypeMustBeOfAllowedKind()
        {
            ALLOWED_TYPE_KINDS = new HashSet<TypeKind>(new[] { TypeKind.OBJECT, TypeKind.UNION, TypeKind.INTERFACE });
            ALLOWED_TYPE_KIND_STRING = $"{TypeKind.OBJECT.ToString()}, {TypeKind.UNION.ToString()} or {TypeKind.INTERFACE.ToString()}";
        }

        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var fragmnet = context.FindContextItem<QueryFragment>();

            if (string.IsNullOrEmpty(fragmnet.TargetGraphTypeName))
                return true;

            // shouldn't be false at this step, but just in case fail out
            if (fragmnet.GraphType == null)
                return false;

            if (!ALLOWED_TYPE_KINDS.Contains(fragmnet.GraphType.Kind))
            {
                this.ValidationError(
                    context,
                    $"The fragment declares a target graph type of '{fragmnet.GraphType.Name}' " +
                    $"of kind '{fragmnet.GraphType.Kind.ToString()}' but " +
                    $"fragments can only target graph types of kind {ALLOWED_TYPE_KIND_STRING}.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.5.1.3";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Fragments-On-Composite-Types";
    }
}