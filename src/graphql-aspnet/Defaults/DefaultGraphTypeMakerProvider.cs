// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using GraphQL.AspNet.Defaults.TypeMakers;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An abstract factory for creating type makers using all the default, built in type makers.
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
    }
}