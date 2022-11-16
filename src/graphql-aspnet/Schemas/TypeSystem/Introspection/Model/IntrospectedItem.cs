// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A base class for all introspected data model items.
    /// </summary>
    public abstract class IntrospectedItem : IIntrospectionSchemaItem
    {
        private readonly ISchemaItem _item;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedItem"/> class.
        /// </summary>
        /// <param name="item">The item being introspected.</param>
        public IntrospectedItem(ISchemaItem item)
        {
            _item = Validation.ThrowIfNullOrReturn(item, nameof(item));
            this.Name = _item.Name;
            this.Description = _item.Description;
            this.Route = _item.Route.ReParent(Constants.Routing.INTROSPECTION_ROOT);
            this.AppliedDirectives = new AppliedDirectiveCollection(this);
        }

        /// <summary>
        /// When overridden in a child class,populates this introspected type using its parent schema to fill in any details about
        /// other references in this instance.
        /// </summary>
        /// <param name="introspectedSchema">The schema from which this item is to be initialized.</param>
        public virtual void Initialize(IntrospectedSchema introspectedSchema)
        {
        }

        /// <inheritdoc />
        [GraphSkip]
        public virtual SchemaItemPath Route { get; }

        /// <inheritdoc />
        [GraphSkip]
        public virtual IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public virtual string Name { get; set; }

        /// <inheritdoc />
        public virtual string Description { get; set; }
    }
}