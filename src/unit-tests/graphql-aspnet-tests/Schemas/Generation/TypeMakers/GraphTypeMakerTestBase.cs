// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers
{
    using System;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;

    public abstract class GraphTypeMakerTestBase
    {
        protected GraphTypeCreationResult MakeGraphType(
            Type type,
            TypeKind kind,
            TemplateDeclarationRequirements? requirements = null)
        {
            var builder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames);
            if (requirements.HasValue)
            {
                builder.AddGraphQL(o =>
                {
                    o.DeclarationOptions.FieldDeclarationRequirements = requirements.Value;
                });
            }

            var testServer = builder.Build();

            var factory = testServer.CreateMakerFactory();

            var template = factory.MakeTemplate(type, kind);
            var maker = factory.CreateTypeMaker(type, kind);

            return maker.CreateGraphType(template);
        }

        protected IGraphField MakeGraphField(IGraphFieldTemplate fieldTemplate)
        {
            var testServer = new TestServerBuilder().Build();

            var factory = testServer.CreateMakerFactory();

            var maker = factory.CreateFieldMaker();

            return maker.CreateField(fieldTemplate).Field;
        }
    }
}