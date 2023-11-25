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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// An graph type template describing a SCALAR graph type.
    /// </summary>
    public class ScalarGraphTypeTemplate : GraphTypeTemplateBase, IScalarGraphTypeTemplate
    {
        private readonly Type _scalarType;
        private IScalarGraphType _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarGraphTypeTemplate"/> class.
        /// </summary>
        /// <param name="typeToTemplate">The type to template.</param>
        public ScalarGraphTypeTemplate(Type typeToTemplate)
            : base(GlobalTypes.FindBuiltInScalarType(typeToTemplate) ?? typeToTemplate)
        {
            Validation.ThrowIfNull(typeToTemplate, nameof(typeToTemplate));
            _scalarType = GlobalTypes.FindBuiltInScalarType(typeToTemplate) ?? typeToTemplate;
            _instance = GlobalTypes.CreateScalarInstance(_scalarType);
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            if (_instance != null)
            {
                this.ItemPath = new ItemPath(ItemPath.Join(ItemPathRoots.Types, _instance.Name));
                this.ObjectType = _instance.ObjectType;
                this.InternalName = _instance.InternalName;
            }

            if (string.IsNullOrWhiteSpace(this.InternalName))
                this.InternalName = _scalarType?.FriendlyName();
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(bool validateChildren = true)
        {
            GlobalTypes.ValidateScalarTypeOrThrow(this.ScalarType);
            base.ValidateOrThrow(validateChildren);
        }

        /// <inheritdoc />
        protected override IEnumerable<IAppliedDirectiveTemplate> ParseAppliedDiretives()
        {
            if (_instance != null)
            {
                return _instance.AppliedDirectives.Select(x => new AppliedDirectiveTemplate(
                    this,
                    x.DirectiveName,
                    x.ArgumentValues)
                {
                    DirectiveType = x.DirectiveType,
                });
            }

            return Enumerable.Empty<IAppliedDirectiveTemplate>();
        }

        /// <inheritdoc />
        public override AppliedSecurityPolicyGroup SecurityPolicies { get; }

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.SCALAR;

        /// <inheritdoc />
        public override string Name => _instance?.Name;

        /// <inheritdoc />
        public override string Description
        {
            get
            {
                return _instance?.Description;
            }

            protected set
            {
                // description is fixed to that in the scalar instance.
            }
        }

        /// <inheritdoc />
        public Type ScalarType => _scalarType;
    }
}