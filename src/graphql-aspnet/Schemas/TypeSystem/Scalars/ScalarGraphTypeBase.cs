// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.Scalars
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A base class defining a lot of common functionality used by all scalar types.
    /// </summary>
    public abstract class ScalarGraphTypeBase : IScalarGraphType, ILeafValueResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarGraphTypeBase" /> class.
        /// </summary>
        /// <param name="name">The name of the scalar as it appears in a schema.</param>
        /// <param name="primaryType">The primary datatype that represents this scalar.</param>
        /// <param name="directives">The directives to apply to this scalar
        /// when its added to a schema.</param>
        /// <param name="specifiedByUrl">An optional url pointing to the specification of this
        /// scalar type.</param>
        protected ScalarGraphTypeBase(
            string name,
            Type primaryType,
            IAppliedDirectiveCollection directives = null,
            string specifiedByUrl = null)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            this.Route = new SchemaItemPath(SchemaItemCollections.Scalars, this.Name);
            this.ObjectType = Validation.ThrowIfNullOrReturn(primaryType, nameof(primaryType));
            this.InternalFullName = this.ObjectType.FriendlyName();
            this.Publish = true;
            this.SourceResolver = this;
            this.AppliedDirectives = new AppliedDirectiveCollection(this);
            this.SpecifiedByUrl = specifiedByUrl?.Trim();

            // since scalars are special
            // we can't do any validation here on directives declared on the scalar
            // must rely on runtime validation in the execution pipeline

            // look for directives declared on the scalar class as attributes
            foreach (var appliedDirective in this.GetType().ExtractAppliedDirectives())
            {
                if (appliedDirective != null)
                    this.AppliedDirectives.Add(appliedDirective);
            }

            // add any directives directly applied in the constructor
            // usually from a scalar inheriting from this base type
            if (directives != null)
            {
                foreach (var directive in directives)
                    this.AppliedDirectives.Add(directive);
            }
        }

        /// <inheritdoc />
        public abstract object Resolve(ReadOnlySpan<char> data);

        /// <inheritdoc />
        public virtual bool ValidateObject(object item)
        {
            if (item == null)
                return true;

            var itemType = item.GetType();
            return itemType == this.ObjectType;
        }

        /// <inheritdoc />
        public virtual object Serialize(object item)
        {
            return item;
        }

        /// <inheritdoc />
        public virtual string SerializeToQueryLanguage(object item)
        {
            var serialized = this.Serialize(item);
            if (serialized == null)
                return "null";

            if (serialized.GetType() == typeof(string))
                return $"\"{serialized}\"";

            return serialized.ToString();
        }

        /// <inheritdoc />
        public virtual IScalarGraphType Clone(string newName)
        {
            newName = Validation.ThrowIfNullWhiteSpaceOrReturn(newName, nameof(newName));
            var newInstance = GlobalTypes.CreateScalarInstanceOrThrow(this.GetType()) as ScalarGraphTypeBase;

            // some built in scalars (defined against this class)
            // should never be renameable (string, int, float, id, boolean)
            if (GlobalTypes.CanBeRenamed(this.Name))
                newInstance.Name = newName;

            return newInstance;
        }

        /// <inheritdoc />
        public virtual string Name { get; protected set; }

        /// <inheritdoc />
        public virtual Type ObjectType { get; }

        /// <inheritdoc />
        public virtual string InternalFullName { get; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public TypeKind Kind => TypeKind.SCALAR;

        /// <inheritdoc />
        public bool Publish { get; set; }

        /// <inheritdoc />
        public abstract ScalarValueType ValueType { get; }

        /// <inheritdoc />
        public virtual ILeafValueResolver SourceResolver { get; set; }

        /// <inheritdoc />
        public virtual bool IsVirtual => false;

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public SchemaItemPath Route { get; }

        /// <inheritdoc />
        public string SpecifiedByUrl { get; set; }
    }
}