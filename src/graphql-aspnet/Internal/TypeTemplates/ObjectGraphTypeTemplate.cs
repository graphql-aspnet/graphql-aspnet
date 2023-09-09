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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
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
        protected override IEnumerable<MemberInfo> GatherPossibleTemplateMembers()
        {
            return this.ObjectType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
              .Where(x =>
                        !x.IsAbstract &&
                        !x.IsGenericMethod &&
                        !x.IsSpecialName &&
                        x.DeclaringType != typeof(object) &&
                        x.DeclaringType != typeof(ValueType))
              .Cast<MemberInfo>()
              .Concat(this.ObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance));
        }

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.OBJECT;
    }
}