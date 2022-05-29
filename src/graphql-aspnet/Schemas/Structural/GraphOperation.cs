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
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Introspection.Fields;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A representation of one of the three graph roots on a schema. The root contains the
    /// fields that can be acted on to fulfill a requested operation.
    /// </summary>
    [DebuggerDisplay("{Name}, Fields = {Fields.Count}")]
    public sealed class GraphOperation : BaseObjectGraphType, IGraphOperation, IInternalSchemaItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphOperation" /> class.
        /// </summary>
        /// <param name="operationType">The operation type this instance represents.</param>
        /// <param name="directives">The directives to apply to this operation
        /// when its added to a schema.</param>
        public GraphOperation(
            GraphOperationType operationType,
            IAppliedDirectiveCollection directives = null)
            : base(
                  Constants.ReservedNames.FindOperationTypeNameByType(operationType),
                  new GraphFieldPath(GraphCollection.Types, Constants.ReservedNames.FindOperationTypeNameByType(operationType)),
                  directives)
        {
            this.OperationType = operationType;
            this.Extend(new Introspection_TypeNameMetaField(Constants.ReservedNames.FindOperationTypeNameByType(operationType)));
        }

        /// <inheritdoc />
        public override bool ValidateObject(object item)
        {
            // graph opertions are top level objects generated internally based on the query type
            // there is no scenario where one would be generated and "returned" from a resolver
            // such that it should or would need to be validated
            return false;
        }

        /// <inheritdoc />
        public GraphOperationType OperationType { get; }

        /// <inheritdoc />
        public IGraphField Extend(IGraphField newField)
        {
            return this.GraphFieldCollection.AddField(newField);
        }

        /// <inheritdoc />
        public override bool IsVirtual => true;

        /// <inheritdoc />
        public Type ObjectType => typeof(GraphOperation);

        /// <inheritdoc />
        public string InternalName => $"{typeof(GraphOperation).FriendlyName()}.{this.OperationType}";
    }
}