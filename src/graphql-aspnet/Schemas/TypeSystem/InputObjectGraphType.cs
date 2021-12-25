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
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Variables;

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
        public InputObjectGraphType(string name, Type objectType)
            : base(name)
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