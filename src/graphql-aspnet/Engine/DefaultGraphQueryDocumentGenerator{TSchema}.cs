// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.PlanGeneration;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Parsing2.Lexing.Source;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document;
    using GraphQL.AspNet.RulesEngine;

    using DocumentConstructionContext = GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstructionContext;

    /// <summary>
    /// Validates that a lexed syntax tree, intended to execute a query on a target schema,
    /// is valid on that schema such that all field selection sets for all graph types are valid and executable
    /// ultimately generating a document that can be used to create a query plan from the various resolvers
    /// that can complete the query.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this generator is registered for.</typeparam>
    public class DefaultGraphQueryDocumentGenerator<TSchema> : IGraphQueryDocumentGenerator<TSchema>
        where TSchema : class, ISchema
    {
        private readonly TSchema _schema;
        private readonly Parsing2.DocumentGeneration.DocumentConstructionRuleProcessor _nodeProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultGraphQueryDocumentGenerator{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema to use when generating validating a document.</param>
        public DefaultGraphQueryDocumentGenerator(TSchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _nodeProcessor = new GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstructionRuleProcessor();
        }

        /// <inheritdoc />
        public virtual IGraphQueryDocument CreateDocument(SourceText sourceText, SyntaxTree syntaxTree)
        {
            Validation.ThrowIfNull(syntaxTree.RootNode, nameof(syntaxTree.RootNode));

            var document = new QueryDocument();
            this.FillDocument(sourceText, syntaxTree, document);
            return document;
        }

        /// <inheritdoc />
        public bool ValidateDocument(IGraphQueryDocument document)
        {
            Validation.ThrowIfNull(document, nameof(document));

            var docProcessor = new DocumentValidationRuleProcessor();
            var context = new DocumentValidationContext(_schema, document);

            return docProcessor.Execute(context);
        }

        /// <summary>
        /// An internal method for populating an existing query document.
        /// </summary>
        /// <param name="sourceText">The source text.</param>
        /// <param name="syntaxTree">The syntax tree to convert.</param>
        /// <param name="document">The query document to fill.</param>
        protected virtual void FillDocument(SourceText sourceText, SyntaxTree syntaxTree, IGraphQueryDocument document)
        {
            Validation.ThrowIfNull(syntaxTree.RootNode, nameof(syntaxTree.RootNode));
            Validation.ThrowIfNull(document, nameof(document));

            // --------------------------------------------
            // Step 1: Parse the syntax tree
            // --------------------------------------------
            // Walk all nodes of the tree and on a "per node" basis perform actions
            // that are required of that node to create pieces (IDocumentPart) of the
            // document being constructed
            // --------------------------------------------
            var constructionContext = new DocumentConstructionContext(syntaxTree, sourceText, document, _schema);

            var completedAllSteps = _nodeProcessor.Execute(ref constructionContext);

            // --------------------------------------------
            // Step 2: Part Linking
            // --------------------------------------------
            // Many document parts reference other parts, such as variable references or
            // fragment spreads. With fragment spreads at the time the parts are constructed
            // the named fragment may or may not have been parsed yet. As a result we need
            // ensure that the fragment the spread references is assigned correctly after
            // the whole document has been parsed
            // --------------------------------------------
            if (completedAllSteps)
            {
                foreach (var spread in constructionContext.Spreads)
                {
                    if (spread.Fragment != null)
                    {
                        spread.Fragment.MarkAsReferenced();
                    }
                    else
                    {
                        // mark all named fragments of this name as referenced
                        document.NamedFragments.MarkAsReferenced(spread.FragmentName.ToString());

                        // assign the official fragment reference to the spread
                        if (document.NamedFragments.TryGetValue(spread.FragmentName.ToString(), out var foundFragment))
                        {
                            spread.AssignNamedFragment(foundFragment);
                        }
                        else
                        {
                            completedAllSteps = false;
                        }
                    }
                }
            }
        }
    }
}