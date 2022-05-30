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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when a type system directive is successfully applied to a targeted <see cref="ISchemaItem"/>.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema on which the directive was applied.</typeparam>
    public class TypeSystemDirectiveAppliedLogEntry<TSchema> : GraphLogEntry
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSystemDirectiveAppliedLogEntry{TSchema}" /> class.
        /// </summary>
        /// <param name="directiveApplied">The directive that was applied.</param>
        /// <param name="appliedTo">the schema item the directive was applied to.</param>
        public TypeSystemDirectiveAppliedLogEntry(IDirective directiveApplied, ISchemaItem appliedTo)
            : base(LogEventIds.TypeSystemDirectiveApplied)
        {
            this.SchemaTypeName = typeof(TSchema).FriendlyName(true);
            this.SchemaItemPath = appliedTo?.Route?.Path;
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
        /// Gets the relative url registered for this schema type to listen on.
        /// </summary>
        /// <value>The name of the schema route.</value>
        public string SchemaItemPath
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_ITEM_PATH);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_ITEM_PATH, value);
        }

        /// <summary>
        /// Gets the schema assigned name of the <see cref="IDirective"/>
        /// that was applied to <see cref="SchemaItemPath"/>.
        /// </summary>
        /// <value>The name of the applied directive.</value>
        public string DirectiveName
        {
            get => this.GetProperty<string>(LogPropertyNames.DIRECTIVE_NAME);
            private set => this.SetProperty(LogPropertyNames.DIRECTIVE_NAME, value);
        }

        /// <summary>
        /// Gets the internal name of the <see cref="IDirective"/> instance
        /// that was applied to <see cref="SchemaItemPath"/>.
        /// </summary>
        /// <value>The name of the applied directive.</value>
        public string DirectiveInternalName
        {
            get => this.GetProperty<string>(LogPropertyNames.DIRECTIVE_INTERNAL_NAME);
            private set => this.SetProperty(LogPropertyNames.DIRECTIVE_INTERNAL_NAME, value);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Directive Applied | Item Path: '{this.SchemaItemPath}', Directive: '{this.DirectiveName}' ";
        }
    }
}