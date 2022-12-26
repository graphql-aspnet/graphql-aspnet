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
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// An entity that can take a <see cref="IQueryExecutionResult" /> and generate a graphql compliant
    /// and formatted response to send to the requestor.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this writer is registered for.</typeparam>
    public interface IQueryResponseWriter<TSchema> : IQueryResponseWriter
         where TSchema : class, ISchema
    {
    }
}