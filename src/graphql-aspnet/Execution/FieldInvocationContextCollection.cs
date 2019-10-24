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
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A general collection field execution contexts and metadata
    /// about them.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class FieldInvocationContextCollection : IFieldInvocationContextCollection
    {
        private readonly HashSet<Tuple<string, string, string, Type>> _uniqueContexts;
        private readonly List<IGraphFieldInvocationContext> _contexts;
        private readonly List<IGraphFieldInvocationContext> _secureContexts;
        private readonly HashSet<Type> _acceptableTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldInvocationContextCollection" /> class.
        /// </summary>
        /// <param name="data">A set of contexts to prepopulate this instance with.</param>
        public FieldInvocationContextCollection(IEnumerable<IGraphFieldInvocationContext> data = null)
        {
            _uniqueContexts = new HashSet<Tuple<string, string, string, Type>>();
            _contexts = new List<IGraphFieldInvocationContext>();
            _secureContexts = new List<IGraphFieldInvocationContext>();
            _acceptableTypes = new HashSet<Type>();

            if (data != null)
            {
                foreach (var item in data)
                    this.Add(item);
            }
        }

        /// <summary>
        /// Adds the specified context to the collection.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Add(IGraphFieldInvocationContext context)
        {
            if (context == null)
                return;

            // as a collection is built its possible that
            // a field (with a given alias/name) is added more than once
            // for instance if field on an interface and as part of a fragment
            // has the same resultant name
            // these fields can be safely merged or in the case of this library
            // the secondary one just discarded
            // spec: https://graphql.github.io/graphql-spec/June2018/#sec-Field-Selection-Merging
            //
            // key (Alias Name, Field name, return graph type name, expected source type)
            var contextKey = Tuple.Create(context.Name, context.Field.Name, context.Field.TypeExpression.TypeName, context.ExpectedSourceType);
            if (_uniqueContexts.Contains(contextKey))
                return;

            _uniqueContexts.Add(contextKey);
            _contexts.Add(context);
            if (context.ExpectedSourceType != null)
            {
                _acceptableTypes.Add(context.ExpectedSourceType);
            }

            if (context.Field.SecurityGroups.Any())
                _secureContexts.Add(context);
        }

        /// <summary>
        /// Determines whether this any context in this collection could accept a source item
        /// of the given type as its source data value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if an item in this instance can accept the specified type; otherwise, <c>false</c>.</returns>
        public bool CanAcceptSourceType(Type type)
        {
            return type != null && _acceptableTypes.Contains(type);
        }

        /// <summary>
        /// Gets the number of contexts in this collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _contexts.Count;

        /// <summary>
        /// Gets the <see cref="IGraphFieldInvocationContext"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>IGraphFieldExecutionContext.</returns>
        public IGraphFieldInvocationContext this[int index] => _contexts[index];

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IGraphFieldInvocationContext> GetEnumerator()
        {
            return _contexts.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets a pre-filtered set of the invocation contexts in this collection
        /// that have security requrirements attached to them.
        /// </summary>
        /// <value>The secure contexts.</value>
        public IEnumerable<IGraphFieldInvocationContext> SecureContexts => _secureContexts;
    }
}