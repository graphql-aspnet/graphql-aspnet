// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Structural
{
    using System.Diagnostics;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An object returned by resolving a field not bound to a concrete type (commonly <see cref="VirtualGraphField"/>). This
    /// object contains some metadata about the field from which it was built to assist in type introspection requests normally
    /// handled by the concrete type assoications in the target <see cref="ISchema"/>. This object acts as a bridge between the
    /// non-concrete nature of virtual graph fields and the need for all resolvers to return usable data for downstream resolvers.
    /// </summary>
    [DebuggerDisplay("Virtual Object (GraphTypeName = {GraphTypeName})")]
    [GraphType(
        Publish = false,
        FieldDeclarationRequirements = TemplateDeclarationRequirements.RequireMethodAndProperties)]
    public sealed class VirtualResolvedObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualResolvedObject"/> class.
        /// </summary>
        /// <param name="graphTypeName">Name of the graph type this object represents.</param>
        public VirtualResolvedObject(string graphTypeName)
        {
            this.GraphTypeName = graphTypeName;
        }

        /// <summary>
        /// Gets the name of the graph type on the schema where the <see cref="VirtualGraphField"/> that produced this object is
        /// declared.
        /// </summary>
        /// <value>The name of the graph type.</value>
        public string GraphTypeName { get; }
    }
}