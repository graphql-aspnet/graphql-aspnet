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
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.PlanGeneration.InputArguments;

    /// <summary>
    /// The default concrete representation of a field execution context.
    /// </summary>
    /// <seealso cref="GraphQL.AspNet.Interfaces.Execution.IGraphFieldInvocationContext" />
    [Serializable]
    [DebuggerDisplay("Field Context: {Name} (Restict To: {ExpectedSourceTypeName}, Children = {ChildContexts.Count})")]
    public class FieldInvocationContext : IGraphFieldInvocationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldInvocationContext" /> class.
        /// </summary>
        /// <param name="expectedSourceType">Expected type of the source.</param>
        /// <param name="name">The name to apply to this data set once resolution is complete.</param>
        /// <param name="field">The field.</param>
        /// <param name="origin">The origin, in the source text, that this context was generated from.</param>
        /// <param name="directives">The directives parsed from a query document that are to be executed as part of this context.</param>
        public FieldInvocationContext(
            Type expectedSourceType,
            string name,
            IGraphField field,
            SourceOrigin origin,
            IEnumerable<IDirectiveInvocationContext> directives = null)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            this.Field = Validation.ThrowIfNullOrReturn(field, nameof(field));
            this.ExpectedSourceType = expectedSourceType;
            this.Origin = origin;
            this.ChildContexts = new FieldInvocationContextCollection();
            this.Arguments = new InputArgumentCollection();

            var list = new List<IDirectiveInvocationContext>();
            if (directives != null)
                list.AddRange(directives);
            this.Directives = list;
        }

        /// <summary>
        /// Places a restriction on this context such that it will only be executed if the provided source item can be successfully
        /// cast to the provided type. Pass null to indicate no restrictions.
        /// </summary>
        /// <param name="restrictToType">The concrete object type to restrict execution to.</param>
        public void Restrict(Type restrictToType)
        {
            this.ExpectedSourceType = restrictToType;
        }

        /// <summary>
        /// Gets a set of arguments that need to be passed to the resolver to complete the operation.
        /// </summary>
        /// <value>The arguments.</value>
        public IInputArgumentCollection Arguments { get; }

        /// <summary>
        /// Gets the field to be executed when this context is invoked.
        /// </summary>
        /// <value>The field.</value>
        public IGraphField Field { get; }

        /// <summary>
        /// Gets the source type, if any, that the source object (when this field is executed) must be castable to
        /// in order for it to be resolved. A value of null indicates no restrictions are set.
        /// </summary>
        /// <value>The source type restriction.</value>
        public Type ExpectedSourceType { get; private set; }

        /// <summary>
        /// Gets the expected name of the source type.
        /// </summary>
        /// <value>The expected name of the source type.</value>
        private string ExpectedSourceTypeName => this.ExpectedSourceType?.FriendlyName() ?? "null";

        /// <summary>
        /// Gets the directives that should be executed as part of this context.
        /// </summary>
        /// <value>The directives.</value>
        public IList<IDirectiveInvocationContext> Directives { get; }

        /// <summary>
        /// Gets the name of the field as it exists in the execution chain. This is typically
        /// the field name or an alias, if it was supplied in the source document.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the child contexts that are dependent on this context's result in order to execute.
        /// </summary>
        /// <value>The child contexts.</value>
        public IFieldInvocationContextCollection ChildContexts { get; }

        /// <summary>
        /// Gets the origin location, in the source document, that coorisponds to this field context.
        /// </summary>
        /// <value>The location.</value>
        public SourceOrigin Origin { get; }
    }
}