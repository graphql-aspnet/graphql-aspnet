// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    /// <summary>
    /// An item that is part of a <see cref="ISchema"/>.
    /// </summary>
    public interface ISchemaItem : INamedItem
    {
        /// <summary>
        /// Gets a collection of directives applied to this schema item
        /// when it was instantiated in a schema.
        /// </summary>
        /// <value>The directives.</value>
        IAppliedDirectiveCollection AppliedDirectives { get; }
    }
}