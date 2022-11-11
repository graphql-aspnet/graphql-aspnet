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
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.Lexing.Source;

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
        IGraphQueryDocument CreateDocument(SourceText sourceText, SynTree syntaxTree);

        /// <summary>
        /// Validates a query document as being valid against the given <typeparamref name="TSchema"/>.
        /// </summary>
        /// <remarks>
        /// Note: A return value of <c>true</c> indicates that the validation completed, not that it was successful.
        /// Inspect the document's messages collection for any validation failures.</remarks>
        /// <param name="document">The document.</param>
        /// <returns><c>true</c> if the validation completed successfully, <c>false</c> otherwise.</returns>
        bool ValidateDocument(IGraphQueryDocument document);
    }
}