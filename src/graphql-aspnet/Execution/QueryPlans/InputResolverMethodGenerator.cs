// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.ValueResolvers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Middleware.DirectiveExecution.Components;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An object that, for a given schema, can generate a resolver function to evaluate
    /// and properly build dynamic values expressed in graph queries as part of field arguments
    /// and input objects.
    /// </summary>
    internal class InputResolverMethodGenerator
    {
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputResolverMethodGenerator"/> class.
        /// </summary>
        /// <param name="schema">The schema this generator will reference when building
        /// the resolver.</param>
        public InputResolverMethodGenerator(ISchema schema)
        {
            _schema = schema;
        }

        /// <summary>
        /// Creates the resolver for the given expression. The Typename of the expression must be
        /// type represented in the schema this generator references or null will be returned.
        /// </summary>
        /// <param name="typeExpression">The type expression of the data type
        /// that should be created by this resolver.</param>
        /// <returns>IQueryInputValueResolver.</returns>
        public IInputValueResolver CreateResolver(GraphTypeExpression typeExpression)
        {
            Validation.ThrowIfNull(typeExpression, nameof(typeExpression));

            var graphType = _schema.KnownTypes.FindGraphType(typeExpression.TypeName);
            if (graphType == null)
                return null;

            return this.CreateResolver(graphType, typeExpression);
        }

        private IInputValueResolver CreateResolver(IGraphType graphType, GraphTypeExpression expression)
        {
            // extract the core resolver for the input type being processed
            IInputValueResolver coreResolver = null;
            Type coreType = null;
            if (graphType is IScalarGraphType scalar)
            {
                coreType = _schema.KnownTypes.FindConcreteType(scalar);
                coreResolver = new ScalarValueInputResolver(scalar.SourceResolver);
            }
            else if (graphType is IEnumGraphType enumGraphType)
            {
                coreType = _schema.KnownTypes.FindConcreteType(enumGraphType);
                coreResolver = new EnumValueInputResolver(enumGraphType.SourceResolver);
            }
            else if (graphType is IInputObjectGraphType inputType)
            {
                coreType = _schema.KnownTypes.FindConcreteType(inputType);
                coreResolver = this.CreateObjectResolver(inputType, coreType);
            }

            // wrap any list wrappers around core resolver according to the type expression
            for (var i = expression.Wrappers.Length - 1; i >= 0; i--)
            {
                if (expression.Wrappers[i] == MetaGraphTypes.IsList)
                {
                    coreResolver = new ListValueResolver(coreType, coreResolver);
                    coreType = typeof(IEnumerable<>).MakeGenericType(coreType);
                }
            }

            return coreResolver;
        }

        private IInputValueResolver CreateObjectResolver(IInputObjectGraphType inputType, Type type)
        {
            var inputObjectResolver = new InputObjectResolver(inputType, type, _schema);

            foreach (var field in inputType.Fields)
            {
                IInputValueResolver childResolver;
                if (field.TypeExpression.TypeName == inputType.Name)
                {
                    childResolver = inputObjectResolver;
                }
                else
                {
                    var graphType = _schema.KnownTypes.FindGraphType(field.TypeExpression.TypeName);
                    childResolver = this.CreateResolver(graphType, field.TypeExpression);
                }

                inputObjectResolver.AddFieldResolver(field.Name, childResolver);
            }

            return inputObjectResolver;
        }
    }
}