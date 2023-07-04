// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.Introspection
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An intermediate base class to define logic common to all "object graph types" that
    /// are part of the introspection system.
    /// </summary>
    internal abstract class BaseIntrospectionObjectType : ObjectGraphTypeBase, IObjectGraphType, IInternalSchemaItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseIntrospectionObjectType"/> class.
        /// </summary>
        /// <param name="name">The name of the graph type as it is displayed in the __type information.</param>
        protected BaseIntrospectionObjectType(string name)
            : base(name, new GraphIntrospectionFieldPath(name))
        {
        }

        /// <inheritdoc />
        public override bool ValidateObject(object item)
        {
            return true;
        }

        /// <inheritdoc />
        public IGraphField Extend(IGraphField newField)
        {
            throw new GraphTypeDeclarationException($"Introspection type '{this.Name}' cannot be extended");
        }

        /// <inheritdoc />
        public virtual Type ObjectType => this.GetType();

        /// <inheritdoc />
        public virtual string InternalFullName => this.ObjectType.FriendlyName();
    }
}