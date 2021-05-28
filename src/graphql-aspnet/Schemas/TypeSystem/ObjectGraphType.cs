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
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Introspection.Fields;

    /// <summary>
    /// A represention of a graphql object in the schema. This object defines all the exposed
    /// fields that can be "requested" when this type is queried.
    /// </summary>
    [DebuggerDisplay("OBJECT {Name} (Fields = {Fields.Count})")]
    public class ObjectGraphType : BaseObjectGraphType, IObjectGraphType, ITypedItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectGraphType" /> class.
        /// </summary>
        /// <param name="name">The name of the graph type.</param>
        /// <param name="concreteType">The concrete type that this graph type is made from.</param>
        /// <param name="graphFields">The initial set of graph fields to add to this instance.</param>
        public ObjectGraphType(string name, Type concreteType, IEnumerable<IGraphField> graphFields = null)
            : base(name, graphFields)
        {
            this.ObjectType = Validation.ThrowIfNullOrReturn(concreteType, nameof(concreteType));
            this.InternalName = this.ObjectType.FriendlyName();

            this.GraphFieldCollection.AddField(new Introspection_TypeNameMetaField(name));
        }

        /// <summary>
        /// Extends this graph type by adding a new field to its collection. An exception may be thrown if
        /// a field with the same name already exists.
        /// </summary>
        /// <param name="newField">The new field.</param>
        public void Extend(IGraphField newField)
        {
            this.GraphFieldCollection.AddField(newField);
        }

        /// <summary>
        /// Determines whether the provided item is of a concrete type represented by this graph type.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is of the correct type; otherwise, <c>false</c>.</returns>
        public override bool ValidateObject(object item)
        {
            return item == null || Validation.IsCastable(item.GetType(), this.ObjectType);
        }

        /// <summary>
        /// Gets the type of the object this graph type was made from.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType { get; }

        /// <summary>
        /// Gets a fully qualified name of the type as it exists on the server (i.e.  Namespace.ClassName). This name
        /// is used in many exceptions and internal error messages.
        /// </summary>
        /// <value>The name of the internal.</value>
        public string InternalName { get; }
    }
}