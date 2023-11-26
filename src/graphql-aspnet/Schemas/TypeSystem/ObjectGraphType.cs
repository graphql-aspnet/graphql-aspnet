// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Fields;

    /// <summary>
    /// A represention of a graphql object in the schema. This object defines all the exposed
    /// fields that can be "requested" when this type is queried.
    /// </summary>
    [DebuggerDisplay("OBJECT {Name} (Fields = {Fields.Count})")]
    public class ObjectGraphType : ObjectGraphTypeBase, IObjectGraphType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectGraphType" /> class.
        /// </summary>
        /// <param name="name">The name of the graph type.</param>
        /// <param name="internalName">The defined internal name for this graph type.</param>
        /// <param name="objectType">The concrete type that this graphtype is made from.</param>
        /// <param name="itemPath">The item path that identifies this object in the schema..</param>
        /// <param name="directives">The directives applied to this object
        /// when its added to a schema.</param>
        public ObjectGraphType(
            string name,
            string internalName,
            Type objectType,
            ItemPath itemPath,
            IAppliedDirectiveCollection directives = null)
            : base(name, internalName, itemPath, directives)
        {
            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));
            this.Extend(new Introspection_TypeNameMetaField(name));
        }

        /// <inheritdoc />
        public override IGraphType Clone(string typeName = null)
        {
            typeName = typeName?.Trim() ?? this.Name;
            var itemPath = this.ItemPath.Clone().Parent.CreateChild(typeName);

            var clonedItem = new ObjectGraphType(
                typeName,
                this.InternalName,
                this.ObjectType,
                itemPath,
                this.AppliedDirectives);

            clonedItem.Description = this.Description;
            clonedItem.Publish = this.Publish;

            foreach (var item in this.InterfaceNames)
                clonedItem.InterfaceNames.Add(item);

            foreach (var field in this.Fields.Where(x => !(x is Introspection_TypeNameMetaField)))
                clonedItem.Extend(field.Clone(clonedItem));

            return clonedItem;
        }

        /// <inheritdoc />
        public override bool ValidateObject(object item)
        {
            return item == null || Validation.IsCastable(item.GetType(), this.ObjectType);
        }

        /// <inheritdoc />
        public Type ObjectType { get; }
    }
}