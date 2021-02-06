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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A representation of the meta data of any given class that could be represented
    /// as an object graph type in an <see cref="ISchema"/>.
    /// </summary>
    [DebuggerDisplay("Object: {InternalName}")]
    public class ObjectGraphTypeTemplate : BaseObjectGraphTypeTemplate, IObjectGraphTypeTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectGraphTypeTemplate" /> class.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        public ObjectGraphTypeTemplate(Type objectType)
            : base(objectType)
        {
            if (!objectType.IsClass)
            {
                throw new GraphTypeDeclarationException(
                    $"The type '{objectType.FriendlyName()}' is not a class and " +
                    $"cannot be parsed as an {nameof(TypeKind.OBJECT)} graph type.",
                    objectType);
            }
        }

        /// <summary>
        /// Gets the kind of graph type that can be made from this template.
        /// </summary>
        /// <value>The kind.</value>
        public override TypeKind Kind => TypeKind.OBJECT;
    }
}