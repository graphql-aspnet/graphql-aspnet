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

        private readonly IDictionary<SyntaxNodeType, IList<BaseDocumentConstructionRuleStep>> _stepCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentConstructionRulePackage" /> class.
        /// </summary>
        private DocumentConstructionRulePackage()
        {
            _stepCollection = new Dictionary<SyntaxNodeType, IList<BaseDocumentConstructionRuleStep>>();

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
        public IEnumerable<BaseDocumentConstructionRuleStep> FetchRules(SyntaxNodeType nodeType)
        {
            ;
            if (!_stepCollection.ContainsKey(nodeType))
                return Enumerable.Empty<BaseDocumentConstructionRuleStep>();

            // append special rules for root nodes of a query document when encountered
            return _stepCollection[nodeType];
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

            _stepCollection.Add(SyntaxNodeType.Operation, steps);
        }

        private void BuildFieldCollectionSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new FieldCollection_GenerateFieldSelectionSet());

            _stepCollection.Add(SyntaxNodeType.FieldCollection, steps);
        }

        private void BuildVariableCollectionSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new VariableCollectionNode_Skip());

            _stepCollection.Add(SyntaxNodeType.VariableCollection, steps);
        }

        private void BuildSingleVariableSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new QueryVariable_CreateNewVariable());

            _stepCollection.Add(SyntaxNodeType.Variable, steps);
        }

        private void BuildSingleFieldSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new FieldSelection_CreateFieldOnContext());

            // special case for the '__typename' field on union graph types
            steps.Add(new FieldSelection_TypeNameMetaField_SpecialCase());

            _stepCollection.Add(SyntaxNodeType.Field, steps);
        }

        private void BuildDirectiveSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new DirectiveNode_CreateDirective());

            _stepCollection.Add(SyntaxNodeType.Directive, steps);
        }

        private void BuildInputArgumentCollectionSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();

            steps.Add(new InputItemCollection_Skip());

            _stepCollection.Add(SyntaxNodeType.InputItemCollection, steps);
        }

        private void BuildSingleInputArgSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new InputArgument_A_AssignArgumentForField());
            steps.Add(new InputArgument_B_AssignInputArgumentForDirective());
            steps.Add(new InputArgument_C_AssignFieldForInputObject());

            _stepCollection.Add(SyntaxNodeType.InputItem, steps);
        }

        private void BuildInputValueSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new InputValue_AssignValueToArgumentOrValue());

            _stepCollection.Add(SyntaxNodeType.ListValue, steps);
            _stepCollection.Add(SyntaxNodeType.ComplexValue, steps);
            _stepCollection.Add(SyntaxNodeType.VariableValue, steps);
            _stepCollection.Add(SyntaxNodeType.ScalarValue, steps);
            _stepCollection.Add(SyntaxNodeType.EnumValue, steps);
            _stepCollection.Add(SyntaxNodeType.NullValue, steps);
        }

        private void BuildFragmentSpreadSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new FragmentSpread_RegisterNamedFragmentToContext());

            _stepCollection.Add(SyntaxNodeType.FragmentSpread, steps);
        }

        private void BuildInlineFragmentSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new FragmentInline_CreateInlineFragmentOnDocument());
            _stepCollection.Add(SyntaxNodeType.InlineFragment, steps);
        }

        private void BuildNamedFragmentSteps()
        {
            var steps = new List<BaseDocumentConstructionRuleStep>();
            steps.Add(new FragmentNamed_CreateFragmentOnDocument());
            _stepCollection.Add(SyntaxNodeType.NamedFragment, steps);
        }
    }
}