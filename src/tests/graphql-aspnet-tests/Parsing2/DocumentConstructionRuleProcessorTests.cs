// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing2
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.DocumentGeneration;
    using GraphQL.AspNet.Parsing2.Lexing.Source;
    using GraphQL.AspNet.PlanGeneration.Document;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Parsing2.DocumentConstructionTestData;
    using NUnit.Framework;

    using DocumentConstructionContext = GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstructionContext;

    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class DocumentConstructionRuleProcessorTests
    {
        private GraphSchema _schema;
        private DocumentConstructionRuleProcessor _documentProcessor;
        private IObjectGraphType _controllerGraphType;
        private IObjectGraphType _donutGraphType;
        private IInputObjectGraphType _inputDonutGraphType;
        private IEnumGraphType _donutFlavorGraphType;
        private IObjectGraphType _bagelGraphType;
        private IDirective _directive1;
        private IDirective _directive3;

        public DocumentConstructionRuleProcessorTests()
        {
            _schema = new Framework.TestServerBuilder()
                .AddGraphController<BakeryController>()
                .AddDirective<Directive1>()
                .Build()
                .Schema;

            // test object
            _documentProcessor = new DocumentConstructionRuleProcessor();

            _controllerGraphType = _schema.KnownTypes.FindGraphType("Query_BakeryController") as IObjectGraphType;
            _donutGraphType = _schema.KnownTypes.FindGraphType(typeof(Donut)) as IObjectGraphType;
            _inputDonutGraphType = _schema.KnownTypes.FindGraphType(typeof(Donut), TypeKind.INPUT_OBJECT) as IInputObjectGraphType;
            _donutFlavorGraphType = _schema.KnownTypes.FindGraphType(typeof(DonutFlavor)) as IEnumGraphType;
            _bagelGraphType = _schema.KnownTypes.FindGraphType(typeof(Bagel)) as IObjectGraphType;
            _directive1 = _schema.KnownTypes.FindDirective(typeof(Directive1));
            _directive3 = _schema.KnownTypes.FindDirective(typeof(Directive3));
        }

        private void AssertInputDonut(
            IComplexSuppliedValueDocumentPart item,
            int id,
            string name,
            string flavor)
        {
            var idValue = item.Children.OfType<IInputObjectFieldDocumentPart>()
                .SingleOrDefault(x => x.Name == "id")?
                .Children
                .OfType<IScalarSuppliedValue>()
                .SingleOrDefault()
                .Value.ToString();

            Assert.AreEqual(id.ToString(), idValue);

            var nameValue = item.Children.OfType<IInputObjectFieldDocumentPart>()
                .SingleOrDefault(x => x.Name == "name")?
                .Children
                .OfType<IScalarSuppliedValue>()
                .SingleOrDefault()
                .Value.ToString();

            Assert.AreEqual(name, nameValue);

            var flavorValue = item.Children.OfType<IInputObjectFieldDocumentPart>()
                .SingleOrDefault(x => x.Name == "flavor")?
                .Children
                .OfType<IEnumSuppliedValueDocumentPart>()
                .SingleOrDefault()
                .Value.ToString();

            Assert.AreEqual(flavor, flavorValue);
        }

        private void AssertInputBagel(
            IComplexSuppliedValueDocumentPart item,
            long id,
            string name,
            bool isHot,
            int? orderCreated)
        {
            var idValue = item.Children.OfType<IInputObjectFieldDocumentPart>()
                .SingleOrDefault(x => x.Name == "id")?
                .Children
                .OfType<IScalarSuppliedValue>()
                .SingleOrDefault()
                .Value.ToString();

            Assert.AreEqual(id.ToString(), idValue);

            var nameValue = item.Children.OfType<IInputObjectFieldDocumentPart>()
                .SingleOrDefault(x => x.Name == "name")?
                .Children
                .OfType<IScalarSuppliedValue>()
                .SingleOrDefault()
                .Value.ToString();

            Assert.AreEqual(name, nameValue);

            var isHotValue = item.Children.OfType<IInputObjectFieldDocumentPart>()
                .SingleOrDefault(x => x.Name == "isHot")?
                .Children
                .OfType<IScalarSuppliedValue>()
                .SingleOrDefault()
                .Value.ToString();

            Assert.AreEqual(isHot.ToString().ToLower(), isHotValue);

            var orderCreatedValue = item.Children.OfType<IInputObjectFieldDocumentPart>()
                .SingleOrDefault(x => x.Name == "orderCreated")?
                .Children
                .OfType<IScalarSuppliedValue>()
                .SingleOrDefault()
                .Value.ToString();

            Assert.AreEqual(orderCreated.ToString(), orderCreatedValue);
        }

        private SynTree CreateSyntaxTree(ref SourceText sourceText)
        {
            var parser = new GraphQLParser2();
            return parser.ParseQueryDocument(ref sourceText);
        }

        private IGraphQueryDocument CreateDocument(SynTree syntaxTree, SourceText sourceText)
        {
            var doc = new QueryDocument();
            var context = new DocumentConstructionContext(
                syntaxTree,
                sourceText,
                doc,
                _schema);

            _documentProcessor.Execute(ref context);
            return doc;
        }

        [Test]
        public void BasicQueryDocumentContentsCheck()
        {
            var text = @"
                query MyQuery {
                    retrieveAllDonuts {
                        id
                        name
                        bigFlavor: flavor
                    }
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.AreEqual(2, result.MaxDepth);
            Assert.AreEqual(1, result.Operations.Count);

            var operation = result.Operations[0];

            Assert.AreEqual("MyQuery", operation.Name);
            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());

            var retrieveDonuts = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.IsNotNull(retrieveDonuts);

            Assert.AreEqual("retrieveAllDonuts", retrieveDonuts.Name.ToString());
            Assert.IsNotNull(retrieveDonuts.FieldSelectionSet);
            Assert.AreEqual(3, retrieveDonuts.FieldSelectionSet.ExecutableFields.Count());
            Assert.IsNotNull(retrieveDonuts.Field);
            Assert.AreEqual("retrieveAllDonuts", retrieveDonuts.Field.Name.ToString());
            Assert.AreEqual(_schema.Operations[GraphOperationType.Query], retrieveDonuts.Field.Parent);

            var id = retrieveDonuts.FieldSelectionSet.ExecutableFields[0];
            var name = retrieveDonuts.FieldSelectionSet.ExecutableFields[1];
            var flavor = retrieveDonuts.FieldSelectionSet.ExecutableFields[2];

            Assert.IsNotNull(id);
            Assert.AreEqual("id", id.Name.ToString());
            Assert.AreEqual("id", id.Alias.ToString());
            Assert.IsNotNull(id.GraphType);
            Assert.IsNotNull(id.Field);
            Assert.AreEqual(id.Field.Parent, _donutGraphType);
            Assert.AreEqual(Constants.ScalarNames.INT, id.GraphType.Name);
            Assert.IsNull(id.FieldSelectionSet);
            Assert.AreEqual(0, id.Directives.Count);
            Assert.AreEqual(0, id.Arguments.Count);

            Assert.IsNotNull(name);
            Assert.AreEqual("name", name.Name.ToString());
            Assert.AreEqual("name", name.Alias.ToString());
            Assert.IsNotNull(name.GraphType);
            Assert.IsNotNull(name.Field);
            Assert.AreEqual(name.Field.Parent, _donutGraphType);
            Assert.AreEqual(Constants.ScalarNames.STRING, name.GraphType.Name);
            Assert.IsNull(name.FieldSelectionSet);
            Assert.AreEqual(0, name.Directives.Count);
            Assert.AreEqual(0, name.Arguments.Count);

            Assert.IsNotNull(flavor);
            Assert.AreEqual("flavor", flavor.Name.ToString());
            Assert.AreEqual("bigFlavor", flavor.Alias.ToString());
            Assert.IsNotNull(flavor.GraphType);
            Assert.IsNotNull(flavor.Field);
            Assert.AreEqual(flavor.Field.Parent, _donutGraphType);
            Assert.AreEqual(nameof(DonutFlavor), flavor.GraphType.Name);
            Assert.IsNull(flavor.FieldSelectionSet);
            Assert.AreEqual(0, flavor.Directives.Count);
            Assert.AreEqual(0, flavor.Arguments.Count);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void SingleScalarArgumentOnFieldContentsCheck()
        {
            var text = @"
                query MyQuery {
                    bakery {
                        retrieveDonut(id: 3) {
                            id
                            name
                            bigFlavor: flavor
                        }
                    }
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.AreEqual(3, result.MaxDepth);
            Assert.AreEqual(1, result.Operations.Count);

            var operation = result.Operations[0];

            Assert.AreEqual("MyQuery", operation.Name);
            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());

            var bakery = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.IsNotNull(bakery);

            Assert.AreEqual("bakery", bakery.Name.ToString());
            Assert.IsNotNull(bakery.FieldSelectionSet);
            Assert.AreEqual(1, bakery.FieldSelectionSet.ExecutableFields.Count());
            Assert.IsNotNull(bakery.Field);
            Assert.AreEqual("bakery", bakery.Field.Name.ToString());
            Assert.AreEqual(_schema.Operations[GraphOperationType.Query], bakery.Field.Parent);

            var retrieveDonut = bakery.FieldSelectionSet.ExecutableFields[0];
            Assert.IsNotNull(retrieveDonut);

            Assert.AreEqual("retrieveDonut", retrieveDonut.Name.ToString());
            Assert.IsNotNull(retrieveDonut.FieldSelectionSet);
            Assert.AreEqual(3, retrieveDonut.FieldSelectionSet.ExecutableFields.Count());
            Assert.IsNotNull(retrieveDonut.Field);
            Assert.AreEqual("retrieveDonut", retrieveDonut.Field.Name.ToString());

            // should be virtual type "bakery"
            Assert.IsTrue(((IGraphType)retrieveDonut.Field.Parent).IsVirtual);

            var args = retrieveDonut.Arguments;
            Assert.IsNotNull(args);
            Assert.AreEqual(1, args.Count);

            var firstArg = args.First();
            Assert.AreEqual("id", firstArg.Key);
            Assert.AreEqual("id", firstArg.Value.Name);
            Assert.AreEqual("Int!", firstArg.Value.Argument.TypeExpression.ToString());

            var argValue = firstArg.Value.Value as DocumentScalarSuppliedValue;
            Assert.IsNotNull(argValue);
            Assert.AreEqual("3", argValue.Value.ToString());
            Assert.AreEqual(ScalarValueType.Number, (ScalarValueType)argValue.ValueType);
            Assert.AreEqual(Constants.ScalarNames.INT, argValue.GraphType.Name);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void SingleNullArgumentOnFieldContentsCheck()
        {
            var text = @"
                query MyQuery {
                    bakery {
                        retrieveDonutByNullableId(id: null) {
                            id
                        }
                    }
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.AreEqual(3, result.MaxDepth);
            Assert.AreEqual(1, result.Operations.Count);

            var operation = result.Operations[0];

            Assert.AreEqual("MyQuery", operation.Name);

            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());

            var bakery = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.IsNotNull(bakery);

            Assert.AreEqual("bakery", bakery.Name.ToString());
            Assert.IsNotNull(bakery.FieldSelectionSet);
            Assert.AreEqual(1, bakery.FieldSelectionSet.ExecutableFields.Count());
            Assert.IsNotNull(bakery.Field);
            Assert.AreEqual("bakery", bakery.Field.Name.ToString());
            Assert.AreEqual(_schema.Operations[GraphOperationType.Query], bakery.Field.Parent);

            var retrieveDonut = bakery.FieldSelectionSet.ExecutableFields[0];
            Assert.IsNotNull(retrieveDonut);

            Assert.AreEqual("retrieveDonutByNullableId", retrieveDonut.Name.ToString());
            Assert.IsNotNull(retrieveDonut.FieldSelectionSet);
            Assert.AreEqual(1, retrieveDonut.FieldSelectionSet.ExecutableFields.Count());
            Assert.IsNotNull(retrieveDonut.Field);
            Assert.AreEqual("retrieveDonutByNullableId", retrieveDonut.Field.Name.ToString());

            // should be virtual type "bakery"
            Assert.IsTrue(((IGraphType)retrieveDonut.Field.Parent).IsVirtual);

            var args = retrieveDonut.Arguments;
            Assert.IsNotNull(args);
            Assert.AreEqual(1, args.Count);

            var firstArg = args.First();
            Assert.AreEqual("id", firstArg.Key);
            Assert.AreEqual("id", firstArg.Value.Name);
            Assert.AreEqual("Int", firstArg.Value.Argument.TypeExpression.ToString());

            var argValue = firstArg.Value.Value as DocumentNullSuppliedValue;
            Assert.IsNotNull(argValue);
            Assert.AreEqual(Constants.ScalarNames.INT, argValue.GraphType.Name);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void SingleListArgumentOnFieldContentsCheck()
        {
            var text = @"
                query MyQuery {
                    bakery {
                        retrieveBagelsById(bagelNumbers: [0,2,5,9]) {
                            id
                        }
                    }
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.AreEqual(3, result.MaxDepth);
            Assert.AreEqual(1, result.Operations.Count);

            var operation = result.Operations[0];

            Assert.AreEqual("MyQuery", operation.Name);
            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());

            var bakery = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.IsNotNull(bakery);

            Assert.AreEqual("bakery", bakery.Name.ToString());
            Assert.IsNotNull(bakery.FieldSelectionSet);
            Assert.AreEqual(1, bakery.FieldSelectionSet.ExecutableFields.Count());
            Assert.IsNotNull(bakery.Field);
            Assert.AreEqual("bakery", bakery.Field.Name.ToString());
            Assert.AreEqual(_schema.Operations[GraphOperationType.Query], bakery.Field.Parent);

            var retrieveBagelsById = bakery.FieldSelectionSet.ExecutableFields[0];
            Assert.IsNotNull(retrieveBagelsById);

            Assert.AreEqual("retrieveBagelsById", retrieveBagelsById.Name.ToString());
            Assert.IsNotNull(retrieveBagelsById.FieldSelectionSet);
            Assert.AreEqual(1, retrieveBagelsById.FieldSelectionSet.ExecutableFields.Count());
            Assert.IsNotNull(retrieveBagelsById.Field);
            Assert.AreEqual("retrieveBagelsById", retrieveBagelsById.Field.Name.ToString());

            // should be virtual type "bakery"
            Assert.IsTrue(((IGraphType)retrieveBagelsById.Field.Parent).IsVirtual);

            var args = retrieveBagelsById.Arguments;
            Assert.IsNotNull(args);
            Assert.AreEqual(1, args.Count);

            var firstArg = args.First();
            Assert.AreEqual("bagelNumbers", firstArg.Key);
            Assert.AreEqual("bagelNumbers", firstArg.Value.Name);
            Assert.AreEqual("[Int!]", firstArg.Value.Argument.TypeExpression.ToString());

            var argValue = firstArg.Value.Value as DocumentListSuppliedValue;
            Assert.IsNotNull(argValue);
            Assert.AreEqual(Constants.ScalarNames.INT, argValue.GraphType.Name);

            Assert.AreEqual(4, argValue.ListItems.Count());
            Assert.AreEqual("0", ((IScalarSuppliedValue)argValue.ListItems.ElementAt(0)).Value.ToString());
            Assert.AreEqual("2", ((IScalarSuppliedValue)argValue.ListItems.ElementAt(1)).Value.ToString());
            Assert.AreEqual("5", ((IScalarSuppliedValue)argValue.ListItems.ElementAt(2)).Value.ToString());
            Assert.AreEqual("9", ((IScalarSuppliedValue)argValue.ListItems.ElementAt(3)).Value.ToString());

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void SingleComplexArgumentOnFieldContentsCheck()
        {
            var text = @"
                query MyQuery {
                    addDonut(newDonut: { id: 5 name: ""donut1"" flavor: STRAWBERRY }) {
                        id
                    }
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.AreEqual(2, result.MaxDepth);
            Assert.AreEqual(1, result.Operations.Count);

            var operation = result.Operations[0];

            Assert.AreEqual("MyQuery", operation.Name);
            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());

            var addDonut = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.IsNotNull(addDonut);

            Assert.AreEqual("addDonut", addDonut.Name.ToString());
            Assert.IsNotNull(addDonut.FieldSelectionSet);
            Assert.AreEqual(1, addDonut.FieldSelectionSet.ExecutableFields.Count());
            Assert.IsNotNull(addDonut.Field);
            Assert.AreEqual("addDonut", addDonut.Field.Name.ToString());
            Assert.AreEqual(_schema.Operations[GraphOperationType.Query], addDonut.Field.Parent);

            var args = addDonut.Arguments;
            Assert.IsNotNull(args);
            Assert.AreEqual(1, args.Count);

            var firstArg = args.First();
            Assert.AreEqual("newDonut", firstArg.Key);
            Assert.AreEqual("newDonut", firstArg.Value.Name);
            Assert.AreEqual(_inputDonutGraphType.Name, firstArg.Value.Argument.TypeExpression.ToString());

            var argValue = firstArg.Value.Value as DocumentComplexSuppliedValue;
            Assert.IsNotNull(argValue);
            Assert.AreEqual(_inputDonutGraphType, argValue.GraphType);

            var inputFields = argValue.Children.OfType<IInputObjectFieldDocumentPart>().ToList();
            Assert.AreEqual(3, inputFields.Count);

            var id = inputFields.FirstOrDefault(x => x.Name == "id");
            var name = inputFields.FirstOrDefault(x => x.Name == "name");
            var flavor = inputFields.FirstOrDefault(x => x.Name == "flavor");

            Assert.IsNotNull(id);
            Assert.AreEqual("id", id.Name);
            Assert.AreEqual(Constants.ScalarNames.INT, id.GraphType.Name);
            Assert.AreEqual("5", ((IScalarSuppliedValue)id.Value).Value.ToString());

            Assert.IsNotNull(name);
            Assert.AreEqual("name", name.Name);
            Assert.AreEqual(Constants.ScalarNames.STRING, name.GraphType.Name);
            Assert.AreEqual("\"donut1\"", ((IScalarSuppliedValue)name.Value).Value.ToString());

            Assert.IsNotNull(flavor);
            Assert.AreEqual("flavor", flavor.Name);
            Assert.AreEqual(_donutFlavorGraphType, flavor.GraphType);
            Assert.AreEqual("STRAWBERRY", ((IEnumSuppliedValueDocumentPart)flavor.Value).Value.ToString());

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableReferenceOnArgumentContentCheck()
        {
            var text = @"
                query MyQuery {
                    addDonut(newDonut: $var1)
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.AreEqual(1, result.MaxDepth);
            Assert.AreEqual(1, result.Operations.Count);

            var operation = result.Operations[0];
            var addDonut = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.IsNotNull(addDonut);

            var args = addDonut.Arguments;
            Assert.IsNotNull(args);
            Assert.AreEqual(1, args.Count);

            var firstArg = args.First();
            Assert.AreEqual("newDonut", firstArg.Key);

            var argValue = firstArg.Value.Value as DocumentVariableUsageValue;
            Assert.IsNotNull(argValue);
            Assert.AreEqual("var1", argValue.VariableName.ToString());

            // the expected graph type of the value is that of hte parent argument
            Assert.AreEqual(_inputDonutGraphType, argValue.GraphType);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableDeclarationContentsCheck()
        {
            var text = @"
                query MyQuery($var1: Int!) {
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.AreEqual(1, result.Operations.Count);

            var operation = result.Operations[0];

            Assert.AreEqual("MyQuery", operation.Name);

            var variables = operation.Variables;
            Assert.AreEqual(1, variables.Count);

            var var1 = variables.First();
            Assert.AreEqual("var1", var1.Name);
            Assert.AreEqual("Int!", var1.TypeExpression.ToString());
            Assert.AreEqual(Constants.ScalarNames.INT, var1.GraphType.Name);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void VariableDeclarationWithDefaultValueContentsCheck()
        {
            var text = @"
                query MyQuery($var1: Int! = 13) {
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.AreEqual(1, result.Operations.Count);

            var operation = result.Operations[0];

            Assert.AreEqual("MyQuery", operation.Name);

            var variables = operation.Children.OfType<IVariableDocumentPart>().ToList();
            Assert.AreEqual(1, variables.Count);

            var var1 = variables[0];
            Assert.AreEqual("var1", var1.Name);
            Assert.AreEqual("Int!", var1.TypeExpression.ToString());
            Assert.AreEqual(Constants.ScalarNames.INT, var1.GraphType.Name);

            Assert.AreEqual(1, var1.Children.Count);

            var value = var1.Children.First() as DocumentScalarSuppliedValue;
            Assert.IsNotNull(value);
            Assert.AreEqual("13", value.Value.ToString());
            Assert.AreEqual(ScalarValueType.Number, (ScalarValueType)value.ValueType);
            Assert.AreEqual(Constants.ScalarNames.INT, value.GraphType.Name);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void MultiArgumentsOnFieldContentCheck()
        {
            var text = @"
                query MyQuery {
                    multiScalarArguments(id: 3, count: 22)
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.AreEqual(1, result.MaxDepth);
            Assert.AreEqual(1, result.Operations.Count);

            var operation = result.Operations[0];
            var multiScalarArguments = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.IsNotNull(multiScalarArguments);

            var args = multiScalarArguments
                .Children
                .OfType<IInputArgumentDocumentPart>()
                .ToList();

            Assert.IsNotNull(args);
            Assert.AreEqual(2, args.Count);

            var firstArg = args.First();
            var secondArg = args.Skip(1).First();

            Assert.AreEqual("id", firstArg.Name);
            Assert.AreEqual("3", ((IScalarSuppliedValue)firstArg.Value).Value.ToString());

            Assert.AreEqual("count", secondArg.Name);
            Assert.AreEqual("22", ((IScalarSuppliedValue)secondArg.Value).Value.ToString());

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void DeepComplexArgumentContentCheck()
        {
            var text = @"
                query MyQuery {
                    complexScenario(items: [
                        {
                            donuts: [
                                {id: 1, name: ""Donut1"", flavor: STRAWBERRY}
                                {id: 2, name: ""Donut2"" flavor: VANILLA}
                            ]
                            bagels: [
                                {id:3 name: ""Bagel3"" isHot: true orderCreated: 3}
                                {id:4 name: ""Bagel4"" isHot: false orderCreated: $orderCount}
                            ]
                            singleDonut : {id: 5, name: ""Donut5"", flavor: CHOCOLATE}
                            singleBagel : {id:6 name: ""Bagel6"" isHot: true orderCreated: 19}
                        }
                        {
                            singleBagel : {id: 12 name: ""Bagel12"" isHot: true orderCreated: 45}
                        }
                    ])
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.AreEqual(1, result.MaxDepth);
            Assert.AreEqual(1, result.Operations.Count);

            var operation = result.Operations[0];
            var complexScenario = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.IsNotNull(complexScenario);

            var args = complexScenario
                .Children.OfType<IInputArgumentDocumentPart>().ToList();

            Assert.IsNotNull(args);
            Assert.AreEqual(1, args.Count);

            var firstArgItems = args.First();
            Assert.AreEqual("items", firstArgItems.Name);

            var values = (IListSuppliedValueDocumentPart)firstArgItems.Value;
            Assert.AreEqual(2, values.ListItems.Count());

            var firstItem = values.First() as DocumentComplexSuppliedValue;
            var secondItem = values.Skip(1).First() as DocumentComplexSuppliedValue;

            // Validate first array entry
            var donutsValue = firstItem
                .Children.OfType<IInputObjectFieldDocumentPart>().Single(x => x.Name == "donuts")
                .Children.OfType<IListSuppliedValueDocumentPart>().Single()
                .Children.OfType<IComplexSuppliedValueDocumentPart>()
                .ToList();

            this.AssertInputDonut(donutsValue[0], 1, "\"Donut1\"", "STRAWBERRY");
            this.AssertInputDonut(donutsValue[1], 2, "\"Donut2\"", "VANILLA");

            var bagelsValue = firstItem
                .Children.OfType<IInputObjectFieldDocumentPart>().Single(x => x.Name == "bagels")
                .Children.OfType<IListSuppliedValueDocumentPart>().Single()
                .Children.OfType<IComplexSuppliedValueDocumentPart>()
                .ToList();

            this.AssertInputBagel(bagelsValue[0], 3, "\"Bagel3\"", true, 3);
            Assert.AreEqual("4", bagelsValue[1]
                .Children.OfType<IInputObjectFieldDocumentPart>().Single(x => x.Name == "id")
                .Children.OfType<IScalarSuppliedValue>().Single().Value.ToString());
            Assert.AreEqual("\"Bagel4\"", bagelsValue[1]
                .Children.OfType<IInputObjectFieldDocumentPart>().Single(x => x.Name == "name")
                .Children.OfType<IScalarSuppliedValue>().Single().Value.ToString());
            Assert.AreEqual("false", bagelsValue[1]
                .Children.OfType<IInputObjectFieldDocumentPart>().Single(x => x.Name == "isHot")
                .Children.OfType<IScalarSuppliedValue>().Single().Value.ToString());

            var bagel4VariableRef = bagelsValue[1]
                .Children.OfType<IInputObjectFieldDocumentPart>().Single(x => x.Name == "orderCreated")
                .Children.OfType<IVariableUsageDocumentPart>().Single();
            Assert.AreEqual("orderCount", bagel4VariableRef.VariableName.ToString());

            var singleDonut = firstItem
                .Children.OfType<IInputObjectFieldDocumentPart>().Single(x => x.Name == "singleDonut")
                .Children.OfType<IComplexSuppliedValueDocumentPart>().Single();
            var singleBagel = firstItem
                .Children.OfType<IInputObjectFieldDocumentPart>().Single(x => x.Name == "singleBagel")
                .Children.OfType<IComplexSuppliedValueDocumentPart>().Single();

            this.AssertInputDonut(singleDonut, 5, "\"Donut5\"", "CHOCOLATE");
            this.AssertInputBagel(singleBagel, 6, "\"Bagel6\"", true, 19);

            // Validate second array entry
            singleBagel = secondItem
                .Children.OfType<IInputObjectFieldDocumentPart>().Single(x => x.Name == "singleBagel")
                .Children.OfType<IComplexSuppliedValueDocumentPart>().Single();

            this.AssertInputBagel(singleBagel, 12, "\"Bagel12\"", true, 45);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void NestedListArgumentContentCheck()
        {
            var text = @"
                query MyQuery {
                    nestedList(items:[[[1,2],[3,4]],[[5,6],[7,8]]] ){}
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var jsonText = tree.ToJsonString(sourceText);

            var result = this.CreateDocument(tree, sourceText);

            var nestedListField = result.Operations[0]
                .FieldSelectionSet.ExecutableFields[0];

            var itemsArg = nestedListField
                .Children.OfType<IInputArgumentDocumentPart>().Single(x => x.Name == "items");
            var outerlist = itemsArg
                .Children.OfType<IListSuppliedValueDocumentPart>().Single();

            Assert.IsNotNull(outerlist);
            Assert.AreEqual(2, outerlist.Children.Count());

            var middleList0 = outerlist.Children.OfType<IListSuppliedValueDocumentPart>().First();
            var middleList1 = outerlist.Children.OfType<IListSuppliedValueDocumentPart>().Skip(1).First();

            Assert.AreEqual(2, middleList0.Children.Count());
            Assert.AreEqual(2, middleList1.Children.Count());

            var innerList0 = middleList0.Children.OfType<IListSuppliedValueDocumentPart>().First();
            var innerList1 = middleList0.Children.OfType<IListSuppliedValueDocumentPart>().Skip(1).First();
            var innerList2 = middleList1.Children.OfType<IListSuppliedValueDocumentPart>().First();
            var innerList3 = middleList1.Children.OfType<IListSuppliedValueDocumentPart>().Skip(1).First();

            Assert.AreEqual(2, innerList0.Children.Count);
            Assert.AreEqual("1", innerList0.Children.OfType<IScalarSuppliedValue>().First().Value.ToString());
            Assert.AreEqual("2", innerList0.Children.OfType<IScalarSuppliedValue>().Skip(1).First().Value.ToString());

            Assert.AreEqual(2, innerList1.Children.Count);
            Assert.AreEqual("3", innerList1.Children.OfType<IScalarSuppliedValue>().First().Value.ToString());
            Assert.AreEqual("4", innerList1.Children.OfType<IScalarSuppliedValue>().Skip(1).First().Value.ToString());

            Assert.AreEqual(2, innerList2.Children.Count);
            Assert.AreEqual("5", innerList2.Children.OfType<IScalarSuppliedValue>().First().Value.ToString());
            Assert.AreEqual("6", innerList2.Children.OfType<IScalarSuppliedValue>().Skip(1).First().Value.ToString());

            Assert.AreEqual(2, innerList3.Children.Count);
            Assert.AreEqual("7", innerList3.Children.OfType<IScalarSuppliedValue>().First().Value.ToString());
            Assert.AreEqual("8", innerList3.Children.OfType<IScalarSuppliedValue>().Skip(1).First().Value.ToString());

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void MultiVariableDeclarationWithDefaultValueContentsCheck()
        {
            var text = @"
                query MyQuery($var1: Int! = 13, $var2: [String!]) {
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Messages.Count);
            Assert.AreEqual(1, result.Operations.Count);

            var operation = result.Operations[0];

            Assert.AreEqual("MyQuery", operation.Name);

            var variables = operation.Children.OfType<IVariableDocumentPart>().ToList();
            Assert.AreEqual(2, variables.Count);

            var var1 = variables[0];
            Assert.AreEqual("var1", var1.Name);
            Assert.AreEqual("Int!", var1.TypeExpression.ToString());
            Assert.AreEqual(Constants.ScalarNames.INT, var1.GraphType.Name);

            Assert.AreEqual(1, var1.Children.Count);

            var value = var1.Children.First() as DocumentScalarSuppliedValue;
            Assert.IsNotNull(value);
            Assert.AreEqual("13", value.Value.ToString());
            Assert.AreEqual(ScalarValueType.Number, (ScalarValueType)value.ValueType);
            Assert.AreEqual(Constants.ScalarNames.INT, value.GraphType.Name);

            var var2 = variables[1];
            Assert.AreEqual("var2", var2.Name);
            Assert.AreEqual("[String!]", var2.TypeExpression.ToString());
            Assert.AreEqual(Constants.ScalarNames.STRING, var2.GraphType.Name);

            // no default value on var2
            Assert.AreEqual(0, var2.Children.Count());

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void InlineFragmentWithTypeConditionContentCheck()
        {
            var text = @"
                query MyQuery {
                    inlineFragTest(id:3){
                        ... on Bagel {
                            id name
                        }
                    }
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            var inlineFragment = result.Operations[0].FieldSelectionSet.ExecutableFields[0]
                .FieldSelectionSet.Children.OfType<IInlineFragmentDocumentPart>().Single();

            Assert.AreEqual(_bagelGraphType, inlineFragment.GraphType);
            Assert.AreEqual("Bagel", inlineFragment.TargetGraphTypeName.ToString());
            Assert.AreEqual(2, inlineFragment
                .Children.OfType<IFieldSelectionSetDocumentPart>().Single()
                .Children.Count());

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void InlineFragmentWithNoTypeConditionContentCheck()
        {
            var text = @"
                query MyQuery {
                    inlineFragTest(id:3){
                        ... {
                            id
                            name
                        }
                    }
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            var inlineFragment = result.Operations[0].FieldSelectionSet.ExecutableFields[0]
                .FieldSelectionSet.Children.OfType<IInlineFragmentDocumentPart>().Single();

            Assert.AreEqual(_donutGraphType, inlineFragment.GraphType);
            Assert.AreEqual(string.Empty, inlineFragment.TargetGraphTypeName.ToString());
            Assert.AreEqual(2, inlineFragment
                .Children.OfType<IFieldSelectionSetDocumentPart>().Single()
                .Children.Count());

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void NamedFragmentContentCheck()
        {
            var text = @"
                fragment myFrag on Bagel {
                    id
                    name
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            var namedFrag = result.NamedFragments[0];

            Assert.AreEqual(_bagelGraphType, namedFrag.GraphType);
            Assert.AreEqual("myFrag", namedFrag.Name.ToString());
            Assert.AreEqual("Bagel", namedFrag.TargetGraphTypeName.ToString());
            Assert.AreEqual(2, namedFrag
                .Children.OfType<IFieldSelectionSetDocumentPart>().Single()
                .Children.Count());

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void FragmentSpreadContentCheck()
        {
            var text = @"
                query MyQuery {
                    inlineFragTest(id:3){
                        ... myFrag
                    }
                }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            var fragSpread = result.Operations[0].FieldSelectionSet.ExecutableFields[0]
                .FieldSelectionSet.Children.OfType<IFragmentSpreadDocumentPart>().Single();

            Assert.AreEqual(null, fragSpread.GraphType); // graphtype not set yet
            Assert.IsNull(fragSpread.Fragment); // fragment not yet set
            Assert.AreEqual("myFrag", fragSpread.FragmentName.ToString());

            SynTreeOperations.Release(ref tree);
        }

        [TestCase("query MyQuery @directive1 @directive2 {}", DirectiveLocation.QUERY)]
        [TestCase("query @directive1 @directive2 {}", DirectiveLocation.QUERY)]
        [TestCase("mutation MyQuery @directive1 @directive2 {}", DirectiveLocation.MUTATION)]
        [TestCase("mutation @directive1 @directive2 {}", DirectiveLocation.MUTATION)]
        [TestCase("subscription MyQuery @directive1 @directive2 {}", DirectiveLocation.SUBSCRIPTION)]
        [TestCase("subscription @directive1 @directive2 {}", DirectiveLocation.SUBSCRIPTION)]
        public void DirectiveOnOperationCheck(string text, DirectiveLocation expectedLocation)
        {
            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            var directives = result.Operations[0]
                .Children.OfType<IDirectiveDocumentPart>().ToList();

            Assert.AreEqual(2, directives.Count);

            var directive1 = directives[0];
            var directive2 = directives[1];

            Assert.AreEqual("directive1", directive1.DirectiveName);
            Assert.AreEqual(expectedLocation, directive1.Location);
            Assert.AreEqual(_directive1, directive1.GraphType);

            Assert.AreEqual("directive2", directive2.DirectiveName);
            Assert.AreEqual(expectedLocation, directive2.Location);
            Assert.IsNull(directive2.GraphType);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void DirectiveOnNamedFragmentCheck()
        {
            var text = "fragment myData on Bagel @directive1 @directive2 {}";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            var directives = result.NamedFragments[0]
                .Children.OfType<IDirectiveDocumentPart>().ToList();

            Assert.AreEqual(2, directives.Count);

            var directive1 = directives[0];
            var directive2 = directives[1];

            Assert.AreEqual("directive1", directive1.DirectiveName);
            Assert.AreEqual(DirectiveLocation.FRAGMENT_DEFINITION, directive1.Location);
            Assert.AreEqual(_directive1, directive1.GraphType);

            Assert.AreEqual("directive2", directive2.DirectiveName);
            Assert.AreEqual(DirectiveLocation.FRAGMENT_DEFINITION, directive2.Location);
            Assert.IsNull(directive2.GraphType);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void DirectiveOnInlineFragmentCheck()
        {
            var text = @"
                    query {
                        inlineFragTest(id:1) {
                            ... @directive1 @directive2 {
                                id
                            }
                        }
                    }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            var inlineFrag = result.Operations[0].FieldSelectionSet
                .Children.OfType<IFieldDocumentPart>().Single().FieldSelectionSet
                .Children.OfType<IInlineFragmentDocumentPart>().Single();

            var directives = inlineFrag.Children.OfType<IDirectiveDocumentPart>().ToList();

            Assert.AreEqual(2, directives.Count);

            var directive1 = directives[0];
            var directive2 = directives[1];

            Assert.AreEqual("directive1", directive1.DirectiveName);
            Assert.AreEqual(DirectiveLocation.INLINE_FRAGMENT, directive1.Location);
            Assert.AreEqual(_directive1, directive1.GraphType);

            Assert.AreEqual("directive2", directive2.DirectiveName);
            Assert.AreEqual(DirectiveLocation.INLINE_FRAGMENT, directive2.Location);
            Assert.IsNull(directive2.GraphType);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void DirectiveOnFragmentSpreadCheck()
        {
            var text = @"
                    query {
                        inlineFragTest(id:1) {
                            ... myFragment @directive1 @directive2
                        }
                    }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            var fragSpread = result.Operations[0].FieldSelectionSet
                .Children.OfType<IFieldDocumentPart>().Single().FieldSelectionSet
                .Children.OfType<IFragmentSpreadDocumentPart>().Single();

            Assert.AreEqual("myFragment", fragSpread.FragmentName.ToString());
            var directives = fragSpread.Children.OfType<IDirectiveDocumentPart>().ToList();

            Assert.AreEqual(2, directives.Count);

            var directive1 = directives[0];
            var directive2 = directives[1];

            Assert.AreEqual("directive1", directive1.DirectiveName);
            Assert.AreEqual(DirectiveLocation.FRAGMENT_SPREAD, directive1.Location);
            Assert.AreEqual(_directive1, directive1.GraphType);

            Assert.AreEqual("directive2", directive2.DirectiveName);
            Assert.AreEqual(DirectiveLocation.FRAGMENT_SPREAD, directive2.Location);
            Assert.IsNull(directive2.GraphType);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void DirectiveOnFieldCheck()
        {
            var text = @"
                    query {
                        inlineFragTest(id:1) @directive1 @directive2 {
                            id name
                        }
                    }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            var field = result.Operations[0].FieldSelectionSet
                .Children.OfType<IFieldDocumentPart>().Single();

            Assert.AreEqual("inlineFragTest", field.Name.ToString());
            var directives = field.Children.OfType<IDirectiveDocumentPart>().ToList();

            Assert.AreEqual(2, directives.Count);

            var directive1 = directives[0];
            var directive2 = directives[1];

            Assert.AreEqual("directive1", directive1.DirectiveName);
            Assert.AreEqual(DirectiveLocation.FIELD, directive1.Location);
            Assert.AreEqual(_directive1, directive1.GraphType);

            Assert.AreEqual("directive2", directive2.DirectiveName);
            Assert.AreEqual(DirectiveLocation.FIELD, directive2.Location);
            Assert.IsNull(directive2.GraphType);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void DirectiveWithArgumentsCheck()
        {
            var text = @"
                    query {
                        inlineFragTest(id:1) @directive3(arg1: 5, arg2: ""bob"") {
                            id name
                        }
                    }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            var field = result.Operations[0].FieldSelectionSet
                .Children.OfType<IFieldDocumentPart>().Single();

            Assert.AreEqual("inlineFragTest", field.Name.ToString());
            var directives = field.Children.OfType<IDirectiveDocumentPart>().ToList();

            Assert.AreEqual(1, directives.Count);

            var directive3 = directives[0];

            Assert.AreEqual("directive3", directive3.DirectiveName);
            Assert.AreEqual(DirectiveLocation.FIELD, directive3.Location);
            Assert.AreEqual(_directive3, directive3.GraphType);

            var args = directive3.Children.OfType<IInputArgumentDocumentPart>().ToList();
            Assert.AreEqual(2, args.Count);

            var arg1 = args[0];
            var arg2 = args[1];

            Assert.AreEqual("arg1", arg1.Name.ToString());
            Assert.AreEqual("5", ((IScalarSuppliedValue)arg1.Value).Value.ToString());

            Assert.AreEqual("arg2", arg2.Name.ToString());
            Assert.AreEqual("\"bob\"", ((IScalarSuppliedValue)arg2.Value).Value.ToString());

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void UnknownArgumentHasNullValues()
        {
            var text = @"
                    query {
                        inlineFragTest(notAnArgument:1){
                            id name
                        }
                    }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            var field = result.Operations[0].FieldSelectionSet
                .Children.OfType<IFieldDocumentPart>().Single();

            Assert.AreEqual("inlineFragTest", field.Name.ToString());
            var arg = field.Children.OfType<IInputArgumentDocumentPart>().Single();

            Assert.IsNull(arg.GraphType);
            Assert.IsNull(arg.Argument);
            Assert.AreEqual("notAnArgument", arg.Name);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void TypeNameFieldIsAddedCorrectly()
        {
            var text = @"
                    query {
                        multiScalarArguments(id:1, count: 3){
                            id
                            __typename
                        }
                    }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            var fields = result.Operations[0]
                .FieldSelectionSet.Children.OfType<IFieldDocumentPart>().Single() // multiScalarArguments
                .FieldSelectionSet.Children.OfType<IFieldDocumentPart>().ToList();

            Assert.AreEqual(2, fields.Count);

            var id = fields[0];
            var typeName = fields[1];

            Assert.AreEqual("id", id.Name.ToString());

            Assert.AreEqual("__typename", typeName.Name.ToString());
            Assert.IsNotNull(typeName.GraphType);
            Assert.AreEqual(Constants.ScalarNames.STRING, typeName.GraphType.Name);

            SynTreeOperations.Release(ref tree);
        }

        [Test]
        public void TypeNameFieldOnAUnionAddsFieldsForEachUnionMember()
        {
            var text = @"
                    query {
                        retrieveEither(){
                            ... on Donut {
                                id
                            }
                            ... on Bagel {
                                id
                            }
                            __typename
                        }
                    }";

            var sourceText = new SourceText(text.AsSpan());
            var tree = this.CreateSyntaxTree(ref sourceText);

            var result = this.CreateDocument(tree, sourceText);
            var retrieveEither = result.Operations[0]
                .FieldSelectionSet.Children.OfType<IFieldDocumentPart>().Single(); // retrieveEither

            var fieldSelectionSet = retrieveEither.FieldSelectionSet;

            // should contain two inline fragments and two __typename fields (one for bagel one for donut)
            Assert.AreEqual(4, fieldSelectionSet.Children.Count);
            Assert.AreEqual(2, fieldSelectionSet.Children[DocumentPartType.InlineFragment].Count);
            Assert.AreEqual(2, fieldSelectionSet.Children[DocumentPartType.Field].Count);
            Assert.IsNotNull(fieldSelectionSet.Children[DocumentPartType.Field]
                .OfType<IFieldDocumentPart>().SingleOrDefault(x => x.Field.Parent == _donutGraphType && x.Field.Name == "__typename"));
            Assert.IsNotNull(fieldSelectionSet.Children[DocumentPartType.Field]
                .OfType<IFieldDocumentPart>().SingleOrDefault(x => x.Field.Parent == _bagelGraphType && x.Field.Name == "__typename"));

            SynTreeOperations.Release(ref tree);
        }
    }
}