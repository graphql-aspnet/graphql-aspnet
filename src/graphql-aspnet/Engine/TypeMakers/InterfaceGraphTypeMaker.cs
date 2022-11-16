// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults.TypeMakers
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A "maker" capable of producing a qualified <see cref="IInterfaceGraphType"/> from its related template.
    /// </summary>
    public class InterfaceGraphTypeMaker : IGraphTypeMaker
    {
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceGraphTypeMaker"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public InterfaceGraphTypeMaker(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <inheritdoc />
        public GraphTypeCreationResult CreateGraphType(Type concreteType)
        {
            if (concreteType == null)
                return null;

            var template = GraphQLProviders.TemplateProvider.ParseType(concreteType, TypeKind.INTERFACE) as IInterfaceGraphTypeTemplate;
            if (template == null)
                return null;

            var result = new GraphTypeCreationResult();
            var formatter = _schema.Configuration.DeclarationOptions.GraphNamingFormatter;

            var directives = template.CreateAppliedDirectives();

            var interfaceType = new InterfaceGraphType(
                formatter.FormatGraphTypeName(template.Name),
                template.ObjectType,
                template.Route,
                directives)
            {
                Description = template.Description,
                Publish = template.Publish,
            };

            // account for any potential type system directives
            result.AddDependentRange(template.RetrieveRequiredTypes());

            var fieldMaker = GraphQLProviders.GraphTypeMakerProvider.CreateFieldMaker(_schema);
            foreach (var fieldTemplate in ObjectGraphTypeMaker.GatherFieldTemplates(template, _schema))
            {
                var fieldResult = fieldMaker.CreateField(fieldTemplate);
                interfaceType.Extend(fieldResult.Field);

                result.MergeDependents(fieldResult);
            }

            // add in declared interfaces by name
            foreach (var iface in template.DeclaredInterfaces)
            {
                interfaceType.InterfaceNames.Add(formatter.FormatGraphTypeName(GraphTypeNames.ParseName(iface, TypeKind.INTERFACE)));
            }

            result.GraphType = interfaceType;
            result.ConcreteType = concreteType;
            return result;
        }
    }
}