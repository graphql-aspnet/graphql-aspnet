// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Engine.TypeMakers;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An abstract factory for creating type makers using all the default type makers types.
    /// </summary>
    public class DefaultGraphTypeMakerProvider : IGraphTypeMakerProvider
    {
        /// <inheritdoc />
        public virtual IGraphFieldMaker CreateFieldMaker(ISchema schema)
        {
            return new GraphFieldMaker(schema);
        }

        /// <inheritdoc />
        public virtual IGraphTypeMaker CreateTypeMaker(ISchema schema, TypeKind kind)
        {
            if (schema == null)
                return null;

            switch (kind)
            {
                case TypeKind.DIRECTIVE:
                    return new DirectiveMaker(schema);

                case TypeKind.SCALAR:
                    return new ScalarGraphTypeMaker();

                case TypeKind.OBJECT:
                    return new ObjectGraphTypeMaker(schema);

                case TypeKind.INTERFACE:
                    return new InterfaceGraphTypeMaker(schema);

                case TypeKind.ENUM:
                    return new EnumGraphTypeMaker(schema);

                case TypeKind.INPUT_OBJECT:
                    return new InputObjectGraphTypeMaker(schema);

                // note: unions cannot currently be made via the type maker stack
            }

            return null;
        }

        /// <inheritdoc />
        public IUnionGraphTypeMaker CreateUnionMaker(ISchema schema)
        {
            return new UnionGraphTypeMaker(schema);
        }

        /// <inheritdoc />
        public IGraphUnionProxy CreateUnionProxyFromType(Type proxyType)
        {
            if (proxyType == null)
                return null;

            IGraphUnionProxy proxy = null;
            if (Validation.IsCastable<IGraphUnionProxy>(proxyType))
            {
                var paramlessConstructor = proxyType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
                if (paramlessConstructor == null)
                {
                    throw new GraphTypeDeclarationException(
                        $"The union proxy type '{proxyType.FriendlyName()}' could not be instantiated. " +
                        "All union proxy types must declare a parameterless constructor.");
                }

                proxy = InstanceFactory.CreateInstance(proxyType) as IGraphUnionProxy;
            }

            return proxy;
        }
    }
}