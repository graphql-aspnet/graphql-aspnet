// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstruction
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstruction.Common;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.Interfaces;

    /// <summary>
    /// A rule package containing rules for processing a nested hierarchy of syntax nodes.
    /// </summary>
    internal sealed class DocumentConstructionRulePackage
    {
        /// <summary>
        /// Gets the singleton instance of this rule package.
        /// </summary>
        /// <value>The instance.</value>
        public static DocumentConstructionRulePackage Instance { get; } = new DocumentConstructionRulePackage();

        private readonly IDictionary<SynNodeType, IList<BaseDocumentConstructionRuleStep>> _stepCollection;
        private readonly IList<BaseDocumentConstructionRuleStep> _topLevelSteps;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentConstructionRulePackage" /> class.
        /// </summary>
        private DocumentConstructionRulePackage()
        {
            _stepCollection = new Dictionary<SynNodeType, IList<BaseDocumentConstructionRuleStep>>();
            _topLevelSteps = new List<BaseDocumentConstructionRuleStep>();

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
        public IEnumerable<BaseDocumentConstructionRuleStep> FetchRules(SynNodeType nodeType)
        {
            ;
            if (!_stepCollection.ContainsKey(nodeType))
                return Enumerable.Empty<BaseDocumentConstructionRuleStep>();

            // append special rules for root nodes of a query document when encountered
            return nodeType == SynNodeType.Document
                ? _topLevelSteps.Concat(_stepCollection[nodeType])
                : _stepCollection[nodeType];
        }

        private void BuildTopLevelDocumentSteps()
        {
            // no steps for processing a root document node at this time
            // its skipped and the operation/framents are immediately processed
        }

        private void BuildOperationSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new OperationNode_CreateOperationOnContext());

            _stepCollection.Add(SynNodeType.Operation, steps);
        }

        private void BuildFieldCollectionSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new FieldCollection_GenerateFieldSelectionSet());

            _stepCollection.Add(SynNodeType.FieldCollection, steps);
        }

        private void BuildVariableCollectionSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new VariableCollectionNode_Skip());

            _stepCollection.Add(SynNodeType.VariableCollection, steps);
        }

        private void BuildSingleVariableSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new QueryVariable_CreateNewVariable());

            _stepCollection.Add(SynNodeType.Variable, steps);
        }

        private void BuildSingleFieldSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new FieldSelection_CreateFieldOnContext());

            // special case for the '__typename' field on union graph types
            steps.Add(new FieldSelection_TypeNameMetaField_SpecialCase());

            _stepCollection.Add(SynNodeType.FieldCollection, steps);
        }

        private void BuildDirectiveSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new DirectiveNode_CreateDirective());

            _stepCollection.Add(SynNodeType.Directive, steps);
        }

        private void BuildInputArgumentCollectionSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();

            steps.Add(new InputItemCollection_Skip());

            _stepCollection.Add(SynNodeType.InputItemCollection, steps);
        }

        private void BuildSingleInputArgSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new InputArgument_A_AssignArgumentForField());
            steps.Add(new InputArgument_B_AssignInputArgumentForDirective());
            steps.Add(new InputArgument_C_AssignFieldForInputObject());

            _stepCollection.Add(SynNodeType.InputItem, steps);
        }

        private void BuildInputValueSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new InputValue_AssignValueToArgumentOrValue());

            _stepCollection.Add(SynNodeType.ListValue, steps);
            _stepCollection.Add(SynNodeType.ComplexValue, steps);
            _stepCollection.Add(SynNodeType.VariableValue, steps);
            _stepCollection.Add(SynNodeType.ScalarValue, steps);
            _stepCollection.Add(SynNodeType.EnumValue, steps);
            _stepCollection.Add(SynNodeType.NullValue, steps);
        }

        private void BuildFragmentSpreadSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new FragmentSpread_RegisterNamedFragmentToContext());

            _stepCollection.Add(SynNodeType.FragmentSpread, steps);
        }

        private void BuildInlineFragmentSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new FragmentInline_CreateInlineFragmentOnDocument());
            _stepCollection.Add(SynNodeType.InlineFragment, steps);
        }

        private void BuildNamedFragmentSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new FragmentNamed_CreateFragmentOnDocument());
            _stepCollection.Add(SynNodeType.NamedFragment, steps);
        }
    }
}