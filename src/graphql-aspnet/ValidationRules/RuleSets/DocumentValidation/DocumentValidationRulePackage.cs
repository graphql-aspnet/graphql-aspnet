// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.Interfaces;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.FieldSelectionSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryDirectiveSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryFragmentSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryInputArgumentSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryInputValueSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryOperationSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryVariableSteps;

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

        private readonly IDictionary<object, IList<IRuleStep<DocumentValidationContext>>> _stepCollection;

        /// <summary>
        /// Prevents a default instance of the <see cref="DocumentValidationRulePackage"/> class from being created.
        /// </summary>
        private DocumentValidationRulePackage()
        {
            _stepCollection = new Dictionary<object, IList<IRuleStep<DocumentValidationContext>>>();

            this.BuildQueryOperationSteps();

            this.BuildFragmentSteps();

            this.BuildFieldSelectionSteps();

            this.BuildQueryInputArgumentSteps();

            this.BuildQueryDirectiveSteps();

            this.BuildQueryVariableSteps();
        }

        /// <summary>
        /// Fetches the rules that should be executed, in order, for the given context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>IEnumerable&lt;IRuleStep&lt;TContext&gt;&gt;.</returns>
        public IEnumerable<IRuleStep<DocumentValidationContext>> FetchRules(DocumentValidationContext context)
        {
            var type = context?.ActivePart?.GetType();
            if (type == null || !_stepCollection.ContainsKey(type))
                return Enumerable.Empty<IRuleStep<DocumentValidationContext>>();

            return _stepCollection[type];
        }

        private void BuildQueryOperationSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();

            steps.Add(new Rule_5_2_2_1_LoneAnonymousOperation());
            steps.Add(new Rule_5_2_3_1_1_SubscriptionsRequire1EncounteredSubscriptionField());

            _stepCollection.Add(typeof(QueryOperation), steps);
        }

        private void BuildFragmentSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();

            // 1. All named fragments must be used at least once
            steps.Add(new Rule_5_5_1_4_AllDeclaredFragmentsMustBeUsed());

            _stepCollection.Add(typeof(QueryFragment), steps);
        }

        private void BuildFieldSelectionSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();

            // 1. All required, schema defined input arguments are supplied in the document
            steps.Add(new Rule_5_4_2_1_RequiredArgumentMustBeSuppliedOrHaveDefaultValueOnField());

            _stepCollection.Add(typeof(FieldSelection), steps);
        }

        private void BuildQueryDirectiveSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();

            // 1. All required, schema defined input arguments are supplied in the document
            steps.Add(new Rule_5_4_2_1_RequiredArgumentMustBeSuppliedOrHaveDefaultValueOnDirective());

            _stepCollection.Add(typeof(QueryDirective), steps);
        }

        private void BuildQueryInputArgumentSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();

            steps.Add(new Rule_5_6_1_ValueMustBeCoerceable());
            steps.Add(new Rule_5_6_4_InputObjectRequiredFieldsMustBeProvided());
            steps.Add(new Rule_5_8_5_VariableValueMustBeUsableInContext());

            _stepCollection.Add(typeof(QueryInputArgument), steps);
        }

        private void BuildQueryVariableSteps()
        {
            var steps = new List<IRuleStep<DocumentValidationContext>>();

            // 1. ensure that any variable declared on an operation is referenced at least once in said operation.
            steps.Add(new Rule_5_6_1_ValueMustBeCoerceable());
            steps.Add(new Rule_5_6_4_InputObjectRequiredFieldsMustBeProvided());
            steps.Add(new Rule_5_8_4_AllVariablesMustBeUsedInTheOperation());

            _stepCollection.Add(typeof(QueryVariable), steps);
        }
    }
}