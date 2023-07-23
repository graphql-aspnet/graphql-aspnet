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

            var result = new GraphTypeCreationResult();
            var formatter = _config.DeclarationOptions.GraphNamingFormatter;

            var directives = template.CreateAppliedDirectives();

            var interfaceType = new InterfaceGraphType(
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

            foreach (var fieldTemplate in this.GatherFieldTemplates(template))
            {
                var fieldResult = _fieldMaker.CreateField(fieldTemplate);
                interfaceType.Extend(fieldResult.Field);

                result.MergeDependents(fieldResult);
            }

            // add in declared interfaces by name
            foreach (var iface in template.DeclaredInterfaces)
            {
                interfaceType.InterfaceNames.Add(formatter.FormatGraphTypeName(GraphTypeNames.ParseName(iface, TypeKind.INTERFACE)));
            }

            result.GraphType = interfaceType;
            result.ConcreteType = template.ObjectType;
            return result;
        }
    }
}