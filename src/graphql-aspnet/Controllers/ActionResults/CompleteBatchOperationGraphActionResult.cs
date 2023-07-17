// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers.ActionResults
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers.ActionResults.Batching;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.FieldResolution.FieldValidation;
    using GraphQL.AspNet.Interfaces.Controllers;

    /// <summary>
    /// A graph action result that is built from a batch builder and can turn said builder into an
    /// appropriate dictionary of items to resolve a type extension.
    /// </summary>
    /// <typeparam name="TSource">The type of the source item identified in the type extension.</typeparam>
    /// <typeparam name="TResult">The type of the result item identified in the type extension.</typeparam>
    /// <typeparam name="TKey">The type of the key that links <typeparamref name="TSource"/> to <typeparamref name="TResult"/>.</typeparam>
    public class CompleteBatchOperationGraphActionResult<TSource, TResult, TKey> : IGraphActionResult
    {
        private readonly BatchBuilder<TSource, TResult, TKey> _batchBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteBatchOperationGraphActionResult{TSource, TResult, TKey}"/> class.
        /// </summary>
        /// <param name="batchBuilder">The batch builder.</param>
        public CompleteBatchOperationGraphActionResult(BatchBuilder<TSource, TResult, TKey> batchBuilder)
        {
            _batchBuilder = batchBuilder;
        }

        /// <inheritdoc />
        public Task CompleteAsync(SchemaItemResolutionContext context)
        {

            if (!(context is FieldResolutionContext frc))
            {
                context.Messages.Critical(
                    $"Batch Operations cannot be completed against a resolution context of type {context?.GetType().FriendlyName()}",
                    Constants.ErrorCodes.INVALID_BATCH_RESULT,
                    context.Request.Origin);
                return Task.CompletedTask;
            }

            if (_batchBuilder == null)
            {
                context.Messages.Critical(
                    $"No batch builder was provided. Unable to complete the requested batch operation.",
                    Constants.ErrorCodes.INVALID_BATCH_RESULT,
                    frc.Request.Origin);
                return Task.CompletedTask;
            }

            var field = frc.Request.Field;
            var dictionary = new Dictionary<TSource, object>();

            // key out the results
            var resultsByKey = new Dictionary<TKey, HashSet<TResult>>();
            if (_batchBuilder.ResultData != null)
            {
                // N:N relationships should complete in O(M)
                // where M is the total number of keys defined across all instances
                //
                // 1:N relationships it should process in O(N)
                // where N is the number of result items
                foreach (var item in _batchBuilder.ResultData)
                {
                    if (item == null)
                        continue;

                    var keys = _batchBuilder.ResultKeySelector(item) ?? Enumerable.Empty<TKey>();
                    foreach (var key in keys)
                    {
                        if (key == null)
                            continue;

                        if (!resultsByKey.ContainsKey(key))
                            resultsByKey.Add(key, new HashSet<TResult>());

                        resultsByKey[key].Add(item);
                    }
                }
            }

            var fieldReturnsAList = field.TypeExpression.IsListOfItems;

            // add each source item to the reslt dictionary pulling in hte matching items from
            // the results set and ensuring list vs no list depending on the executed field.
            foreach (var sourceItem in _batchBuilder.SourceData)
            {
                object sourceResults = null;
                var key = _batchBuilder.SourceKeySelector(sourceItem);
                var lookupResults = resultsByKey.ContainsKey(key) ? resultsByKey[key] : null;
                if (lookupResults != null)
                {
                    if (fieldReturnsAList)
                    {
                        sourceResults = lookupResults.ToList();
                    }
                    else
                    {
                        if (lookupResults.Count > 1)
                        {
                            frc.Messages.Critical(
                                $"Invalid field resolution. When attempting to finalize a batch for field '{field.Name}', " +
                                $"a source item with key '{key}' had {lookupResults.Count} result(s) in the batch was expected to have " +
                                "a single item.",
                                Constants.ErrorCodes.INVALID_BATCH_RESULT,
                                frc.Request.Origin);

                            return Task.CompletedTask;
                        }

                        sourceResults = lookupResults.FirstOrDefault();
                    }
                }

                dictionary.Add(sourceItem, sourceResults);
            }

            frc.Result = dictionary;
            return Task.CompletedTask;
        }
    }
}