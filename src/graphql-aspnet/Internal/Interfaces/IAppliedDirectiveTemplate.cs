// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Internal.Interfaces
{
    using System;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A template containing the necessary information to create an apply
    /// a directive to a schema item.
    /// </summary>
    public interface IAppliedDirectiveTemplate
    {
        /// <summary>
        /// Parses this template setting various properties needed to
        /// construct it.
        /// </summary>
        void Parse();

        /// <summary>
        /// Validates that this template is valid and correct
        /// or throws an exception.
        /// </summary>
        void ValidateOrThrow();

        /// <summary>
        /// Creates an instance of <see cref="IAppliedDirectiveTemplate"/>
        /// from the information contained in this template.
        /// </summary>
        /// <returns>IAppliedDirective.</returns>
        IAppliedDirective CreateAppliedDirective();

        /// <summary>
        /// Gets the owner template of this directive invocation.
        /// </summary>
        /// <value>The owner.</value>
        INamedItem Owner { get; }

        /// <summary>
        /// Gets the concrete type of the directive to apply.
        /// </summary>
        /// <value>The type of the directive.</value>
        Type DirectiveType { get; }

        /// <summary>
        /// Gets the name of the directive to apply as it exists in the schema.
        /// </summary>
        /// <value>The name of the directive.</value>
        string DirectiveName { get; }

        /// <summary>
        /// Gets the argument values to pass to the directive when its invoked.
        /// </summary>
        /// <value>The arguments.</value>
        object[] Arguments { get; }
    }
}