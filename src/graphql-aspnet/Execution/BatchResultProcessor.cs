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
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;

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
        /// Determines whether the provided type to check is IDictionary{ExpectedKey, AnyValue} ensuring that it can be
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

            bool CheckArgs(Type[] argSet)
            {
                if (argSet.Length != 2 || argSet[0] != expectedKeyType)
                    return false;

                // the declared dictionary might have a value of a List<BatchedItemType>
                // strip the list decorators and test the actual type
                var actualBatchedItemType = GraphValidation.EliminateWrappersFromCoreType(argSet[1]);
                return batchedItemType == actualBatchedItemType;
            }

            // special case when the typeToCheck IS IDictionary
            if (typeToCheck.IsGenericType && typeToCheck.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                return CheckArgs(typeToCheck.GetGenericArguments());
            }
            else
            {
                var idictionaries = typeToCheck.GetInterfaces().Where(x =>
                      x.IsGenericType &&
                      x.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                foreach (var dic in idictionaries)
                {
                    if (CheckArgs(dic.GetGenericArguments()))
                        return true;
                }
            }

            return false;
        }
    }
}