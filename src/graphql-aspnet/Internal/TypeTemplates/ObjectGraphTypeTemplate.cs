// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An graph type template describing an OBJECT graph type.
    /// </summary>
    [DebuggerDisplay("Object: {InternalName}")]
    public class ObjectGraphTypeTemplate : NonLeafGraphTypeTemplateBase, IObjectGraphTypeTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectGraphTypeTemplate" /> class.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        public ObjectGraphTypeTemplate(Type objectType)
            : base(objectType)
        {
        }

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.OBJECT;
    }
}