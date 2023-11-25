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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution;
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
                    _instance = GlobalTypes.CreateUnionProxyFromType(_proxyType);
                    if (_instance != null)
                    {
                        this.ItemPath = new ItemPath(ItemPath.Join(ItemPathRoots.Types, _instance.Name));
                        this.InternalName = _instance.InternalName;
                    }

                    this.InternalName = this.InternalName ?? _proxyType.FriendlyName();
                }
            }
            catch
            {
            }

            if (string.IsNullOrWhiteSpace(this.InternalName))
            {
                // ad-hoc unions will be a flat instance of GraphUnionProxy, not a differentiated instance
                // BUT internally it will always be guarunteed that the flat instance is instantiable
                // and that a name will be defined so this "should" never run....best laid plans though, am i rite?
                this.InternalName = _proxyType?.FriendlyName();
            }
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(bool validateChildren = true)
        {
            base.ValidateOrThrow(validateChildren);
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
        public Type ProxyType => _proxyType;

        /// <inheritdoc />
        public override bool Publish => _instance?.Publish ?? false;
    }
}