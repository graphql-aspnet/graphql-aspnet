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
        /// Initializes a new instance of the <see cref="SchemaOptions{TSchema}"/> class.
        /// </summary>
        public SchemaOptions()
            : base(typeof(TSchema))
        {
        }

        /// <summary>
        /// Registers a runtime component that this schema will forward all requests to for
        /// query resolution.
        /// </summary>
        /// <typeparam name="TRuntime">The type of the runtime object to use.</typeparam>
        /// <param name="lifetimeScope">In a DI context, what is the scope of the runtime when it
        /// is created.</param>
        public void RegisterRuntime<TRuntime>(ServiceLifetime lifetimeScope)
            where TRuntime : IGraphQLRuntime<TSchema>
        {
            this.RuntimeDescriptor = new ServiceDescriptor(typeof(IGraphQLRuntime<TSchema>), typeof(TRuntime), lifetimeScope);
        }

        /// <summary>
        /// Gets the runtime descriptor.
        /// </summary>
        /// <value>The runtime descriptor.</value>
        internal ServiceDescriptor RuntimeDescriptor { get; private set; }
    }
}