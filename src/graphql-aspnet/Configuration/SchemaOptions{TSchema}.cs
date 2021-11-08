// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A collection of <see cref="SchemaOptions"/> with additiona, schema specific
    /// setting used during setup.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema these options are for.</typeparam>
    public class SchemaOptions<TSchema> : SchemaOptions
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaOptions{TSchema}" /> class.
        /// </summary>
        /// <param name="serviceCollection">The service collection to which all
        /// found types or services should be registered.</param>
        public SchemaOptions(IServiceCollection serviceCollection)
            : base(typeof(TSchema), serviceCollection)
        {
        }
    }
}