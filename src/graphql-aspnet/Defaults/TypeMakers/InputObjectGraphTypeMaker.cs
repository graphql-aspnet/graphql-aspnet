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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A "maker" capable of producing a qualified <see cref="IInputObjectGraphType"/> from its related template.
    /// </summary>
    public class InputObjectGraphTypeMaker : IGraphTypeMaker
    {
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputObjectGraphTypeMaker"/> class.
        /// </summary>
        /// <param name="schema">The schema defining the graph type creation rules this generator should follow.</param>
        public InputObjectGraphTypeMaker(ISchema schema)
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
            var template = GraphQLProviders.TemplateProvider.ParseType(concreteType, TypeKind.INPUT_OBJECT) as IInputObjectGraphTypeTemplate;
            if (template == null)
                return null;

            var formatter = _schema.Configuration.DeclarationOptions.GraphNamingFormatter;
            var result = new GraphTypeCreationResult();

            var fieldSet = new List<IGraphField>();
            var fieldMaker = GraphQLProviders.GraphTypeMakerProvider.CreateFieldMaker(_schema);
            foreach (var fieldTemplate in ObjectGraphTypeMaker.GatherFieldTemplates(template, _schema))
            {
                var fieldResult = fieldMaker.CreateField(fieldTemplate);
                fieldSet.Add(fieldResult.Field);

                result.MergeDependents(fieldResult);
            }

            var inputObjectType = new InputObjectGraphType(
                formatter.FormatGraphTypeName(template.Name),
                concreteType,
                fieldSet)
            {
                Description = template.Description,
                Publish = template.Publish,
            };

            result.GraphType = inputObjectType;
            result.ConcreteType = concreteType;
            return result;
        }
    }
}