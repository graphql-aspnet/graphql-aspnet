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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;

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
        /// <param name="route">The route path that identifies this object in the schema.</param>
        /// <param name="directives">The directives to apply to this input
        /// object when its added to a schema.</param>
        public InputObjectGraphType(
            string name,
            Type objectType,
            GraphFieldPath route,
            IAppliedDirectiveCollection directives = null)
            : base(name, route, directives)
        {
            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));
            this.InternalName = this.ObjectType.FriendlyName();
        }

        /// <inheritdoc />
        public override bool ValidateObject(object item)
        {
            return item == null || item.GetType() == this.ObjectType;
        }

        /// <summary>
        /// Attempts to add the field to the collection tracked by this graph type.
        /// </summary>
        /// <param name="field">The field.</param>
        public void AddField(IGraphField field)
        {
            this.GraphFieldCollection.AddField(field);
        }

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.INPUT_OBJECT;

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public string InternalName { get; }
    }
}