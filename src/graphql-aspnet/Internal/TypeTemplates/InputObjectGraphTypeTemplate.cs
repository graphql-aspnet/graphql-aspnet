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
    using System.Reflection;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A representation of the meta data of any given class that could be represented
    /// as an input object graph type in an <see cref="ISchema"/>.
    /// </summary>
    public class InputObjectGraphTypeTemplate : BaseObjectGraphTypeTemplate, IInputObjectGraphTypeTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputObjectGraphTypeTemplate"/> class.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        public InputObjectGraphTypeTemplate(Type objectType)
            : base(objectType)
        {
            if (objectType.IsClass)
            {
                // class objects MUST declare a default constructor
                // so it can be used in a 'new T()' operation when generating
                // input params
                var constructor = objectType.GetConstructor(new Type[0]);
                if (constructor == null || !constructor.IsPublic)
                {
                    throw new GraphTypeDeclarationException(
                        $"The type '{objectType.FriendlyName()}' does not declare a public, parameterless constructor " +
                        $"and cannot be used as an {nameof(TypeKind.INPUT_OBJECT)} graph type.");
                }
            }

            if (objectType.IsInterface)
            {
                throw new GraphTypeDeclarationException(
                    $"The type '{objectType.FriendlyName()}' is an interface and cannot be used as an {nameof(TypeKind.INPUT_OBJECT)} graph type.");
            }

            // since KeyValuePair<,> is pretty common
            // and a specific error message for the type
            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                throw new GraphTypeDeclarationException(
                    $"The type '{objectType.FriendlyName()}' cannot be used as an {nameof(TypeKind.INPUT_OBJECT)} graph type. '{typeof(KeyValuePair<,>).FriendlyName()}' does not " +
                    $"declare public setters for its Key and Value properties.");
            }
        }

        /// <inheritdoc />
        protected override GraphFieldPath GenerateFieldPath()
        {
            // a standard graph object cannot contain any route pathing or nesting like controllers can
            // before creating hte route, ensure that the declared name, by itself, is valid for graphql
            var graphName = GraphTypeNames.ParseName(this.ObjectType, TypeKind.INPUT_OBJECT);
            return new GraphFieldPath(GraphFieldPath.Join(GraphCollection.Types, graphName));
        }

        /// <inheritdoc />
        protected override bool CouldBeGraphField(MemberInfo memberInfo)
        {
            // methods can never be fields on input objects (only basic properties)
            if (memberInfo is MethodInfo)
                return false;

            // we must be able to set property values of input objects
            if (memberInfo is PropertyInfo pi)
            {
                if (pi.GetSetMethod() == null)
                    return false;
            }

            return base.CouldBeGraphField(memberInfo);
        }

        /// <inheritdoc />
        protected override IGraphTypeFieldTemplate CreateMethodFieldTemplate(MethodInfo methodInfo)
        {
            // safety check to ensure no method fields are created for an input object
            return null;
        }

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.INPUT_OBJECT;
    }
}