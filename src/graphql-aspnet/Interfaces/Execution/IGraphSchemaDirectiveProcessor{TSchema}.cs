// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.Execution
{
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An interface describing a processor that will, for the target schema,
    /// execute all applied type system level directives against their target schema
    /// items during initialization.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this processor works against.</typeparam>
    public interface IGraphSchemaDirectiveProcessor<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Scans all types, fields and arguments and applies any
        /// found directives in their declared order. The directives must also be found in
        /// the schema or an exception will be thrown.
        /// </summary>
        /// <param name="schema">The schema to scan.</param>
        void ApplyDirectives(TSchema schema);
    }
}