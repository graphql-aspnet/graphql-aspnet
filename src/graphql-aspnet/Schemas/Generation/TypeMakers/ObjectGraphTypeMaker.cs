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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Engine.TypeMakers;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A "maker" capable of producing a qualified <see cref="IObjectGraphType"/> from its related template.
    /// </summary>
    public class ObjectGraphTypeMaker : FieldContainerGraphTypeMakerBase, IGraphTypeMaker
    {
        private readonly ISchemaConfiguration _config;
        private readonly IGraphFieldMaker _fieldMaker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectGraphTypeMaker" /> class.
        /// </summary>
        /// <param name="config">The schema configuration and setup options to use
        /// when building out the graph type.</param>
        /// <param name="fieldMaker">The field maker instnace used to create fields
        /// on any created graph types.</param>
        public ObjectGraphTypeMaker(
            ISchemaConfiguration config,
            IGraphFieldMaker fieldMaker)
            : base(config)
        {
            _config = Validation.ThrowIfNullOrReturn(config, nameof(config));
            _fieldMaker = Validation.ThrowIfNullOrReturn(fieldMaker, nameof(fieldMaker));
        }

        /// <inheritdoc />
        public virtual GraphTypeCreationResult CreateGraphType(IGraphTypeTemplate typeTemplate)
        {
            if (!(typeTemplate is IObjectGraphTypeTemplate template))
                return null;

            var result = new GraphTypeCreationResult();

            var formatter = _config.DeclarationOptions.GraphNamingFormatter;
            var directives = template.CreateAppliedDirectives();

            var objectType = new ObjectGraphType(
                formatter.FormatGraphTypeName(template.Name),
                template.ObjectType,
                template.Route,
                directives)
            {
                Description = template.Description,
                Publish = template.Publish,
            };

            result.GraphType = objectType;
            result.ConcreteType = template.ObjectType;

            // account for any potential type system directives
            result.AddDependentRange(template.RetrieveRequiredTypes());

            foreach (var fieldTemplate in this.GatherFieldTemplates(template))
            {
                var fieldResult = _fieldMaker.CreateField(fieldTemplate);
                objectType.Extend(fieldResult.Field);
                result.MergeDependents(fieldResult);
            }

            // add in declared interfaces by name
            foreach (var iface in template.DeclaredInterfaces)
            {
                objectType.InterfaceNames.Add(formatter.FormatGraphTypeName(GraphTypeNames.ParseName(iface, TypeKind.INTERFACE)));
            }

            return result;
        }
    }
}