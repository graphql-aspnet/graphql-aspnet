// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Engine
{
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// Validates that a lexed syntax tree, intended to execute a query on a target schema,
    /// is valid on that schema such that all field selection sets for all graph types are valid and executable
    /// ultimately generating a document that can be used to create a query plan from the various resolvers
    /// that can complete the query.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this generator is registered for.</typeparam>
    public interface IGraphQueryDocumentGenerator<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Interpretes the syntax tree and generates a contextual document that can be transformed into
        /// a query plan.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree to create a document for.</param>
        /// <returns>IGraphQueryDocument.</returns>
        IGraphQueryDocument CreateDocument(ISyntaxTree syntaxTree);
    }
}