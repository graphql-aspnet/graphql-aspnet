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
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A scaled down, customized priority queue for ordering <see cref="IGraphExecutionContext"/>
    /// for execution.
    /// </summary>
    public class SortedFieldExecutionContextList
    {
        private List<GraphFieldExecutionContext> _syncList;
        private List<GraphFieldExecutionContext> _paralellList;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedFieldExecutionContextList"/> class.
        /// </summary>
        public SortedFieldExecutionContextList()
        {
            _syncList = new List<GraphFieldExecutionContext>();
            _paralellList = new List<GraphFieldExecutionContext>();
        }

        /// <summary>
        /// Adds a new context to the collection.
        /// </summary>
        /// <param name="context">The context to insert.</param>
        /// <param name="executeInIsolation">if set to <c>true</c> the context will be flagged
        /// as running in isolation.</param>
        public void Add(GraphFieldExecutionContext context, bool executeInIsolation)
        {
            Validation.ThrowIfNull(context, nameof(context));

            if (executeInIsolation)
                _syncList.Add(context);
            else
                _paralellList.Add(context);
        }

        /// <summary>
        /// Gets the contexts of this set that must be executed in isolation.
        /// </summary>
        /// <value>The isolated contexts.</value>
        public IEnumerable<GraphFieldExecutionContext> IsolatedContexts => _syncList;

        /// <summary>
        /// Gets the contexts of this set that can be executed simultaniously.
        /// </summary>
        /// <value>The paralell contexts.</value>
        public IEnumerable<GraphFieldExecutionContext> ParalellContexts => _paralellList;
    }
}