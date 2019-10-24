// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution.ValueResolvers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An object that, for a given schema, can generate a resolver function to evaluate
    /// and properly build input data values used in graph queries.
    ///
    /// </summary>
    public class InputResolverMethodGenerator
    {
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputResolverMethodGenerator"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public InputResolverMethodGenerator(ISchema schema)
        {
            _schema = schema;
        }

        /// <summary>
        /// Creates the resolver for the given expression. The Typename of the expression must be
        /// type represented in the schema this generator references or null will be returned.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>IQueryInputValueResolver.</returns>
        public IInputValueResolver CreateResolver(GraphTypeExpression expression)
        {
            var graphType = _schema.KnownTypes.FindGraphType(expression.TypeName);
            if (graphType == null)
                return null;

            return this.CreateResolver(graphType, expression);
        }

        /// <summary>
        /// Creates the resolver.
        /// </summary>
        /// <param name="graphType">The graph type to generate a resolver for.</param>
        /// <param name="expression">The expression represneting how the type should be wrapped.</param>
        /// <returns>IQueryInputValueResolver.</returns>
        private IInputValueResolver CreateResolver(IGraphType graphType, GraphTypeExpression expression)
        {
            // extract the core resolver for the input type being processed
            IInputValueResolver coreResolver = null;
            Type coreType = null;
            if (graphType is IScalarGraphType scalar)
            {
                coreType = _schema.KnownTypes.FindConcreteType(scalar);
                coreResolver = new ScalarInputResolver(scalar.SourceResolver);
            }
            else if (graphType is IEnumGraphType enumGraphType)
            {
                coreType = _schema.KnownTypes.FindConcreteType(enumGraphType);
                coreResolver = new EnumValueResolver(coreType);
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

        /// <summary>
        /// Creates the object resolver used to individually resolve the fields against the schema and assemble the final input object.
        /// </summary>
        /// <param name="inputType">The graph type to generate a resolver for.</param>
        /// <param name="type">The concrete type that the supplied graph type is linked to.</param>
        /// <returns>IQueryInputValueResolver.</returns>
        private IInputValueResolver CreateObjectResolver(IInputObjectGraphType inputType, Type type)
        {
            var inputObjectResolver = new InputObjectResolver(inputType, type);

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