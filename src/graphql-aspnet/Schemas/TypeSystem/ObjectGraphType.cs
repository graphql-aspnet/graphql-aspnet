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
    public class ObjectGraphType : BaseObjectGraphType, IObjectGraphType, ITypedSchemaItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectGraphType" /> class.
        /// </summary>
        /// <param name="name">The name of the graph type.</param>
        /// <param name="objectType">The concrete type that this graphtype is made from.</param>
        public ObjectGraphType(string name, Type objectType)
            : base(name)
        {
            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));
            this.InternalName = this.ObjectType.FriendlyName();

            this.GraphFieldCollection.AddField(new Introspection_TypeNameMetaField(name));
        }

        /// <inheritdoc />
        public void Extend(IGraphField newField)
        {
            this.GraphFieldCollection.AddField(newField);
        }

        /// <inheritdoc />
        public override bool ValidateObject(object item)
        {
            return item == null || Validation.IsCastable(item.GetType(), this.ObjectType);
        }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public string InternalName { get; }
    }
}