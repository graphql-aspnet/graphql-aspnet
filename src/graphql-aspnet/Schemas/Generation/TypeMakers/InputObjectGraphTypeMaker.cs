// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeMakers
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A "maker" capable of producing a qualified <see cref="IInputObjectGraphType"/> from its related template.
    /// </summary>
    public class InputObjectGraphTypeMaker : IGraphTypeMaker
    {
        private readonly ISchemaConfiguration _config;
        private readonly IGraphFieldMaker _fieldMaker;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputObjectGraphTypeMaker" /> class.
        /// </summary>
        /// <param name="config">The configuration to use when constructing the input graph type.</param>
        /// <param name="fieldMaker">The field maker used to create input field instances.</param>
        public InputObjectGraphTypeMaker(
            ISchemaConfiguration config,
            IGraphFieldMaker fieldMaker)
        {
            _config = Validation.ThrowIfNullOrReturn(config, nameof(config));
            _fieldMaker = Validation.ThrowIfNullOrReturn(fieldMaker, nameof(fieldMaker));
        }

        /// <inheritdoc />
        public virtual GraphTypeCreationResult CreateGraphType(IGraphTypeTemplate typeTemplate)
        {
            if (!(typeTemplate is IInputObjectGraphTypeTemplate template))
                return null;

            var formatter = _config.DeclarationOptions.GraphNamingFormatter;
            var result = new GraphTypeCreationResult();

            // gather directives
            var directives = template.CreateAppliedDirectives();

            var inputObjectType = new InputObjectGraphType(
                formatter.FormatGraphTypeName(template.Name),
                template.InternalName,
                template.ObjectType,
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
                ?? _config.DeclarationOptions.FieldDeclarationRequirements;
            var fieldTemplates = template.FieldTemplates.Values.Where(x =>
                x.IsExplicitDeclaration || requiredDeclarations.AllowImplicitProperties());

            // create the fields for the graph type
            foreach (var fieldTemplate in fieldTemplates)
            {
                var fieldResult = _fieldMaker.CreateField(fieldTemplate);
                inputObjectType.AddField(fieldResult.Field);

                result.MergeDependents(fieldResult);
            }

            result.GraphType = inputObjectType;
            result.ConcreteType = template.ObjectType;
            return result;
        }
    }
}