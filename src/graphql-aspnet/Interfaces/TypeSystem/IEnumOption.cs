// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    /// <summary>
    /// An interface describing data to fully populate an enumeration item into the object graph.
    /// </summary>
    public interface IEnumOption : INamedItem, IDeprecatable
    {
        /// <summary>
        /// When overridden in a child class, allows the option to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the option from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        void ValidateOrThrow();

        /// <summary>
        /// Gets a value indicating whether this instance was explicitly declared to be part of the object graph or not.
        /// </summary>
        /// <value><c>true</c> if this instance is explicitly declared; otherwise, <c>false</c>.</value>
        bool IsExplicitlyDeclared { get; }
    }
}