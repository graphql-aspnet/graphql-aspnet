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
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A builder to help construct a batch result for a field that can be properly dispursed to the source items in the batch.
    /// </summary>
    public class BatchBuilder : BatchBuilder<object, object, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchBuilder" /> class.
        /// </summary>
        /// <param name="field">The field for witch this batch is being produced.</param>
        public BatchBuilder(IGraphField field)
            : base(field, null, null, null, null)
        {
        }

        /// <summary>
        /// Appends the source data to the batch using the provided key selector to marry each source item with any matched results that return the source key.
        /// </summary>
        /// <typeparam name="TSource">The type of source object that was supplied to this field.</typeparam>
        /// <typeparam name="TKey">The type of the key value to use for matching source item to result item(s).</typeparam>
        /// <param name="sourceData">The source data that was provided or is represented in this batch. Source items supplied to this field not in this batch
        /// will receive null as their result for the batch.</param>
        /// <param name="sourceKeySelector">A function to extract a single key value from each source item item to marry it with each matched result item that produces that
        /// same key.</param>
        /// <returns>A batch builder with a given set of source data.</returns>
        public BatchBuilder<TSource, TKey> FromSource<TSource, TKey>(IEnumerable<TSource> sourceData, Func<TSource, TKey> sourceKeySelector)
            where TSource : class
        {
            return new BatchBuilder<TSource, TKey>(this.Field, sourceData, sourceKeySelector);
        }
    }
}