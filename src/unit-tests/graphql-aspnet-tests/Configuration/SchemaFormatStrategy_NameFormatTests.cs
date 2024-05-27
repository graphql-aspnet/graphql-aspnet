// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Configuration
{
    using System;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Configuration.FormatStrategyTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaFormatStrategy_NameFormatTests
    {
        [TestCase("TYPE1", TextFormatOptions.UpperCase)]
        [TestCase("type1", TextFormatOptions.LowerCase)]
        [TestCase("Type1", TextFormatOptions.ProperCase)]
        public void EnumValues_FormatTests(
            string expectedValueName,
            TextFormatOptions enumValueFormat)
        {
            var strategy = SchemaFormatStrategyBuilder.Create()
                .WithEnumValueFormat(enumValueFormat)
                .Build();

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddType<WidgetType>();
                    o.DeclarationOptions.SchemaFormatStrategy = strategy;
                })
                .Build();

            var enumType = server.Schema.KnownTypes.FindGraphType(typeof(WidgetType)) as IEnumGraphType;
            var firstValue = enumType.Values.FindByEnumValue(WidgetType.Type1);

            Assert.AreEqual(expectedValueName, firstValue.Name);
        }

        [TestCase(typeof(Widget), TypeKind.OBJECT, TextFormatOptions.ProperCase, "Widget")]
        [TestCase(typeof(Widget), TypeKind.OBJECT, TextFormatOptions.UpperCase, "WIDGET")]
        [TestCase(typeof(Widget), TypeKind.OBJECT, TextFormatOptions.CamelCase, "widget")]
        [TestCase(typeof(Widget), TypeKind.OBJECT, TextFormatOptions.LowerCase, "widget")]
        [TestCase(typeof(Widget), TypeKind.INPUT_OBJECT, TextFormatOptions.ProperCase, "Input_Widget")]
        [TestCase(typeof(Widget), TypeKind.INPUT_OBJECT, TextFormatOptions.CamelCase, "input_Widget")]
        [TestCase(typeof(Widget), TypeKind.INPUT_OBJECT, TextFormatOptions.UpperCase, "INPUT_WIDGET")]
        [TestCase(typeof(Widget), TypeKind.INPUT_OBJECT, TextFormatOptions.LowerCase, "input_widget")]
        [TestCase(typeof(IWidget), TypeKind.OBJECT, TextFormatOptions.ProperCase, "IWidget")]
        [TestCase(typeof(IWidget), TypeKind.OBJECT, TextFormatOptions.CamelCase, "iWidget")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, TextFormatOptions.UpperCase, "IWIDGET")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, TextFormatOptions.LowerCase, "iwidget")]
        [TestCase(typeof(WidgetType), TypeKind.ENUM, TextFormatOptions.ProperCase, "WidgetType")]
        [TestCase(typeof(WidgetType), TypeKind.ENUM, TextFormatOptions.CamelCase, "widgetType")]
        [TestCase(typeof(WidgetType), TypeKind.ENUM, TextFormatOptions.UpperCase, "WIDGETTYPE")]
        [TestCase(typeof(WidgetType), TypeKind.ENUM, TextFormatOptions.LowerCase, "widgettype")]

        // directive name is uneffected by graph type name formats
        [TestCase(typeof(WidgetDirective), TypeKind.DIRECTIVE, TextFormatOptions.ProperCase, "widgetD")]
        [TestCase(typeof(WidgetDirective), TypeKind.DIRECTIVE, TextFormatOptions.UpperCase, "widgetD")]
        [TestCase(typeof(WidgetDirective), TypeKind.DIRECTIVE, TextFormatOptions.LowerCase, "widgetD")]
        public void GraphTypeNames_FormatTests(
            Type targetType,
            TypeKind typeKind,
            TextFormatOptions typeNameFormat,
            string expectedName)
        {
            var strategy = SchemaFormatStrategyBuilder.Create()
                .WithGraphTypeNameFormat(typeNameFormat)
                .Build();

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddType(targetType, typeKind);
                    o.DeclarationOptions.SchemaFormatStrategy = strategy;
                })
                .Build();

            var graphType = server.Schema.KnownTypes.FindGraphType(targetType);

            Assert.AreEqual(expectedName, graphType.Name);
        }

        [TestCase(typeof(Widget), TypeKind.OBJECT, TextFormatOptions.ProperCase, "IntProp")]
        [TestCase(typeof(Widget), TypeKind.OBJECT, TextFormatOptions.CamelCase, "intProp")]
        [TestCase(typeof(Widget), TypeKind.OBJECT, TextFormatOptions.UpperCase, "INTPROP")]
        [TestCase(typeof(Widget), TypeKind.OBJECT, TextFormatOptions.LowerCase, "intprop")]
        [TestCase(typeof(Widget), TypeKind.INPUT_OBJECT, TextFormatOptions.ProperCase, "IntProp")]
        [TestCase(typeof(Widget), TypeKind.INPUT_OBJECT, TextFormatOptions.CamelCase, "intProp")]
        [TestCase(typeof(Widget), TypeKind.INPUT_OBJECT, TextFormatOptions.UpperCase, "INTPROP")]
        [TestCase(typeof(Widget), TypeKind.INPUT_OBJECT, TextFormatOptions.LowerCase, "intprop")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, TextFormatOptions.ProperCase, "IntProp")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, TextFormatOptions.CamelCase, "intProp")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, TextFormatOptions.UpperCase, "INTPROP")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, TextFormatOptions.LowerCase, "intprop")]
        public void FieldNames_FormatTests(
        Type targetType,
        TypeKind typeKind,
        TextFormatOptions typeNameFormat,
        string expectedName)
        {
            var strategy = SchemaFormatStrategyBuilder.Create()
                .WithFieldNameFormat(typeNameFormat)
                .Build();

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddType(targetType, typeKind);
                    o.DeclarationOptions.SchemaFormatStrategy = strategy;
                })
                .Build();

            var graphType = server.Schema.KnownTypes.FindGraphType(targetType);

            switch (typeKind)
            {
                case TypeKind.INPUT_OBJECT:
                    Assert.IsNotNull(((IInputObjectGraphType)graphType).Fields.FindField(expectedName));
                    return;

                case TypeKind.OBJECT:
                case TypeKind.INTERFACE:
                    Assert.IsNotNull(((IGraphFieldContainer)graphType).Fields.FindField(expectedName));
                    return;
            }
        }

        // directive name is uneffected by field name formats
        [TestCase(typeof(WidgetDirective), TypeKind.DIRECTIVE, TextFormatOptions.ProperCase, "WidgetD")]
        [TestCase(typeof(WidgetDirective), TypeKind.DIRECTIVE, TextFormatOptions.CamelCase, "widgetD")]
        [TestCase(typeof(WidgetDirective), TypeKind.DIRECTIVE, TextFormatOptions.UpperCase, "WIDGETD")]
        [TestCase(typeof(WidgetDirective), TypeKind.DIRECTIVE, TextFormatOptions.LowerCase, "widgetd")]
        public void DirectiveNames_FormatTests(
        Type targetType,
        TypeKind typeKind,
        TextFormatOptions typeNameFormat,
        string expectedName)
        {
            var strategy = SchemaFormatStrategyBuilder.Create()
                .WithDirectiveNameFormat(typeNameFormat)
                .Build();

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddType(targetType, typeKind);
                    o.DeclarationOptions.SchemaFormatStrategy = strategy;
                })
                .Build();

            var directive = server.Schema.KnownTypes.FindGraphType(targetType) as IDirective;

            Assert.AreEqual(expectedName, directive.Name);
        }

        [TestCase(typeof(Widget), TypeKind.OBJECT, TextFormatOptions.ProperCase, "argItem", "Arg1")]
        [TestCase(typeof(Widget), TypeKind.OBJECT, TextFormatOptions.CamelCase, "argItem", "arg1")]
        [TestCase(typeof(Widget), TypeKind.OBJECT, TextFormatOptions.UpperCase, "argItem", "ARG1")]
        [TestCase(typeof(Widget), TypeKind.OBJECT, TextFormatOptions.LowerCase, "argItem", "arg1")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, TextFormatOptions.ProperCase, "argItem", "Arg1")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, TextFormatOptions.CamelCase, "argItem", "arg1")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, TextFormatOptions.UpperCase, "argItem", "ARG1")]
        [TestCase(typeof(IWidget), TypeKind.INTERFACE, TextFormatOptions.LowerCase, "argItem", "arg1")]
        public void ArgumentNames_FormatTests(
        Type targetType,
        TypeKind typeKind,
        TextFormatOptions typeNameFormat,
        string fieldName,
        string expectedArgName)
        {
            var strategy = SchemaFormatStrategyBuilder.Create()
                .WithFieldArgumentNameFormat(typeNameFormat)
                .Build();

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddType(targetType, typeKind);
                    o.DeclarationOptions.SchemaFormatStrategy = strategy;
                })
                .Build();

            var graphType = server.Schema.KnownTypes.FindGraphType(targetType) as IGraphFieldContainer;
            var field = graphType.Fields[fieldName];

            Assert.IsNotNull(field.Arguments.FindArgument(expectedArgName));
        }

        [TestCase(TextFormatOptions.ProperCase, "RetrieveRootWidget")]
        [TestCase(TextFormatOptions.CamelCase, "retrieveRootWidget")]
        [TestCase(TextFormatOptions.UpperCase, "RETRIEVEROOTWIDGET")]
        [TestCase(TextFormatOptions.LowerCase, "retrieverootwidget")]
        public void Controller_FieldNames_FormatTests(
            TextFormatOptions typeNameFormat,
            string expectedFieldName)
        {
            var strategy = SchemaFormatStrategyBuilder.Create()
                .WithFieldNameFormat(typeNameFormat)
                .Build();

            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddType<WidgetsController>();
                    o.DeclarationOptions.SchemaFormatStrategy = strategy;
                })
                .Build();

            var fieldSet = server.Schema.KnownTypes.FindGraphType("Query") as IGraphFieldContainer;

            Assert.IsNotNull(fieldSet.Fields.FindField(expectedFieldName));
        }
    }
}
