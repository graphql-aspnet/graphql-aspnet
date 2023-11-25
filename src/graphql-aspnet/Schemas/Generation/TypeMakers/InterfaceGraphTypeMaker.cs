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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A "maker" capable of producing a qualified <see cref="IInterfaceGraphType"/> from its related template.
    /// </summary>
    public class InterfaceGraphTypeMaker : FieldContainerGraphTypeMakerBase, IGraphTypeMaker
    {
        private readonly ISchemaConfiguration _config;
        private readonly IGraphFieldMaker _fieldMaker;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceGraphTypeMaker" /> class.
        /// </summary>
        /// <param name="config">The configuration data to use when generating any graph types.</param>
        /// <param name="fieldMaker">The field maker to use when generating fields on any
        /// created interface types.</param>
        public InterfaceGraphTypeMaker(
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
            if (!(typeTemplate is IInterfaceGraphTypeTemplate template))
                return null;

            template.Parse();
            template.ValidateOrThrow(false);

            var result = new GraphTypeCreationResult();

            var directives = template.CreateAppliedDirectives();

            var interfaceType = new InterfaceGraphType(
                template.Name,
                template.InternalName,
                template.ObjectType,
                template.Route,
                directives)
            {
                Description = template.Description,
                Publish = template.Publish,
            };

            // add in declared interfaces by name
            foreach (var iface in template.DeclaredInterfaces)
                interfaceType.InterfaceNames.Add(GraphTypeNames.ParseName(iface, TypeKind.INTERFACE));

            interfaceType = _config
                .DeclarationOptions?
                .SchemaFormatStrategy?
                .ApplyFormatting(_config, interfaceType) ?? interfaceType;

            // account for any potential type system directives
            result.AddDependentRange(template.RetrieveRequiredTypes());

            foreach (var fieldTemplate in this.GatherFieldTemplates(template))
            {
                var fieldResult = _fieldMaker.CreateField(fieldTemplate);
                interfaceType.Extend(fieldResult.Field);

                result.MergeDependents(fieldResult);
            }

            // at least one field should have been rendered
            // the type is invalid if there are no fields othe than __typename
            if (interfaceType.Fields.Count == 1)
            {
                throw new GraphTypeDeclarationException(
                  $"The interface graph type '{template.ObjectType.FriendlyName()}' defines 0 fields. " +
                  $"All interface types must define at least one field.",
                  template.ObjectType);
            }

            result.GraphType = interfaceType;
            result.ConcreteType = template.ObjectType;
            return result;
        }
    }
}