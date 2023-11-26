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
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Internal;

    /// <summary>
    /// A base class representing common functionality between all field templates based on
    /// C# methods.
    /// </summary>
    [DebuggerDisplay("Path: {ItemPath.Path}")]
    public abstract class MethodGraphFieldTemplateBase : GraphFieldTemplateBase
    {
        private readonly List<GraphArgumentTemplate> _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodGraphFieldTemplateBase" /> class.
        /// </summary>
        /// <param name="parent">The parent object template that owns this method.</param>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="attributeProvider">A custom, external attribute provider to use instead for extracting
        /// configuration attributes instead of the provider on <paramref name="methodInfo"/>.</param>
        protected MethodGraphFieldTemplateBase(IGraphTypeTemplate parent, MethodInfo methodInfo, ICustomAttributeProvider attributeProvider)
            : base(parent, attributeProvider)
        {
            this.Method = Validation.ThrowIfNullOrReturn(methodInfo, nameof(methodInfo));
            this.Parameters = this.Method.GetParameters().ToList();
            _arguments = new List<GraphArgumentTemplate>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodGraphFieldTemplateBase" /> class.
        /// </summary>
        /// <param name="parent">The parent object template that owns this method.</param>
        /// <param name="methodInfo">The method information.</param>
        protected MethodGraphFieldTemplateBase(IGraphTypeTemplate parent, MethodInfo methodInfo)
            : base(parent, methodInfo)
        {
            this.Method = Validation.ThrowIfNullOrReturn(methodInfo, nameof(methodInfo));
            this.Parameters = this.Method.GetParameters().ToList();
            _arguments = new List<GraphArgumentTemplate>();
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            // set the internal name of the item
            var fieldDeclaration = this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>();
            if (fieldDeclaration != null)
                this.InternalName = fieldDeclaration.InternalName;
            if (string.IsNullOrWhiteSpace(this.InternalName))
                this.InternalName = $"{this.Parent.InternalName}.{this.Method.Name}";

            // parse all input parameters from the method signature
            foreach (var parameter in this.Parameters)
            {
                var argTemplate = this.CreateArgument(parameter);
                argTemplate.Parse();
                _arguments.Add(argTemplate);
            }

            this.ExpectedReturnType = GraphValidation.EliminateWrappersFromCoreType(
                this.DeclaredReturnType,
                false,
                true,
                false);
        }

        /// <summary>
        /// Creates input argument for this template given the parameter info supplied.
        /// </summary>
        /// <param name="paramInfo">The parameter information.</param>
        /// <returns>IGraphFieldArgumentTemplate.</returns>
        protected virtual GraphArgumentTemplate CreateArgument(ParameterInfo paramInfo)
        {
            return new GraphArgumentTemplate(this, paramInfo);
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(bool validateChildren = true)
        {
            base.ValidateOrThrow(validateChildren);

            if (this.ExpectedReturnType == null)
            {
                throw new GraphTypeDeclarationException(
                   $"Invalid graph method declaration. The method '{this.InternalName}' has no valid {nameof(this.ExpectedReturnType)}. An expected " +
                   "return type must be assigned from the declared return type.");
            }
        }

        /// <inheritdoc />
        public override IGraphFieldResolver CreateResolver()
        {
            return new ObjectMethodGraphFieldResolver(this.CreateResolverMetaData());
        }

        /// <inheritdoc />
        public override IGraphFieldResolverMetaData CreateResolverMetaData()
        {
            var paramSet = new FieldResolverParameterMetaDataCollection(
                this.Arguments.Select(x => x.CreateResolverMetaData()));

            return new FieldResolverMetaData(
                this.Method,
                paramSet,
                this.ExpectedReturnType,
                this.IsAsyncField,
                this.InternalName,
                this.Method.Name,
                this.Parent.ObjectType,
                this.Parent.InternalName,
                this.Parent.TemplateSource);
        }

        /// <inheritdoc />
        public override IReadOnlyList<IGraphArgumentTemplate> Arguments => _arguments;

        /// <inheritdoc cref="IGraphFieldResolverMetaData.Method" />
        public MethodInfo Method { get; }

        /// <inheritdoc />
        public override Type DeclaredReturnType => this.Method.ReturnType;

        /// <inheritdoc cref="IGraphFieldResolverMetaData.ExpectedReturnType" />
        public Type ExpectedReturnType { get; protected set; }

        /// <inheritdoc />
        public override string DeclaredName => this.Method.Name;

        /// <inheritdoc />
        public override GraphFieldSource FieldSource => GraphFieldSource.Method;

        /// <summary>
        /// Gets the set of parameters found on the target <see cref="Method"/>.
        /// </summary>
        /// <value>The parameters.</value>
        public IReadOnlyList<ParameterInfo> Parameters { get; }
    }
}