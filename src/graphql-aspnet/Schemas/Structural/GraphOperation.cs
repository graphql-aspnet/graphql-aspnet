﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Structural
{
    using System.Diagnostics;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Introspection.Fields;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A representation of one of the three graph roots on a schema. The root contains the
    /// fields that can be acted on to fulfill a requested operation.
    /// </summary>
    [DebuggerDisplay("{Name}, Fields = {Fields.Count}")]
    public class GraphOperation : BaseObjectGraphType, IGraphOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphOperation" /> class.
        /// </summary>
        /// <param name="operationType">The operation type this instance represents.</param>
        /// <param name="directives">The directives to apply to this operation
        /// when its added to a schema.</param>
        public GraphOperation(
            GraphCollection operationType,
            IAppliedDirectiveCollection directives = null)
            : base(
                  Constants.ReservedNames.FindOperationTypeNameByType(operationType),
                  new GraphFieldPath(GraphCollection.Types, Constants.ReservedNames.FindOperationTypeNameByType(operationType)),
                  directives)
        {
            this.OperationType = operationType;
            this.Extend(new Introspection_TypeNameMetaField(Constants.ReservedNames.FindOperationTypeNameByType(operationType)));
        }

        /// <summary>
        /// Determines whether the provided item is of a concrete type represented by this graph type.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is of the correct type; otherwise, <c>false</c>.</returns>
        public override bool ValidateObject(object item)
        {
            return false;
        }

        /// <summary>
        /// Gets the graph operation represented by this instance.
        /// </summary>
        /// <value>The type of the root.</value>
        public GraphCollection OperationType { get; }

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
        /// Gets a value indicating whether this instance is virtual and added by the runtime to facilitate
        /// a user defined graph structure. When false, this graph types points to a concrete type
        /// defined by a developer.
        /// </summary>
        /// <value><c>true</c> if this instance is virtual; otherwise, <c>false</c>.</value>
        public override bool IsVirtual => true;
    }
}