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
    using System.Collections.Generic;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A context, containing all the requisite information to properly execute a field request
    /// for single item within an overall <see cref="IGraphOperationRequest"/>.
    /// </summary>
    public interface IGraphFieldInvocationContext
    {
        /// <summary>
        /// Places a restriction on this context such that it will only be executed if the provided source item can be successfully
        /// case to the provided type. Pass null to indicate no restrictions.
        /// </summary>
        /// <param name="restrictToType">Type of the restrict to.</param>
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
        /// Gets the directives that should be executed as part of this context.
        /// </summary>
        /// <value>The directives.</value>
        IList<IDirectiveInvocationContext> Directives { get; }

        /// <summary>
        /// Gets the child contexts that are dependent on this context's result in order to execute.
        /// </summary>
        /// <value>The child contexts.</value>
        IFieldInvocationContextCollection ChildContexts { get; }

        /// <summary>
        /// Gets the origin location, in the source document, that coorisponds to this field context.
        /// </summary>
        /// <value>The location.</value>
        SourceOrigin Origin { get; }

        /// <summary>
        /// Gets the source type, if any, that the source object (when this field is executed) must be castable to
        /// in order for it to be resolved. A value of null indicates no restrictions are set.
        /// </summary>
        /// <value>The source type restriction.</value>
        Type ExpectedSourceType { get; }
    }
}