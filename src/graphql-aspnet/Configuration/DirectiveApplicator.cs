// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Configuration
{
    using System;
    using System.Linq.Expressions;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A class that encapsulates the late binding of a directive to a schema item
    /// and relavant fields there in.
    /// </summary>
    public class DirectiveApplicator : IDirectiveApplicator, ISchemaConfigurationExtension
    {
        private Type _directiveType;
        private string _directiveName;
        private object[] _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveApplicator"/> class.
        /// </summary>
        /// <param name="directiveType">Type of the directive being applied in this instnace.</param>
        public DirectiveApplicator(Type directiveType)
        {
            _directiveType = Validation.ThrowIfNullOrReturn(directiveType, nameof(directiveType));
            Validation.ThrowIfNotCastable<GraphDirective>(_directiveType, nameof(directiveType));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveApplicator" /> class.
        /// </summary>
        /// <param name="directiveName">Name of the directive as it is declared in the schema
        /// where it is being applied.</param>
        public DirectiveApplicator(string directiveName)
        {
            _directiveName = Validation.ThrowIfNullWhiteSpaceOrReturn(directiveName, nameof(directiveName));
        }

        /// <inheritdoc />
        public void Configure(ISchema schema)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IDirectiveApplicator WithArguments(params object[] arguments)
        {
            _arguments = arguments;
            return this;
        }

        // SCHEMA, OBJECT, INTERFACE, INPUT_OBJECT, UNION, ENUM, SCALAR
        public IDirectiveApplicator ToOperation(GraphOperationType operationType)
        {
            return this;
        }

        // SCHEMA, OBJECT, INTERFACE, INPUT_OBJECT, UNION, ENUM, SCALAR
        public IDirectiveApplicator ToGraphType(string graphTypeName)
        {
            return this;
        }

        public IDirectiveApplicator ToGraphType<TType>(TypeKind typeKind)
        {
            return this;
        }

        public IDirectiveApplicator ToGraphType<TType>()
        {
            return this.ToGraphType<TType>(TypeKind.OBJECT);
        }

        public IDirectiveApplicator ToObject<TType>()
        {
            return this.ToGraphType<TType>(TypeKind.OBJECT);
        }

        public IDirectiveApplicator ToInputObject<TType>()
        {
            return this.ToGraphType<TType>(TypeKind.INPUT_OBJECT);
        }

        public IDirectiveApplicator ToInterface<TType>()
        {
            return this.ToGraphType<TType>(TypeKind.INTERFACE);
        }

        public IDirectiveApplicator ToUnion<TType>()
            where TType : IGraphUnionProxy
        {
            return this.ToGraphType<TType>(TypeKind.UNION);
        }

        public IDirectiveApplicator ToEnum<TType>()
            where TType : Enum
        {
            return this.ToGraphType<TType>(TypeKind.ENUM);
        }

        public IDirectiveApplicator ToEnumValue<TType>(object enumValue)
            where TType : Enum
        {
            return this;
        }

        public IDirectiveApplicator ToFieldDefinition<TType>(
            Expression<Func<TType, object>> field)
        {
            return this;
        }

        public IDirectiveApplicator ToFieldDefinition<TType>(
            Expression<Func<TType, Delegate>> field)
        {
            return this;
        }

        public IDirectiveApplicator ToFieldArgument<TType>(
            Expression<Func<TType, object>> field,
            string argumentName)
        {
            return this;
        }

        public IDirectiveApplicator ToFieldArgument<TType>(
            Expression<Func<TType, Delegate>> field,
            string argumentName)
        {
            return this;
        }

        public IDirectiveApplicator ToFieldDefinition<TType>(string fieldName)
        {
            return this;
        }

        public IDirectiveApplicator ToInputFieldDefinition<TType>(
            Expression<Func<TType, object>> field)
        {
            return this;
        }

        public IDirectiveApplicator ToInputFieldDefinition<TType>(string fieldName)
        {
            return this;
        }

    }
}