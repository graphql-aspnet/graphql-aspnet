// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryFragmentSteps
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Ensures that for those fragments that do declare a target graph type that that graph type is
    /// of the kinds allowed by the specificiation (INTERFACE, UNION, OBJECT).
    /// </summary>
    internal class Rule_5_5_1_3_FragmentTargetTypeMustBeOfAllowedKind
        : DocumentPartValidationRuleStep<IFragmentDocumentPart>
    {
        private static readonly HashSet<TypeKind> ALLOWED_TYPE_KINDS;
        private static readonly string ALLOWED_TYPE_KIND_STRING;

        static Rule_5_5_1_3_FragmentTargetTypeMustBeOfAllowedKind()
        {
            ALLOWED_TYPE_KINDS = new HashSet<TypeKind>(new[] { TypeKind.OBJECT, TypeKind.UNION, TypeKind.INTERFACE });
            ALLOWED_TYPE_KIND_STRING = $"{TypeKind.OBJECT}, {TypeKind.UNION} or {TypeKind.INTERFACE}";
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var fragment = (IFragmentDocumentPart)context.ActivePart;

            if (string.IsNullOrEmpty(fragment.TargetGraphTypeName))
                return true;

            // shouldn't be false at this step, but just in case fail out
            if (fragment.GraphType == null)
                return false;

            if (!ALLOWED_TYPE_KINDS.Contains(fragment.GraphType.Kind))
            {
                this.ValidationError(
                    context,
                    $"The fragment declares a target graph type of '{fragment.GraphType.Name}' " +
                    $"of kind '{fragment.GraphType.Kind.ToString()}' but " +
                    $"fragments can only target graph types of kind {ALLOWED_TYPE_KIND_STRING}.");

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.5.1.3";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Fragments-On-Composite-Types";
    }
}