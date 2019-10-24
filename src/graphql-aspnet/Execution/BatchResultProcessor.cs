// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A data processor that handles internal batch operations for items being processed through a graph query.
    /// </summary>
    public class BatchResultProcessor
    {
        private readonly SourceOrigin _origin;
        private readonly IGraphField _field;
        private readonly IEnumerable<GraphDataItem> _sourceItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchResultProcessor" /> class.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="sourceItems">The source items.</param>
        /// <param name="origin">The origin.</param>
        public BatchResultProcessor(IGraphField field, IEnumerable<GraphDataItem> sourceItems, SourceOrigin origin)
        {
            _origin = origin;
            _field = Validation.ThrowIfNullOrReturn(field, nameof(field));
            _sourceItems = Validation.ThrowIfNullOrReturn(sourceItems, nameof(sourceItems));
            this.Messages = new GraphMessageCollection();
        }

        /// <summary>
        /// Resolves the specified response splicing it and assigning batch items into the correct source items. This
        /// method returns an enumerable of those items that were successfully assigned data.
        /// </summary>
        /// <param name="data">The data that needs to be resolved.</param>
        /// <returns>IEnumerable&lt;GraphDataItem&gt;.</returns>
        public IEnumerable<GraphDataItem> Resolve(object data)
        {
            var batch = this.CreateDataBatch(data);
            if (batch == null)
            {
                this.Messages.Critical(
                    $"Unable to process the batch field '{this._field.Name}'. The result of the batch operation was improperly formatted.",
                    Constants.ErrorCodes.INVALID_BATCH_RESULT,
                    _origin,
                    new GraphExecutionException($"Invalid batch operation result for field '{this._field.Name}'. A batch result must be a " +
                                                $"'{nameof(IDictionary)}' keyed on the source items provided for the batch. Consider using the action helper methods " +
                                                $"of '{nameof(GraphController)}' when applicable to properly generate a batch."));
                yield break;
            }

            foreach (var item in _sourceItems)
            {
                var singleItemResult = batch.RetrieveAssociatedItems(item.SourceData);
                item.AssignResult(singleItemResult);
                yield return item;
            }
        }

        /// <summary>
        /// Creates the data batch.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>FieldDataBatch.</returns>
        private DataBatch CreateDataBatch(object data)
        {
            if (data == null)
                return new DataBatch(_field.TypeExpression, null);

            if (!(data is IDictionary dictionary))
                return null;

            return new DataBatch(_field.TypeExpression, dictionary);
        }

        /// <summary>
        /// Gets the messages generated during the processing of a pipeline response.
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Determines whether the the provided type to check is IDictionary{ExpectedKey, AnyValue} ensuring that it can be
        /// used as an input to a batch processor that supplies data to multiple waiting items.
        /// </summary>
        /// <param name="typeToCheck">The type being checked.</param>
        /// <param name="expectedKeyType">The expected key type.</param>
        /// <param name="batchedItemType">Type of the batched item. if the type to check returns an IEnumerable per
        /// key this type must represent the individual item of the set. (i.e. the 'T' of IEnumerable{T}).</param>
        /// <returns><c>true</c> if the type represents a dictionary keyed on the expected key type; otherwise, <c>false</c>.</returns>
        public static bool IsBatchDictionaryType(Type typeToCheck, Type expectedKeyType, Type batchedItemType)
        {
            if (typeToCheck == null || expectedKeyType == null || batchedItemType == null)
                return false;

            var enumeratedType = typeToCheck.GetEnumerableUnderlyingType();
            if (enumeratedType == null || !enumeratedType.IsGenericType || typeof(KeyValuePair<,>) != enumeratedType.GetGenericTypeDefinition())
                return false;

            var paramSet = enumeratedType.GetGenericArguments();
            if (paramSet.Length != 2)
                return false;

            if (paramSet[0] != expectedKeyType)
                return false;

            var unwrapped = GraphValidation.EliminateWrappersFromCoreType(paramSet[1]);
            if (unwrapped != batchedItemType)
                return false;

            return true;
        }
    }
}