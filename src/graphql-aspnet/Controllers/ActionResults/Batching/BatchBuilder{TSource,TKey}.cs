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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A builder to help construct a batch result for a field that can be properly dispursed to the source items in the batch.
    /// </summary>
    /// <typeparam name="TSource">The type of the source item that were used to build this batch.</typeparam>
    /// <typeparam name="TKey">The key value from source item that will identify it in the batch.</typeparam>
    public class BatchBuilder<TSource, TKey> : BatchBuilder<TSource, object, TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchBuilder{TSource, TKey}" /> class.
        /// </summary>
        /// <param name="field">The field for witch this batch is being produced.</param>
        /// <param name="sourceData">The source data.</param>
        /// <param name="sourceKeySelector">A function to extract a single key value from each source item.</param>
        public BatchBuilder(IGraphField field, IEnumerable<TSource> sourceData, Func<TSource, TKey> sourceKeySelector)
            : base(field, sourceData, null, sourceKeySelector, null)
        {
        }

        /// <summary>
        /// Appends (or replaces) the given results to the batch using the provided key selector to marry each result item to the source item (or items)
        /// for which it matches a single key value. Use this method for a 1:Many or 1:1 relationships, building a collection of children (or a single items)
        /// for each source item provided.
        /// </summary>
        /// <typeparam name="TResult">The type of object being returned from this field.</typeparam>
        /// <param name="resultData">The actual data produced in the batch operation.</param>
        /// <param name="resultKeySelector">A function to extract a single key value from each result item to marry it with source data that produces a matching value.</param>
        /// <returns>A fully qualified batch builder capable of creating a batch result.</returns>
        public BatchBuilder<TSource, TResult, TKey> WithResults<TResult>(IEnumerable<TResult> resultData, Func<TResult, TKey> resultKeySelector)
            where TResult : class
        {
            return new BatchBuilder<TSource, TResult, TKey>(
                this.Field,
                this.SourceData,
                resultData,
                this.SourceKeySelector,
                (resultItem) => resultKeySelector(resultItem).AsEnumerable());
        }

        /// <summary>
        /// Appends the given results to the batch using the provided key selector to marry each result item with multiple the source items
        /// for which it matches a single key value.Use this method for Many:Many relationships, building a collection of children where each result item is assigned to
        /// each source item it returns a matched key for.
        /// </summary>
        /// <typeparam name="TResult">The type of object being returned from this field.</typeparam>
        /// <param name="resultData">The data produced in the batch operation.</param>
        /// <param name="resultKeySelector">A function to extract multiple key values from each result item to marry it with each matched source item from that key set.</param>
        /// <returns>A fully qualified batch builder capable of creating a batch result.</returns>
        public BatchBuilder<TSource, TResult, TKey> WithResults<TResult>(IEnumerable<TResult> resultData, Func<TResult, IEnumerable<TKey>> resultKeySelector)
            where TResult : class
        {
            return new BatchBuilder<TSource, TResult, TKey>(this.Field, this.SourceData, resultData, this.SourceKeySelector, resultKeySelector);
        }
    }
}