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
        public FieldInvocationContextCollection()
        {
            _uniqueContexts = new HashSet<Tuple<string, string, string, Type>>();
            _contexts = new List<IGraphFieldInvocationContext>();
            _secureContexts = new List<IGraphFieldInvocationContext>();
            _acceptableTypes = new HashSet<Type>();
        }

        /// <inheritdoc />
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
            // spec: https://graphql.github.io/graphql-spec/October2021/#sec-Field-Selection-Merging
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

            if (context.Field.SecurityGroups.Count > 0)
                _secureContexts.Add(context);
        }

        /// <inheritdoc />
        public bool CanAcceptSourceType(Type type)
        {
            return type != null && _acceptableTypes.Contains(type);
        }

        /// <inheritdoc />
        public int Count => _contexts.Count;

        /// <inheritdoc />
        public IGraphFieldInvocationContext this[int index] => _contexts[index];

        /// <inheritdoc />
        public IEnumerator<IGraphFieldInvocationContext> GetEnumerator()
        {
            return _contexts.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}