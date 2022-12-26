﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Engine
{
    using System;
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// Called by the runtime to convert an AST into an unvalidated query document
    /// targeting a specific schema.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this generator is registered for.</typeparam>
    public interface IQueryDocumentGenerator<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Interpretes the syntax tree and generates a contextual document that can be transformed into
        /// a query plan.
        /// </summary>
        /// <param name="queryText">The raw query text from which to generate a document.</param>
        /// <returns>IGraphQueryDocument.</returns>
        IQueryDocument CreateDocument(ReadOnlySpan<char> queryText);

        /// <summary>
        /// Validates a query document as being valid against the given <typeparamref name="TSchema"/>.
        /// </summary>
        /// <remarks>
        /// Note: A return value of <c>true</c> indicates that the validation completed, not that it was successful.
        /// Inspect the document's messages collection for any validation failures.</remarks>
        /// <param name="document">The document.</param>
        /// <returns><c>true</c> if the validation completed, <c>false</c> otherwise.</returns>
        bool ValidateDocument(IQueryDocument document);
    }
}