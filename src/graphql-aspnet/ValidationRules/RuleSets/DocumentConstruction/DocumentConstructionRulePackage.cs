// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.Interfaces;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.DirectiveNodeSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.FieldColletionNodeSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.FieldNodeSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.FragmentInlineNodeSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.FragmentSpreadNodeSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.InputItemCollectionNodeSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.InputItemNodeSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.InputValueNodeSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.NamedFragmentNodeSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.OperationNodeSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.QueryFragmentSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.TopLevelNodeSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.VariableCollectionNodeSteps;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.VariableNodeSteps;

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

            // build the top level rules for nodes declared at the outermost level
            // of the document then build the matched specifics for the expected defintions (operation and fragment)
            _topLevelSteps.Add(new Rule_5_1_1_ExecutableDefinitions());

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

        /// <summary>
        /// Fetches the rules that should be executed, in order, for the given context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>IEnumerable&lt;IRuleStep&lt;TContext&gt;&gt;.</returns>
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

        /// <summary>
        /// Builds the rule chain required to successfully process the top level operation node.
        /// </summary>
        private void BuildOperationSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();

            // 0.  Ensure that the type of operation (mutation, query etc.) exists on the schema
            steps.Add(new Rule_5_2_OperationTypeMustBeDefinedOnTheSchema());

            // 1. Ensure that the operation type (query, mutation, subscription) exists for the
            //    target schema
            steps.Add(new Rule_5_2_1_1_OperationNamesMustBeUnique());

            // 2. Add the node to context
            steps.Add(new OperationNode_CreateOperationOnContext());

            // 3. Ensure that subscriptions have exactly one root field
            steps.Add(new Rule_5_2_3_1_SubscriptionsRequire1RootField());
            steps.Add(new Rule_DenySubscriptions());

            _stepCollection.Add(typeof(OperationTypeNode), steps);
        }

        private void BuildFieldCollectionSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();

            // 1. Generate a new context field set whenever it is encountered.
            steps.Add(new FieldCollection_GenerateFieldSelectionSet());

            _stepCollection.Add(typeof(FieldCollectionNode), steps);
        }

        private void BuildVariableCollectionSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();

            // 1. Ensure that variables are only declared on operations
            steps.Add(new Rule_5_8_OnlyOperationsCanDeclareVariables());

            _stepCollection.Add(typeof(VariableCollectionNode), steps);
        }

        private void BuildSingleVariableSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();

            // for each variable in the collection
            // 1. ensure a unique name
            steps.Add(new Rule_5_8_1_VariableNamesMustBeUnique());

            // 2. add the variable to active collection
            steps.Add(new QueryVariable_CreateNewVariableOnContext());

            // 3. ensure the variable declares a type declaration
            steps.Add(new Rule_5_8_2_A_VariablesMustDeclareAType());

            // 4. ensure that type declaration is valid (exists in the schema)
            steps.Add(new Rule_5_8_2_B_VariablesMustDeclareAValidGraphType());

            // 5. assign the graph type to the variable
            steps.Add(new QueryVariable_AssignGraphTypeStep());

            // 6. ensure that the graph type is allowed in context (ENUM, SCALAR, INPUT_OBJECT)
            steps.Add(new Rule_5_8_2_C_VariableGraphTypeMustBeOfAllowedTypeKinds());

            _stepCollection.Add(typeof(VariableNode), steps);
        }

        private void BuildSingleFieldSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();

            // 1. ensure the field exists on the "in context" target graph type
            steps.Add(new Rule_5_3_1_FieldMustExistOnTargetGraphType());

            // 2. Create the field and set as the field on the active context
            //    Also add it to the active selection set
            steps.Add(new FieldSelection_CreateFieldOnContext());

            // 3. Check the field against hte other fields in the active selection set, if it
            //    matches output name insure it has an identicial signature
            steps.Add(new Rule_5_3_2_FieldsOfIdenticalOutputMustHaveIdenticalSigs());

            // 4. ensure no down stream fields exist when this field's return type is a leaf
            //    (i.e. scalars dont have fields)
            steps.Add(new Rule_5_3_3_LeafReturnMustNotHaveChildFields());

            // special case for the '__typename' field on union graph types
            steps.Add(new TypeNameMetaField_SpecialCase());

            _stepCollection.Add(typeof(FieldNode), steps);
        }

        private void BuildDirectiveSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();

            // 1. Ensure the directive is defined on the schema
            steps.Add(new Rule_5_7_1_DirectiveMustBeDefinedInTheSchema());

            // 2. create the entry for the directive in the document
            steps.Add(new QueryDirective_CreateDirectiveOnContext());

            // 3. Ensure the directive is used in a valid location on the document
            steps.Add(new Rule_5_7_2_DirectiveMustBeUsedInValidLocation());

            // 4. Ensure the directive is only used once in the location where it is declared
            steps.Add(new Rule_5_7_3_DirectiveIsDefinedNoMoreThanOncePerLocation());

            _stepCollection.Add(typeof(DirectiveNode), steps);
        }

        private void BuildInputArgumentCollectionSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();

            // 1. Ensure that input arguments (a collection of them) only exists on fields or directives
            steps.Add(new Rule_5_4_InputItemsOnlyOnFieldsOrDirectives());

            _stepCollection.Add(typeof(InputItemCollectionNode), steps);
        }

        private void BuildSingleInputArgSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();

            // 1. Ensure the nodes are valid for the given scope
            // field arguments
            steps.Add(new Rule_5_4_1_A_ArgumentMustBeDefinedOnTheField());
            steps.Add(new Rule_5_4_2_A_ArgumentMustBeUniqueOnTheField());

            // directive arguments
            steps.Add(new Rule_5_4_1_B_ArgumentMustBeDefinedOnTheDirective());
            steps.Add(new Rule_5_4_2_B_ArgumentMustBeUniqueOnTheDirective());

            // INPUT_OBJECT arguments
            steps.Add(new Rule_5_6_2_InputObjectFieldsMustExistOnTargetGraphType());
            steps.Add(new Rule_5_6_3_InputObjectFieldNamesMustBeUnique());

            // 2. create a new query argument on the context
            steps.Add(new InputArgument_A_AssignContextQueryInputArgumentForField());
            steps.Add(new InputArgument_B_AssignContextQueryInputArgumentForDirective());
            steps.Add(new InputArgument_C_AssignContextQueryInputArgumentForInputObject());

            _stepCollection.Add(typeof(InputItemNode), steps);
        }

        private void BuildInputValueSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();

            // 1. Ensure variable references are allowed in the scoped operation
            steps.Add(new Rule_5_8_3_AllUsedVariablesMustBeDeclaredOnTheOperation());

            // 2. Assign the input value to the active argument on the context
            steps.Add(new InputValue_AssignValueToArgumentOrValue());
            steps.Add(new InputValue_AssignVariableReference());

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

            // 1. Ensure that the fragment pointed to by this spread node
            //    actually exists on the target document
            steps.Add(new Rule_5_5_2_1_SpreadOfNamedFragmentMustExist());

            // 2. Check for any cyclic references of named fragments that would
            //    result in an infite fragment resolution
            steps.Add(new Rule_5_5_2_2_SpreadingANamedFragmentMustNotFormCycles());

            // 3. Ensure that based on where the named fragment is being spread
            //    that it COULD be spead into the current context
            steps.Add(new Rule_5_5_2_3_1_ObjectFragmentSpreadInObjectCanSpreadInContext());
            steps.Add(new Rule_5_5_2_3_2_AbstractFragmentSpreadInObjectCanSpreadInContext());
            steps.Add(new Rule_5_5_2_3_3_ObjectFragmentSpreadInAbstractCanSpreadInContext());
            steps.Add(new Rule_5_5_2_3_4_AbstractFragmentSpreadInAbstractCanSpreadInContext());

            // 3. Set the active query fragment to be the fragment pointed to by the spread
            steps.Add(new FragmentSpread_RegisterNamedFragmentToContext());

            _stepCollection.Add(typeof(FragmentSpreadNode), steps);
        }

        private void BuildInlineFragmentSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();

            // 1. set the fragment node to be the QueryFragment that on the active context
            steps.Add(new InlineFragment_RegisterFragmentToContext());

            // process the context query fragment
            this.AddQueryFragmentSteps(steps);
            _stepCollection.Add(typeof(FragmentNode), steps);
        }

        private void BuildNamedFragmentSteps()
        {
            var steps = new List<IRuleStep<DocumentConstructionContext>>();

            // 1. Ensure unique name on the document
            steps.Add(new Rule_5_5_1_1_FragmentNamesMustBeUnique());

            // 2. Create the named fragment and register it with the master document context.
            steps.Add(new NamedFragment_CreateFragmentOnDocument());

            // process the finish processing the fragment
            this.AddQueryFragmentSteps(steps);
            _stepCollection.Add(typeof(NamedFragmentNode), steps);
        }

        /// <summary>
        /// Adds a collection of steps that are common between inlined fragments and named fragments that operate
        /// against a <see cref="QueryFragment"/> placed onto the active context.
        /// </summary>
        /// <param name="steps">The step collection to populate.</param>
        private void AddQueryFragmentSteps(IList<IRuleStep<DocumentConstructionContext>> steps)
        {
            // 1. Ensure that the type on the fragment (if declared) exists in the target schema
            steps.Add(new Rule_5_5_1_2_FragmentGraphTypeMustExistInTheSchema());

            // 2. Assign the target graph type to the query fragment
            steps.Add(new QueryFragment_AssignGraphType());

            // 3. Ensure that the target type (if declared) is one of the allowed types (INTERFACE, UNION, OBJECT)
            steps.Add(new Rule_5_5_1_3_FragmentTargetTypeMustBeOfAllowedKind());
        }
    }
}