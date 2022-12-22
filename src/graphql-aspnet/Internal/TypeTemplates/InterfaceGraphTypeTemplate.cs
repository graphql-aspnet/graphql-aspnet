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
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An graph type template describing an INTERFACE graph type.
    /// </summary>
    [DebuggerDisplay("Object: {InternalName}")]
    public class InterfaceGraphTypeTemplate : NonLeafGraphTypeTemplateBase, IInterfaceGraphTypeTemplate
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

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.INTERFACE;
    }
}