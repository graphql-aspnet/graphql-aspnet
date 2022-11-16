// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Defaults.TypeMakers
{
    using System;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Defaults.TypeMakers;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;

    public abstract class GraphTypeMakerTestBase
    {
        protected GraphTypeCreationResult MakeGraphType(Type type, TypeKind kind, TemplateDeclarationRequirements? requirements = null)
        {
            var builder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames);
            if (requirements.HasValue)
            {
                builder.AddGraphQL(o =>
                {
                    o.DeclarationOptions.FieldDeclarationRequirements = requirements.Value;
                });
            }

            var typeMaker = new DefaultGraphTypeMakerProvider();
            var testServer = builder.Build();
            var maker = typeMaker.CreateTypeMaker(testServer.Schema, kind);
            return maker.CreateGraphType(type);
        }

        protected IGraphField MakeGraphField(IGraphFieldTemplate fieldTemplate)
        {
            var testServer = new TestServerBuilder().Build();
            var maker = new GraphFieldMaker(testServer.Schema);
            return maker.CreateField(fieldTemplate).Field;
        }
    }
}