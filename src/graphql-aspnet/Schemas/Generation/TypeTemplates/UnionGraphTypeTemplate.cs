// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// An graph type template describing a SCALAR graph type.
    /// </summary>
    public class UnionGraphTypeTemplate : GraphTypeTemplateBase, IUnionGraphTypeTemplate
    {
        private readonly Type _proxyType;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionGraphTypeTemplate" /> class.
        /// </summary>
        /// <param name="proxyType">The type that implements <see cref="IGraphUnionProxy"/>.</param>
        public UnionGraphTypeTemplate(Type proxyType)
            : base(proxyType)
        {
            Validation.ThrowIfNull(proxyType, nameof(proxyType));
            _proxyType = proxyType;
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            try
            {
                if (_proxyType != null)
                {
                    var instance = InstanceFactory.CreateInstance(_proxyType) as IGraphUnionProxy;

                    if (instance != null)
                    {
                        this.Route = new SchemaItemPath(SchemaItemPath.Join(SchemaItemCollections.Types, instance.Name));
                        this.ObjectType = null;
                    }
                }
            }
            catch
            {
            }
        }

        /// <inheritdoc />
        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();
            if (_proxyType == null)
            {
                throw new GraphTypeDeclarationException(
                    "Unable to create a union graph type template from a null supplied proxy type.");
            }

            if (!Validation.IsCastable<IGraphUnionProxy>(_proxyType))
            {
                throw new GraphTypeDeclarationException(
                    $"The type {_proxyType.FriendlyGraphTypeName()} does not implement {nameof(IGraphUnionProxy)}. All " +
                    $"types being used as a declaration of a union must implement {nameof(IGraphUnionProxy)}.");
            }
        }

        /// <inheritdoc />
        public override AppliedSecurityPolicyGroup SecurityPolicies { get; }

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.SCALAR;

        /// <inheritdoc />
        public override string InternalFullName => _proxyType?.Name;

        /// <inheritdoc />
        public override string InternalName => _proxyType?.Name;

        /// <inheritdoc />
        public Type ProxyType => _proxyType;
    }
}