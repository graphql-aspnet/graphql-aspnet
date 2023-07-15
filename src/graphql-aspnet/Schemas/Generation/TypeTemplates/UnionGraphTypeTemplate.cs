// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeTemplates
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// An graph type template describing a SCALAR graph type.
    /// </summary>
    public class UnionGraphTypeTemplate : GraphTypeTemplateBase, IUnionGraphTypeTemplate
    {
        private readonly Type _proxyType;
        private IGraphUnionProxy _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionGraphTypeTemplate" /> class.
        /// </summary>
        /// <param name="proxyType">The type that implements <see cref="IGraphUnionProxy"/>.</param>
        public UnionGraphTypeTemplate(Type proxyType)
            : base(proxyType)
        {
            _proxyType = proxyType;
            this.ObjectType = null;
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            try
            {
                if (_proxyType != null)
                {

                    _instance = InstanceFactory.CreateInstance(_proxyType) as IGraphUnionProxy;
                    if (_instance != null)
                    {
                        this.Route = new SchemaItemPath(SchemaItemPath.Join(SchemaItemCollections.Types, _instance.Name));
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
            GlobalTypes.ValidateUnionProxyOrThrow(_proxyType);
        }

        /// <inheritdoc />
        public override AppliedSecurityPolicyGroup SecurityPolicies => null;

        /// <inheritdoc />
        public override string Name => _instance?.Name;

        /// <inheritdoc />
        public override string Description => _instance?.Description;

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.UNION;

        /// <inheritdoc />
        public override string InternalFullName => _proxyType?.FriendlyName(true);

        /// <inheritdoc />
        public override string InternalName => _proxyType?.Name;

        /// <inheritdoc />
        public Type ProxyType => _proxyType;

        /// <inheritdoc />
        public override bool Publish => _instance?.Publish ?? false;
    }
}