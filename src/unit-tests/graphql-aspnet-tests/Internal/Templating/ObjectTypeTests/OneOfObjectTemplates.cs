// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

#pragma warning disable SA1649
#pragma warning disable SA1402
namespace GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests
{
    using GraphQL.AspNet.Attributes;

    public struct MyStruct
    {
        public int? Prop0 { get; set; }
    }

    [OneOf]
    public class InvalidOneOfTemplateWithStructField
    {
        public MyStruct Prop2 { get; set; }
    }

    [OneOf]
    public class InvalidOneOfTemplateWithNonNullFieldType
    {
        public int Prop2 { get; set; }
    }

    [OneOf]
    public class InvalidOneOfTemplateNonNullTypeExpression
    {
        [GraphField(TypeExpression = "Type!")]
        public string Prop1 { get; set; }
    }

    [OneOf]
    public class InvalidOneOfTemplateDefaultValueOnNullableField
    {
        public InvalidOneOfTemplateDefaultValueOnNullableField()
        {
            this.Prop1 = 15;
        }

        public int? Prop1 { get; set; }
    }

    [OneOf]
    public class InvalidOneOfTemplateDefaultValueOnStringField
    {
        public InvalidOneOfTemplateDefaultValueOnStringField()
        {
            this.Prop1 = "some value";
        }

        public string Prop1 { get; set; }
    }

    [OneOf]
    public class InvalidOneOfTemplateWithNullableStructFieldAndDeclaredDefaultvalue
    {
        public InvalidOneOfTemplateWithNullableStructFieldAndDeclaredDefaultvalue()
        {
            this.Prop2 = new MyStruct
            {
                Prop0 = 1,
            };
        }

        public MyStruct? Prop2 { get; set; }
    }

    [OneOf]
    public class InvalidOneOfTemplateDefaultValueOnNonNullField
    {
        public InvalidOneOfTemplateDefaultValueOnNonNullField()
        {
            this.Prop1 = 15;
        }

        public int Prop1 { get; set; }
    }

    [OneOf]
    public class ValidOneOfObjectWithNullableIntProp
    {
        public int? Prop2 { get; set; }
    }

    [OneOf]
    public class ValidOneOfObjectWithStringProp
    {
        public string Prop1 { get; set; }
    }

    [OneOf]
    public class ValidOneOfTemplateWithNullableStructField
    {
        public MyStruct? Prop2 { get; set; }
    }

    public class SimpleObjectWithStringProp
    {
        public string Prop1 { get; set; }
    }
}