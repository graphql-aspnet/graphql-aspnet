// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Logging.ExecutionEvents
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Recorded when a type system directive is successfully applied to a targeted <see cref="ISchemaItem"/>.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema on which the directive was applied.</typeparam>
    public class ExecutionDirectiveAppliedLogEntry<TSchema> : GraphLogEntry
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionDirectiveAppliedLogEntry{TSchema}" /> class.
        /// </summary>
        /// <param name="directiveApplied">The directive that was applied.</param>
        /// <param name="appliedTo">the document part the directive was applied to.</param>
        public ExecutionDirectiveAppliedLogEntry(IDirective directiveApplied, IDocumentPart appliedTo)
            : base(LogEventIds.ExecutionDirectiveApplied)
        {
            this.SchemaTypeName = typeof(TSchema).FriendlyName(true);
            this.SourceLine = appliedTo?.SourceLocation.LineNumber ?? 0;
            this.SourceLineIndex = appliedTo?.SourceLocation.LineIndex ?? 0;
            this.DirectiveLocation = appliedTo?.AsDirectiveLocation().ToString() ?? "-unknown-";
            this.DirectiveName = directiveApplied?.Name;
            this.DirectiveInternalName = directiveApplied?.InternalName;
        }

        /// <summary>
        /// Gets the <see cref="Type" /> name of the schema instance the pipeline was generated for.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public string SchemaTypeName
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_TYPE_NAME);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets the target location in the source document of the item that was targetd by the
        /// directive.
        /// </summary>
        /// <value>The directive location.</value>
        public string DirectiveLocation
        {
            get => this.GetProperty<string>(LogPropertyNames.DIRECTIVE_LOCATION);
            private set => this.SetProperty(LogPropertyNames.DIRECTIVE_LOCATION, value);
        }

        /// <summary>
        /// Gets the schema assigned name of the <see cref="IDirective"/>
        /// that was applied.
        /// </summary>
        /// <value>The name of the applied directive.</value>
        public string DirectiveName
        {
            get => this.GetProperty<string>(LogPropertyNames.DIRECTIVE_NAME);
            private set => this.SetProperty(LogPropertyNames.DIRECTIVE_NAME, value);
        }

        /// <summary>
        /// Gets the internal name of the <see cref="IDirective"/> instance
        /// that was applied to a document part.
        /// </summary>
        /// <value>The name of the applied directive.</value>
        public string DirectiveInternalName
        {
            get => this.GetProperty<string>(LogPropertyNames.DIRECTIVE_INTERNAL_NAME);
            private set => this.SetProperty(LogPropertyNames.DIRECTIVE_INTERNAL_NAME, value);
        }

        /// <summary>
        /// Gets the line index in the query text where the directive
        /// was declared/applied.
        /// </summary>
        /// <value>The source line position.</value>
        public int SourceLine
        {
            get => this.GetProperty<int>(LogPropertyNames.SOURCE_LINE);
            private set => this.SetProperty(LogPropertyNames.SOURCE_LINE, value);
        }

        /// <summary>
        /// Gets the position index on the <see cref="SourceLine"/> in the query text
        /// where the directive was declared/applied.
        /// </summary>
        /// <value>The source line position.</value>
        public int SourceLineIndex
        {
            get => this.GetProperty<int>(LogPropertyNames.SOURCE_LINE_INDEX);
            private set => this.SetProperty(LogPropertyNames.SOURCE_LINE_INDEX, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Directive Applied | Location: {this.DirectiveLocation}, {{{this.SourceLine}, {this.SourceLineIndex}}}, Directive: '{this.DirectiveName}' ";
        }
    }
}