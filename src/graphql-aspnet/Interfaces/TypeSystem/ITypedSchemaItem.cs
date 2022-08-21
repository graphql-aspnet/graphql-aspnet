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
    using System;

    /// <summary>
    /// A  <see cref="ISchemaItem"/> constructed from a specific concrete <see cref="Type"/>.
    /// </summary>
    public interface ITypedSchemaItem : ISchemaItem
    {
        /// <summary>
        /// Gets the .NET type of the class or struct that represents this schema item at runtime.
        /// </summary>
        /// <value>The type of the object.</value>
        Type ObjectType { get; }

        /// <summary>
        /// <para>Gets a fully-qualified, internal name of schema item as it exists on the server. This name
        /// is used in many exceptions and internal error messages.
        /// </para>
        /// <para>
        /// Examples: <br/>
        /// <b>Scalar:</b>                     System.Int<br/>
        /// <b>Controller Resolver Method:</b> MyProject.MyController.RetrieveWidgets<br/>
        /// <b>Object Property:</b>            MyProject.Widget.Name<br/>
        /// .
        /// </para>
        /// </summary>
        /// <value>The fully qualiified, internal name of this schema item.</value>
        string InternalName { get; }
    }
}