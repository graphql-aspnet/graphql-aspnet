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
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A graph type template describing an OBJECT graph type.
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
        protected override IEnumerable<IMemberInfoProvider> GatherPossibleFieldTemplates()
        {
            return this.ObjectType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
              .Where(x => !x.IsAbstract && !x.IsGenericMethod && !x.IsSpecialName).Cast<MemberInfo>()
              .Concat(this.ObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
              .Select(x => new MemberInfoProvider(x));
        }

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.OBJECT;
    }
}