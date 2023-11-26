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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Resolvers;
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
        /// Initializes a new instance of the <see cref="BaseIntrospectionObjectType" /> class.
        /// </summary>
        /// <param name="name">The name of the graph type as it is displayed in the __type information.</param>
        /// <param name="internalName">The internal name of the introspected graph type as its defined for the target graph type.</param>
        protected BaseIntrospectionObjectType(string name, string internalName)
            : base(name, internalName, new IntrospectedFieldPath(name))
        {
        }

        /// <summary>
        /// Creates and adds a new <see cref="IGraphField" /> to the growing collection.
        /// </summary>
        /// <typeparam name="TSource">The expected type of the source data supplied to the resolver.</typeparam>
        /// <typeparam name="TReturn">The expected type of data to be returned from this field.</typeparam>
        /// <param name="fieldName">The formatted name of the field as it will appear in the object graph.</param>
        /// <param name="internalName">The internal name of the field as its represented in source code.</param>
        /// <param name="typeExpression">The item representing how this field returns a graph type.</param>
        /// <param name="itemPath">The formal path that uniquely identifies this field in the object graph.</param>
        /// <param name="resolver">The resolver used to fulfil requests to this field.</param>
        /// <param name="description">The description to assign to the field.</param>
        /// <returns>IGraphTypeField.</returns>
        protected IGraphField AddField<TSource, TReturn>(
            string fieldName,
            string internalName,
            GraphTypeExpression typeExpression,
            ItemPath itemPath,
            Func<TSource, Task<TReturn>> resolver,
            string description = null)
            where TSource : class
        {
            IGraphField field = new MethodGraphField(
                fieldName,
                internalName,
                typeExpression,
                itemPath,
                GraphValidation.EliminateNextWrapperFromCoreType(typeof(TReturn)),
                typeof(TReturn),
                FieldResolutionMode.PerSourceItem,
                new FunctionGraphFieldResolver<TSource, TReturn>(resolver));

            field.Description = description;
            field = field.Clone(parent: this);

            return this.GraphFieldCollection.AddField(field);
        }

        /// <inheritdoc />
        public override bool ValidateObject(object item)
        {
            return true;
        }

        /// <inheritdoc />
        public override IGraphField Extend(IGraphField newField)
        {
            throw new GraphTypeDeclarationException($"Introspection type '{this.Name}' cannot be extended");
        }

        /// <inheritdoc />
        public override IGraphType Clone(string typeName = null)
        {
            throw new InvalidOperationException($"Introspection type '{this.Name}' cannot be cloned");
        }

        /// <inheritdoc />
        public virtual Type ObjectType => this.GetType();
    }
}