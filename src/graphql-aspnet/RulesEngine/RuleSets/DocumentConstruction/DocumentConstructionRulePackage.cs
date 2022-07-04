// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.Interfaces;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Steps;

    /// <summary>
    /// A rule package containing rules for processing a nested hierarchy of syntax nodes.
    /// </summary>
    internal sealed class DocumentConstructionRulePackage : IRulePackage<DocumentConstructionContext>
    {
        /// <summary>
        /// Gets the singleton instance of this rule package.
        /// </summary>
        /// <value>The instance.</value>
        public static DocumentConstructionRulePackage Instance { get; } = new DocumentConstructionRulePackage();

        private readonly IDictionary<object, IList<IRuleStep<DocumentConstructionContext>>> _stepCollection;
        private readonly IList<IRuleStep<DocumentConstructionContext>> _topLevelSteps;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentConstructionRulePackage" /> class.
        /// </summary>
        private DocumentConstructionRulePackage()
        {
            _stepCollection = new Dictionary<object, IList<IRuleStep<DocumentConstructionContext>>>();
            _topLevelSteps = new List<IRuleStep<DocumentConstructionContext>>();

            // top level document
            this.BuildTopLevelDocumentSteps();

            // operation and variables
            this.BuildOperationSteps();
            this.BuildVariableCollectionSteps();
            this.BuildSingleVariableSteps();

            // fields
            this.BuildFieldCollectionSteps();
            this.BuildSingleFieldSteps();

            // directives
            this.BuildDirectiveSteps();

            // field and directive inputs
            this.BuildInputArgumentCollectionSteps();
            this.BuildSingleInputArgSteps();
            this.BuildInputValueSteps();

            // fragment type nodes
            this.BuildNamedFragmentSteps();
            this.BuildFragmentSpreadSteps();
            this.BuildInlineFragmentSteps();
        }

        /// <inheritdoc />
        public IEnumerable<IRuleStep<DocumentConstructionContext>> FetchRules(DocumentConstructionContext context)
        {
            var type = context?.ActiveNode?.GetType();
            if (type == null || !_stepCollection.ContainsKey(type))
                return Enumerable.Empty<IRuleStep<DocumentConstructionContext>>();

            // append special rules for root nodes of a query document when encountered
            return context.ParentContext == null
                ? _topLevelSteps.Concat(_stepCollection[type])
                : _stepCollection[type];
        }

        private void BuildTopLevelDocumentSteps()
        {
            // no steps for processing a root document node at this time
            // its skipped and the operation/framents are immediately processed
        }

        private void BuildOperationSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();
            steps.Add(new OperationNode_CreateOperationOnContext());

            _stepCollection.Add(typeof(OperationNode), steps);
        }

        private void BuildFieldCollectionSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();
            steps.Add(new FieldCollection_GenerateFieldSelectionSet());

            _stepCollection.Add(typeof(FieldCollectionNode), steps);
        }

        private void BuildVariableCollectionSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();
            steps.Add(new VariableCollectionNode_Skip());

            _stepCollection.Add(typeof(VariableCollectionNode), steps);
        }

        private void BuildSingleVariableSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();
            steps.Add(new QueryVariable_CreateNewVariable());

            _stepCollection.Add(typeof(VariableNode), steps);
        }

        private void BuildSingleFieldSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();
            steps.Add(new FieldSelection_CreateFieldOnContext());

            // special case for the '__typename' field on union graph types
            steps.Add(new FieldSelection_TypeNameMetaField_SpecialCase());

            _stepCollection.Add(typeof(FieldNode), steps);
        }

        private void BuildDirectiveSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();
            steps.Add(new DirectiveNode_CreateDirective());

            _stepCollection.Add(typeof(DirectiveNode), steps);
        }

        private void BuildInputArgumentCollectionSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();

            steps.Add(new InputItemCollection_Skip());

            _stepCollection.Add(typeof(InputItemCollectionNode), steps);
        }

        private void BuildSingleInputArgSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();
            steps.Add(new InputArgument_A_AssignContextQueryInputArgumentForField());
            steps.Add(new InputArgument_B_AssignContextQueryInputArgumentForDirective());
            steps.Add(new InputArgument_C_AssignContextQueryInputArgumentForInputObject());

            _stepCollection.Add(typeof(InputItemNode), steps);
        }

        private void BuildInputValueSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();
            steps.Add(new InputValue_AssignValueToArgumentOrValue());

            _stepCollection.Add(typeof(ListValueNode), steps);
            _stepCollection.Add(typeof(ComplexValueNode), steps);
            _stepCollection.Add(typeof(VariableValueNode), steps);
            _stepCollection.Add(typeof(ScalarValueNode), steps);
            _stepCollection.Add(typeof(EnumValueNode), steps);
            _stepCollection.Add(typeof(NullValueNode), steps);
        }

        private void BuildFragmentSpreadSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();
            steps.Add(new FragmentSpread_RegisterNamedFragmentToContext());

            _stepCollection.Add(typeof(FragmentSpreadNode), steps);
        }

        private void BuildInlineFragmentSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();
            steps.Add(new FragmentInline_CreateInlineFragmentOnDocument());
            _stepCollection.Add(typeof(InlineFragmentNode), steps);
        }

        private void BuildNamedFragmentSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();
            steps.Add(new FragmentNamed_CreateFragmentOnDocument());
            _stepCollection.Add(typeof(NamedFragmentNode), steps);
        }
    }
}