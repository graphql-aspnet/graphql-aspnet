// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Execution.Contexts
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.RulesEngine;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A document context used to validate the actual, resolved variable data against a query before its executed. This context guaruntees that: <br />
    /// 1. The query document is properly structured <br />
    /// 2. An operation has been chosen to be invoked <br />
    /// 3. All declared variables have been validated and deemed coercable where used within the operation and referenced fragments.<br />
    /// 4. Variable data has been successfully resolved from the request and is available for inspection
    /// </summary>
    public class VariableDataValidationContext : IRuleProcessorChildContextGenerator<VariableDataValidationContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableDataValidationContext" /> class.
        /// </summary>
        /// <param name="schema">The schema instance the data is being validated in.</param>
        /// <param name="queryDocument">The query document that was parsed from the initial request.</param>
        /// <param name="targetOperation">
        /// The chosen operation that has been validated and will execute. The operation will be
        /// set as the active part for this context.
        /// </param>
        /// <param name="messages">The set of messages, if any, to record against. If null, a new collection of messages will be generated.</param>
        /// <param name="variables">The set of fully realized values assigned for each resolved variable in the <paramref name="targetOperation" />.</param>
        public VariableDataValidationContext(
            ISchema schema,
            IQueryDocument queryDocument,
            IOperationDocumentPart targetOperation,
            IResolvedVariableCollection variables,
            IGraphMessageCollection messages = null)
            : this(schema, queryDocument, targetOperation, variables, targetOperation, messages)
        {
        }

        private VariableDataValidationContext(
            ISchema schema,
            IQueryDocument queryDocument,
            IOperationDocumentPart targetOperation,
            IResolvedVariableCollection variables,
            IDocumentPart activePart,
            IGraphMessageCollection messages = null)
        {
            this.Schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            this.QueryDocument = Validation.ThrowIfNullOrReturn(queryDocument, nameof(queryDocument));
            this.Operation = Validation.ThrowIfNullOrReturn(targetOperation, nameof(targetOperation));
            this.VariableData = Validation.ThrowIfNullOrReturn(variables, nameof(variables));
            this.ActivePart = Validation.ThrowIfNullOrReturn(activePart, nameof(activePart));
            this.Messages = messages ?? new GraphMessageCollection();
            this.RuleMetaData = new Dictionary<Guid, object>();
        }

        /// <inheritdoc />
        public IEnumerable<VariableDataValidationContext> CreateChildContexts()
        {
            // since this context only validates variable usages we only care about
            // contexts taht reference such things
            if (this.ActivePart is IOperationDocumentPart opdocPart)
            {
                foreach (var usagePart in opdocPart.VariableUsages)
                {
                    yield return new VariableDataValidationContext(
                        this.Schema,
                        this.QueryDocument,
                        this.Operation,
                        this.VariableData,
                        usagePart,
                        this.Messages);
                }
            }
        }

        /// <summary>
        /// Gets the fully resolved set of variables, keyed by their supplied name, as determined by the data values provided
        /// on the request (e.g. the json document that represented the varaibles).
        /// </summary>
        public IResolvedVariableCollection VariableData { get; private set; }

        /// <summary>
        /// Gets the part being validated on this context.
        /// </summary>
        /// <value>The active part.</value>
        public IDocumentPart ActivePart { get; private set; }

        /// <summary>
        /// Gets the operation chosen by the runtime that will execute (unless otherwise stopped due to validation failures).
        /// </summary>
        public IOperationDocumentPart Operation { get; private set; }

        /// <summary>
        /// Gets the full query document that was generated from the query text received ont eh request.
        /// </summary>
        public IQueryDocument QueryDocument { get; private set; }

        /// <summary>
        /// Gets the message collection where validation errors should be saved.
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages { get; private set; }

        /// <summary>
        /// Gets a metadata object (by rule id) to carry information
        /// between rule invocations.
        /// </summary>
        /// <value>The shared metadata for this request invocation.</value>
        public Dictionary<Guid, object> RuleMetaData { get; private set; }

        /// <summary>
        /// Gets the part type of the current <see cref="ActivePart" />.
        /// </summary>
        public DocumentPartType DocumentPartType => this.ActivePart.PartType;

        /// <summary>
        /// Gets the schema current in focus.
        /// </summary>
        public ISchema Schema { get; set; }
    }
}