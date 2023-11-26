// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers.ActionResults.Batching
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Controllers;

    /// <summary>
    /// A builder to help construct a batch result for a field that can be properly dispursed to the source items in the batch.
    /// </summary>
    /// <typeparam name="TSource">The type of the source item that were used to build this batch.</typeparam>
    /// <typeparam name="TResult">The type of object being returned from this field.</typeparam>
    /// <typeparam name="TKey">The key value from source item that will identify it in the batch.</typeparam>
    public class BatchBuilder<TSource, TResult, TKey> : IBatchBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchBuilder{TSource, TResult, TKey}"/> class.
        /// </summary>
        /// <param name="sourceData">The source data.</param>
        /// <param name="resultData">The result data.</param>
        /// <param name="sourceKeySelector">The source key selector.</param>
        /// <param name="resultKeySelector">The result key selector.</param>
        public BatchBuilder(
            IEnumerable<TSource> sourceData,
            IEnumerable<TResult> resultData,
            Func<TSource, TKey> sourceKeySelector,
            Func<TResult, IEnumerable<TKey>> resultKeySelector)
        {
            this.SourceData = sourceData;
            this.ResultData = resultData;
            this.SourceKeySelector = sourceKeySelector;
            this.ResultKeySelector = resultKeySelector;
        }

        /// <summary>
        /// Creates the final lookup result for this batch.
        /// </summary>
        /// <returns>A graph action result containing the results.</returns>
        public IGraphActionResult Complete()
        {
            if (this.SourceData == null)
            {
                return new InternalServerErrorGraphActionResult("The source data list, when attempting to finalize a " +
                                                                $"batch was null.");
            }

            if (this.SourceKeySelector == null)
            {
                return new InternalServerErrorGraphActionResult("The source key locator, when attempting to finalize a " +
                                                                $"batch was null.");
            }

            if (this.ResultData != null && this.ResultKeySelector == null)
            {
                return new InternalServerErrorGraphActionResult("The result key locator, when attempting to finalize a " +
                                                                $"batch was null.");
            }

            return new CompleteBatchOperationGraphActionResult<TSource, TResult, TKey>(this);
        }

        /// <summary>
        /// Gets or sets the source data.
        /// </summary>
        /// <value>The source data.</value>
        public IEnumerable<TSource> SourceData { get; set; }

        /// <summary>
        /// Gets or sets the result data.
        /// </summary>
        /// <value>The result data.</value>
        public IEnumerable<TResult> ResultData { get; set; }

        /// <summary>
        /// Gets or sets a function to extract a single key value from each source item item to marry it
        /// with each matched result item that produces that same key.
        /// </summary>
        /// <value>The source key selector.</value>
        public Func<TSource, TKey> SourceKeySelector { get; set; }

        /// <summary>
        /// Gets or sets a function to extract a single key value from each result item to marry it with source data
        /// that produces a matching value.
        /// </summary>
        /// <value>The result key selector.</value>
        public Func<TResult, IEnumerable<TKey>> ResultKeySelector { get; set; }
    }
}