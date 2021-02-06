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
    /// A representation of a parsed description of the meta data of any given interface that could be represented
    /// as a type in an <see cref="ISchema"/>.
    /// </summary>
    [DebuggerDisplay("Object: {InternalName}")]
    public class InterfaceGraphTypeTemplate : BaseObjectGraphTypeTemplate, IInterfaceGraphTypeTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceGraphTypeTemplate " /> class.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        public InterfaceGraphTypeTemplate(Type interfaceType)
            : base(interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new GraphTypeDeclarationException(
                    $"The type '{interfaceType.FriendlyName()}' is not an interface and cannot be parsed as an interface graph type.",
                    interfaceType);
            }
        }

        /// <summary>
        /// Gets the kind of graph type that can be made from this template.
        /// </summary>
        /// <value>The kind.</value>
        public override TypeKind Kind => TypeKind.INTERFACE;
    }
}