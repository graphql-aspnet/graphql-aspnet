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

    /// <summary>
    /// A represention of a graphql object in the schema. This object defines all the exposed
    /// fields that can be "requested" when this type is queried.
    /// </summary>
    [DebuggerDisplay("INPUT {Name} (Fields = {Fields.Count})")]
    public class InputObjectGraphType : BaseObjectGraphType, IInputObjectGraphType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputObjectGraphType" /> class.
        /// </summary>
        /// <param name="name">The name of the graph type.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="graphFields">The initial set of graph fields to add to this instance.</param>
        public InputObjectGraphType(string name, Type objectType, IEnumerable<IGraphField> graphFields)
            : base(name, graphFields)
        {
            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));
            this.InternalName = this.ObjectType.FriendlyName();
        }

        /// <summary>
        /// Determines whether the provided item is of a concrete type represented by this graph type.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is of the correct type; otherwise, <c>false</c>.</returns>
        public override bool ValidateObject(object item)
        {
            return item == null || item.GetType() == this.ObjectType;
        }

        /// <summary>
        /// Gets the value indicating what type of graph type this instance is in the type system. (object, scalar etc.)
        /// </summary>
        /// <value>The kind.</value>
        public override TypeKind Kind => TypeKind.INPUT_OBJECT;

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType { get; }

        /// <summary>
        /// Gets the name of the internal.
        /// </summary>
        /// <value>The name of the internal.</value>
        public string InternalName { get; }
    }
}