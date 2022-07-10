// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.RulesEngine.Interfaces;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.DocumentLevelSteps;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.FieldSelectionSetSteps;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.FieldSelectionSteps;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryDirectiveSteps;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryFragmentSteps;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryInputArgumentSteps;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryInputValueSteps;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryOperationSteps;

    /// <summary>
    /// A rule package for doing a wholistic validation pass at parsed query document before the final
    /// <see cref="IGraphQueryDocument"/> is generated. Performs deeper validations (such as no unused variables) across
    /// the fully parsed operations.
    /// </summary>
    internal sealed class DocumentValidationRulePackage : IRulePackage<DocumentValidationContext>
    {
        /// <summary>
        /// Gets the singleton instance of this rule package.
        /// </summary>
        /// <value>The instance.</value>
        public static DocumentValidationRulePackage Instance { get; } = new DocumentValidationRulePackage();

        private readonly IDictionary<DocumentPartType, IList<IRuleStep<DocumentValidationContext>>> _stepCollection;

        /// <summary>
        /// Prevents a default instance of the <see cref="DocumentValidationRulePackage"/> class from being created.
        /// </summary>
        private DocumentValidationRulePackage()
        {
            _stepCollection = new Dictionary<DocumentPartType, IList<IRuleStep<DocumentValidationContext>>>();

            this.BuildDocumentSteps();
            this.BuildOperationSteps();
            this.BuildNamedFragmentSteps();
            this.BuildInlineFragmentSteps();
            this.BuildFragmentSpreadSteps();
            this.BuildFieldSelectionSteps();
            this.BuildFieldSelectionSetSteps();
            this.BuildArgumentSteps();
            this.BuildDirectiveSteps();
            this.BuildSuppliedValueSteps();
        }

        /// <summary>
        /// Fetches the rules that should be executed, in order, for the given context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>IEnumerable&lt;IRuleStep&lt;TContext&gt;&gt;.</returns>
        public IEnumerable<IRuleStep<DocumentValidationContext>> FetchRules(DocumentValidationContext context)
        {
            var type = context?.ActivePart?.PartType ?? DocumentPartType.Unknown;
            if (!_stepCollection.ContainsKey(type))
                return Enumerable.Empty<IRuleStep<DocumentValidationContext>>();

            return _stepCollection[type];
        }

        private void BuildDocumentSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();
            steps.Add(new Rule_5_1_1_OnlyExecutableDefinition());
            steps.Add(new Rule_5_2_1_1_OperationNamesMustBeUnique());
            steps.Add(new Rule_5_2_2_1_LoneAnonymousOperation());

            _stepCollection.Add(DocumentPartType.Document, steps);
        }

        private void BuildFieldSelectionSetSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();
            steps.Add(new FieldSelectionSet_DocumentPartCheck());

            _stepCollection.Add(DocumentPartType.FieldSelectionSet, steps);
        }

        private void BuildSuppliedValueSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();
            steps.Add(new Rule_5_6_2_ComplexValueFieldsMustExistOnTargetGraphType());
            steps.Add(new Rule_5_6_3_InputObjectFieldNamesMustBeUnique());
            steps.Add(new Rule_5_6_4_InputObjectRequiredFieldsMustBeProvided());

            _stepCollection.Add(DocumentPartType.SuppliedValue, steps);
        }

        private void BuildFragmentSpreadSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();
            steps.Add(new Rule_5_5_2_1_SpreadOfNamedFragmentMustExist());
            steps.Add(new Rule_5_5_2_2_SpreadingANamedFragmentMustNotFormCycles());
            steps.Add(new Rule_5_5_2_3_1_ObjectFragmentSpreadInObjectCanSpreadInContext());
            steps.Add(new Rule_5_5_2_3_2_AbstractFragmentSpreadInObjectCanSpreadInContext());
            steps.Add(new Rule_5_5_2_3_3_ObjectFragmentSpreadInAbstractCanSpreadInContext());
            steps.Add(new Rule_5_5_2_3_4_AbstractFragmentSpreadInAbstractCanSpreadInContext());

            _stepCollection.Add(DocumentPartType.FragmentSpread, steps);
        }

        private void BuildOperationSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();
            steps.Add(new Rule_5_1_1_ExecutableOperationDefinition());
            steps.Add(new Rule_5_2_OperationTypeMustBeDefinedOnTheSchema());
            steps.Add(new Rule_5_2_3_1_SubscriptionsRequire1RootField());
            steps.Add(new Rule_5_2_3_1_1_SubscriptionsRequire1EncounteredSubscriptionField());

            // variable checks, the 5.8 series, are evaluated in context of the operation
            // due to the nature of fragment spreads and variables used
            steps.Add(new Rule_5_8_VariableDeclarationChecks());

            _stepCollection.Add(DocumentPartType.Operation, steps);
        }

        private void BuildNamedFragmentSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();
            steps.Add(new Rule_5_5_1_1_FragmentNamesMustBeUnique());
            steps.Add(new Rule_5_5_1_2_NamedFragmentGraphTypeMustExistInTheSchema());
            steps.Add(new Rule_5_5_1_3_FragmentTargetTypeMustBeOfAllowedKind());
            steps.Add(new Rule_5_5_1_4_AllDeclaredFragmentsMustBeUsed());

            _stepCollection.Add(DocumentPartType.NamedFragment, steps);
        }

        private void BuildInlineFragmentSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();
            steps.Add(new Rule_5_5_1_2_InlineFragmentGraphTypeMustExistInTheSchema());
            steps.Add(new Rule_5_5_1_3_FragmentTargetTypeMustBeOfAllowedKind());

            _stepCollection.Add(DocumentPartType.InlineFragment, steps);
        }

        private void BuildFieldSelectionSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();
            steps.Add(new FieldSelection_DocumentPartCheck());
            steps.Add(new Rule_5_3_1_FieldMustExistOnTargetGraphType());
            steps.Add(new Rule_5_3_2_FieldsOfIdenticalOutputMustHaveIdenticalSigs());
            steps.Add(new Rule_5_3_3_LeafReturnMustNotHaveChildFields());
            steps.Add(new Rule_5_4_2_1_RequiredArgumentMustBeSuppliedOrHaveDefaultValueOnField());

            _stepCollection.Add(DocumentPartType.Field, steps);
        }

        private void BuildDirectiveSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();
            steps.Add(new Rule_5_4_2_1_RequiredArgumentMustBeSuppliedOrHaveDefaultValueOnDirective());
            steps.Add(new Rule_5_7_1_DirectiveMustBeDefinedInTheSchema());
            steps.Add(new Rule_5_7_2_DirectiveMustBeUsedInValidLocation());
            steps.Add(new Rule_5_7_3_NonRepeatableDirectiveIsDefinedNoMoreThanOncePerLocation());

            _stepCollection.Add(DocumentPartType.Directive, steps);
        }

        private void BuildArgumentSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();
            steps.Add(new Rule_5_4_1_ArgumentMustBeDefinedOnTheField());
            steps.Add(new Rule_5_4_2_ArgumentMustBeUniquePerInvocation());
            steps.Add(new Rule_5_6_1_ValueMustBeCoerceable());

            _stepCollection.Add(DocumentPartType.Argument, steps);
        }
    }
}