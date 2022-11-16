// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using System;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A context, containing all the requisite information to properly resolve a field
    /// for single item within an overall <see cref="IGraphOperationRequest"/>.
    /// </summary>
    public interface IGraphFieldInvocationContext
    {
        /// <summary>
        /// Places a restriction on this context such that it will only be executed if the provided
        /// source item can be successfully cast to the provided type. Pass null to indicate no restrictions.
        /// </summary>
        /// <param name="restrictToType">A .NET type to restrict this invocation to.</param>
        void Restrict(Type restrictToType);

        /// <summary>
        /// Gets the name of the field as it exists in the execution chain. This is typically
        /// the field name or an alias, if it was supplied in the source document.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets a set of arguments that need to be passed to the resolver to complete the operation.
        /// </summary>
        /// <value>The arguments.</value>
        IInputArgumentCollection Arguments { get; }

        /// <summary>
        /// Gets the field to be executed when this context is invoked.
        /// </summary>
        /// <value>The field.</value>
        IGraphField Field { get; }

        /// <summary>
        /// Gets the field document part from which this invocation context was generated.
        /// </summary>
        /// <value>The field document part.</value>
        public IFieldDocumentPart FieldDocumentPart { get; }

        /// <summary>
        /// Gets the child contexts that are dependent on this context's result in order to execute.
        /// </summary>
        /// <value>The child contexts.</value>
        IFieldInvocationContextCollection ChildContexts { get; }

        /// <summary>
        /// Gets the schema from which this context is based.
        /// </summary>
        /// <value>The schema.</value>
        ISchema Schema { get; }

        /// <summary>
        /// Gets the qualified origin location, in the source document, that coorisponds to this field context.
        /// </summary>
        /// <value>The location.</value>
        SourceOrigin Origin { get; }

        /// <summary>
        /// Gets the location, in the source text, that coorisponds to this field context.
        /// </summary>
        /// <value>The location.</value>
        SourceLocation Location { get; }

        /// <summary>
        /// Gets the source type, if any, that the source object (when this field is executed) must be castable to
        /// in order for it to be resolved. A value of null indicates no restrictions are set.
        /// </summary>
        /// <value>The source type restriction.</value>
        Type ExpectedSourceType { get; }
    }
}