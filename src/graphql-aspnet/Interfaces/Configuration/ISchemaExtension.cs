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
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// An object that needs to extend or apply some changes to the schema
    /// before its considered "complete" and ready to serve.
    /// </summary>
    public interface ISchemaExtension
    {
        /// <summary>
        /// Instructs this configuration mechanism to apply itself to the supplied schema.
        /// </summary>
        /// <param name="schema">The schema to extend.</param>
        void Extend(ISchema schema);
    }
}