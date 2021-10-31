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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A "maker" capable of producing a qualified <see cref="IObjectGraphType"/> from its related template.
    /// </summary>
    public class ObjectGraphTypeMaker : IGraphTypeMaker
    {
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectGraphTypeMaker"/> class.
        /// </summary>
        /// <param name="schema">The schema defining the graph type creation rules this generator should follow.</param>
        public ObjectGraphTypeMaker(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <summary>
        /// Inspects the given type and, in accordance with the rules of this maker, will
        /// generate a complete set of necessary graph types required to support it.
        /// </summary>
        /// <param name="concreteType">The concrete type to incorporate into the schema.</param>
        /// <returns>GraphTypeCreationResult.</returns>
        public virtual GraphTypeCreationResult CreateGraphType(Type concreteType)
        {
            if (concreteType == null)
                return null;

            var template = GraphQLProviders.TemplateProvider.ParseType(concreteType, TypeKind.OBJECT) as IObjectGraphTypeTemplate;
            if (template == null)
                return null;

            var result = new GraphTypeCreationResult();
            var formatter = _schema.Configuration.DeclarationOptions.GraphNamingFormatter;

            var fieldSet = new List<IGraphField>();
            var fieldMaker = GraphQLProviders.GraphTypeMakerProvider.CreateFieldMaker(_schema);
            foreach (var fieldTemplate in ObjectGraphTypeMaker.GatherFieldTemplates(template, _schema))
            {
                var fieldResult = fieldMaker.CreateField(fieldTemplate);
                fieldSet.Add(fieldResult.Field);

                result.MergeDependents(fieldResult);
            }

            var objectType = new ObjectGraphType(
                formatter.FormatGraphTypeName(template.Name),
                concreteType,
                fieldSet)
            {
                Description = template.Description,
                Publish = template.Publish,
            };

            // add in declared interfaces by name
            foreach (var iface in template.DeclaredInterfaces)
            {
                objectType.InterfaceNames.Add(formatter.FormatGraphTypeName(GraphTypeNames.ParseName(iface, TypeKind.OBJECT)));
            }

            result.GraphType = objectType;
            result.ConcreteType = concreteType;
            return result;
        }

        /// <summary>
        /// Creates the collection of graph fields that belong to the template.
        /// </summary>
        /// <param name="template">The template to generate fields for.</param>
        /// <param name="schema">The schema.</param>
        /// <returns>IEnumerable&lt;IGraphField&gt;.</returns>
        internal static IEnumerable<IGraphTypeFieldTemplate> GatherFieldTemplates(IGraphTypeFieldTemplateContainer template, ISchema schema)
        {
            // gather the fields to include in the graph type
            var requiredDeclarations = template.DeclarationRequirements ?? schema.Configuration.DeclarationOptions.FieldDeclarationRequirements;

            return template.FieldTemplates.Values.Where(x =>
            {
                if (x.IsExplicitDeclaration)
                    return true;

                switch (x.FieldSource)
                {
                    case GraphFieldSource.Method:
                        return requiredDeclarations.AllowImplicitMethods();

                    case GraphFieldSource.Property:
                        return requiredDeclarations.AllowImplicitProperties();
                }

                return false;
            });
        }
    }
}