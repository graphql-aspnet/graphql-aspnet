// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.Configuration
{
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An object that needs to apply some configuration or setup to the schema
    /// before its considered "complete" and ready to serve.
    /// </summary>
    public interface ISchemaConfigurationExtension
    {
        /// <summary>
        /// Instructs this configuration mechanism to apply itself to the supplied schema.
        /// </summary>
        /// <param name="schema">The schema to inspect.</param>
        void Configure(ISchema schema);
    }
}