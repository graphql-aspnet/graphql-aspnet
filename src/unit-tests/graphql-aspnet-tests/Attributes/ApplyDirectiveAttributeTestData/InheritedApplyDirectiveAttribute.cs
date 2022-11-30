// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Attributes.ApplyDirectiveAttributeTestData
{
    using System;
    using GraphQL.AspNet.Attributes;

    public class InheritedApplyDirectiveAttribute : ApplyDirectiveAttribute
    {
        public InheritedApplyDirectiveAttribute(Type directiveType, params object[] arguments)
            : base(directiveType, arguments)
        {
        }
    }
}