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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;

    /// <summary>
    /// The default schema factory, capable of creating singleton instances of
    /// schemas, fully populated and ready to serve requests.
    /// </summary>
    public partial class DefaultGraphQLSchemaFactory<TSchema>
    {
        /// <summary>
        /// Ensures that the root operation type (query, mutation etc.) exists on this schema and the associated virtual
        /// type representing it also exists in the schema's type collection.
        /// </summary>
        /// <param name="operationType">Type of the operation.</param>
        protected virtual void EnsureGraphOperationType(GraphOperationType operationType)
        {
            if (operationType == GraphOperationType.Unknown)
            {
                throw new ArgumentOutOfRangeException($"The operation type '{operationType}' is " +
                    $"not supported by graphql.");
            }

            if (!this.Schema.Operations.ContainsKey(operationType))
            {
                var operation = new GraphOperation(operationType);
                this.Schema.KnownTypes.EnsureGraphType(operation);
                this.Schema.Operations.Add(operation.OperationType, operation);
            }
        }

        /// <summary>
        /// An idempodent method that will parse and add the given type into the schema
        /// in a manner appropriate to its definition and expected type kind.
        /// </summary>
        /// <param name="type">The type to ensure exists in the graph.</param>
        /// <param name="typeKind">The type kind to add the provided <paramref name="type"/>
        /// as. If the provided type can only be matched to one type kind (such as enums), this
        /// value is ignored.
        /// </param>
        ///
        /// <remarks>
        /// <paramref name="typeKind"/> is required to differentiate OBJECT from INPUT_OBJECT registrations
        /// for explicit type inclusions.
        /// </remarks>
        protected virtual void EnsureGraphType(Type type, TypeKind? typeKind = null)
        {
            Validation.ThrowIfNull(type, nameof(type));
            try
            {
                this.EnsureGraphTypeInternal(type, typeKind);
            }
            catch (GraphTypeDeclarationException ex)
            {
                if (ex.FailedObjectType == type)
                    throw;

                // wrap a thrown exception to be nested within this type that was being parsed
                throw new GraphTypeDeclarationException(
                    $"An error occured while trying to add a dependent of '{type.FriendlyName()}' " +
                    $"to the target schema. See inner exception for details.",
                    type,
                    ex);
            }
        }

        /// <summary>
        /// A method where <see cref="EnsureGraphType(Type, TypeKind?)"/> performs its actual work.
        /// Seperated to allow exceptions to be trapped and bubbled correctly.
        /// </summary>
        /// <param name="type">The type to ensure exists in the graph.</param>
        /// <param name="typeKind">The type kind to add the provided <paramref name="type"/>
        /// as. If the provided type can only be matched to one type kind (such as enums), this
        /// value is ignored.
        /// </param>
        ///
        /// <remarks>
        /// <paramref name="typeKind"/> is required to differentiate OBJECT from INPUT_OBJECT registrations
        /// for explicit type inclusions.
        /// </remarks>
        protected virtual void EnsureGraphTypeInternal(Type type, TypeKind? typeKind = null)
        {
            type = GraphValidation.EliminateWrappersFromCoreType(type);

            // if the type is already registered, early exit no point in running it through again
            var existingGraphType = this.Schema.KnownTypes.FindGraphType(type);
            if (existingGraphType != null)
            {
                if (existingGraphType.Kind == TypeKind.SCALAR)
                    return;

                if (this.Schema.KnownTypes.Contains(type, typeKind))
                    return;
            }

            var template = this.MakerFactory.MakeTemplate(type, typeKind);
            if (template is IGraphControllerTemplate controllerTemplate)
            {
                this.AddController(controllerTemplate);
                return;
            }

            GraphTypeCreationResult makerResult = null;
            var maker = this.MakerFactory.CreateTypeMaker(type, typeKind);
            if (maker != null)
            {
                // if a maker can be assigned for this graph type
                // create the graph type directly
                makerResult = maker.CreateGraphType(template);
            }

            this.AddMakerResult(makerResult);
        }

        /// <summary>
        /// Processes the result of creating a graph type from a template and adds its contents to the schema.
        /// </summary>
        /// <param name="makerResult">The maker result to process.</param>
        protected virtual void AddMakerResult(GraphTypeCreationResult makerResult)
        {
            if (makerResult != null)
            {
                this.Schema.KnownTypes.EnsureGraphType(makerResult.GraphType, makerResult.ConcreteType);
                this.EnsureDependents(makerResult);
            }
        }

        /// <summary>
        /// Ensures the union proxy is incorporated into the schema appropriately. This method is used
        /// when a union is discovered when parsing various field declarations.
        /// </summary>
        /// <param name="union">The union to include.</param>
        protected virtual void EnsureUnion(IGraphUnionProxy union)
        {
            Validation.ThrowIfNull(union, nameof(union));
            var maker = this.MakerFactory.CreateUnionMaker();
            var result = maker.CreateUnionFromProxy(union);

            this.Schema.KnownTypes.EnsureGraphType(result.GraphType, result.ConcreteType);
        }

        /// <summary>
        /// Ensures the dependents in the provided collection are part of the target <see cref="Schema"/>.
        /// </summary>
        /// <param name="dependencySet">The dependency set to inspect.</param>
        protected virtual void EnsureDependents(IGraphItemDependencies dependencySet)
        {
            foreach (var abstractType in dependencySet.AbstractGraphTypes)
            {
                this.Schema.KnownTypes.EnsureGraphType(abstractType);
            }

            foreach (var dependent in dependencySet.DependentTypes)
            {
                if (dependent.Type != null)
                    this.EnsureGraphType(dependent.Type, dependent.ExpectedKind);
                else if (dependent.UnionDeclaration != null)
                    this.EnsureUnion(dependent.UnionDeclaration);
            }
        }
    }
}