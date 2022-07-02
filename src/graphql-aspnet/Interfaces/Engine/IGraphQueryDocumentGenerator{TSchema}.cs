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
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// Called by the runtime to convert an AST into an unvalidated query document
    /// targeting a specific schema.
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