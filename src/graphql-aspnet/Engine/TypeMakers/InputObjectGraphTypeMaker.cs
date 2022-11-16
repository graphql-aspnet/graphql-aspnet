﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine.TypeMakers
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
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

            // gather directives
            var directives = template.CreateAppliedDirectives();

            var inputObjectType = new InputObjectGraphType(
                formatter.FormatGraphTypeName(template.Name),
                concreteType,
                template.Route,
                directives)
            {
                Description = template.Description,
                Publish = template.Publish,
            };

            // account for any potential type system directives
            result.AddDependentRange(template.RetrieveRequiredTypes());

            // gather the fields to include in the graph type
            var requiredDeclarations = template.DeclarationRequirements
                ?? _schema.Configuration.DeclarationOptions.FieldDeclarationRequirements;
            var fieldTemplates = template.FieldTemplates.Values.Where(x =>
                x.IsExplicitDeclaration || requiredDeclarations.AllowImplicitProperties());

            // create the fields for the graph type
            var fieldMaker = GraphQLProviders.GraphTypeMakerProvider.CreateFieldMaker(_schema);
            foreach (var fieldTemplate in fieldTemplates)
            {
                var fieldResult = fieldMaker.CreateField(fieldTemplate);
                inputObjectType.AddField(fieldResult.Field);

                result.MergeDependents(fieldResult);
            }

            result.GraphType = inputObjectType;
            result.ConcreteType = concreteType;
            return result;
        }
    }
}