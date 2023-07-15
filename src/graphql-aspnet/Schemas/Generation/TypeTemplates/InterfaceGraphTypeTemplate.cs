// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
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
        protected override IEnumerable<IMemberInfoProvider> GatherPossibleFieldTemplates()
        {
            return this.ObjectType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
              .Where(x => !x.IsGenericMethod && !x.IsSpecialName).Cast<MemberInfo>()
              .Concat(this.ObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
              .Select(x => new MemberInfoProvider(x));
        }

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.INTERFACE;
    }
}