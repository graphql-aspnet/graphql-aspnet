// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.TypeCollections;

    /// <summary>
    /// An managed collection of all the known types the schema can process.
    /// </summary>
    public interface ISchemaTypeCollection : IEnumerable<IGraphType>
    {
        /// <summary>
        /// Registers the extension field to the <see cref="IObjectGraphType" /> corrisponding to the supplied
        /// concrete type. If a matching graph type cannot be found for the concrete type supplied, the field
        /// is queued for when it is registered.
        /// </summary>
        /// <param name="masterType">The master type that, once added to the schema, will trigger that addition of this field.</param>
        /// <param name="field">The field that will be added to the graph type associated with the master type.</param>
        void EnsureGraphFieldExtension(Type masterType, IGraphField field);

        /// <summary>
        /// Ensures the provided <see cref="IGraphType" /> exists in this collection (adding it if it is missing)
        /// and that the given type reference is assigned to it. An exception will be thrown if the type reference is already assigned
        /// to a different <see cref="IGraphType" />. No dependents or additional types will be added.
        /// </summary>
        /// <param name="graphType">Type of the graph.</param>
        /// <param name="associatedType">The concrete type to associate to the graph type.</param>
        /// <returns><c>true</c> if type had to be added, <c>false</c> if it already existed in the collection.</returns>
        bool EnsureGraphType(IGraphType graphType, Type associatedType = null);

        /// <summary>
        /// Attempts to expand the given graph into the object graph
        /// types it represents. If the given graph type is not one that can be expanded an empty list is returned. Object
        /// graph types will be returned untouched as part of an enumeration.
        /// </summary>
        /// <param name="graphType">The graph type to expand.</param>
        /// <returns>An enumeration of graph types.</returns>
        IEnumerable<IObjectGraphType> ExpandAbstractType(IGraphType graphType);

        /// <summary>
        /// Attempts to find an <see cref="IGraphType" /> currently associated with the given concrete type. Returns null
        /// if no <see cref="IGraphType" /> is found.
        /// </summary>
        /// <param name="concreteType">The concrete type to search for.</param>
        /// <param name="kind">The graph type to search for an association of.</param>
        /// <returns>IGraphType.</returns>
        IGraphType FindGraphType(Type concreteType, TypeKind kind);

        /// <summary>
        /// Attempts to find an <see cref="IGraphType" /> with the given name. Returns null
        /// if no <see cref="IGraphType" /> is found.
        /// </summary>
        /// <param name="graphTypeName">The name of the type in the object graph.</param>
        /// <returns>IGraphType.</returns>
        IGraphType FindGraphType(string graphTypeName);

        /// <summary>
        /// Finds the graph type, if any, associated with the given field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>IGraphType.</returns>
        IGraphType FindGraphType(IGraphField field);

        /// <summary>
        /// Finds the graph type, if any, associated with the object instance.
        /// </summary>
        /// <param name="data">The data object to search with.</param>
        /// <returns>IGraphType.</returns>
        IGraphType FindGraphType(object data);

        /// <summary>
        /// Finds the single, known concrete type related to the supplied graph type (OBJECT, INPUT_OBJECT, SCALAR). Returns null if no type is found.
        /// Also, returns null if the type represents an abstract graph type such as an INTERFACE or UNION that may be mapped to more than one concrete type.
        /// Use <see cref="FindConcreteTypes(IGraphType[])"/> for multi-targeted graph types.
        /// </summary>
        /// <param name="graphType">The graph type to search against.</param>
        /// <returns>Type.</returns>
        Type FindConcreteType(IGraphType graphType);

        /// <summary>
        /// Finds the concrete types related to the supplied graph types. returns an empty list if no types are found.
        /// </summary>
        /// <param name="graphTypes">The graph types to search against.</param>
        /// <returns>A collection of conrete types valid for the given graph types.</returns>
        IEnumerable<Type> FindConcreteTypes(params IGraphType[] graphTypes);

        /// <summary>
        /// Executes an analysis operation to attempt to find all known concrete <see cref="Type"/>
        /// for the given <paramref name="targetGraphType"/> that <paramref name="typeToCheck"/>
        /// could masquerade as for the <paramref name="targetGraphType"/>.
        /// In most cases this will be one system <see cref="Type"/> but could be more in the case of abstact types
        /// such as <see cref="TypeKind.UNION"/> or <see cref="TypeKind.INTERFACE"/>.
        /// </summary>
        /// <param name="targetGraphType">The graph type to which <paramref name="typeToCheck"/> was supplied for.</param>
        /// <param name="typeToCheck">The type wishing to be used as the <paramref name="targetGraphType"/>.</param>
        /// <returns>IEnumerable&lt;Type&gt;.</returns>
        SchemaConcreteTypeAnalysisResult AnalyzeRuntimeConcreteType(IGraphType targetGraphType, Type typeToCheck);

        /// <summary>
        /// Attempts to find a single directive within this schema by its name. Returns null
        /// if the directive is not found.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>IDirectiveGraphType.</returns>
        IDirectiveGraphType FindDirective(string name);

        /// <summary>
        /// Retrieves the collection of graph types that implement the provided named interface,
        /// if any.
        /// </summary>
        /// <param name="interfaceType">The interface type type to look for.</param>
        /// <returns>IEnumerable&lt;IGraphType&gt;.</returns>
        IEnumerable<IObjectGraphType> FindGraphTypesByInterface(IInterfaceGraphType interfaceType);

        /// <summary>
        /// Retrieves the collection of graph types that implement the provided named interface
        /// if any.
        /// </summary>
        /// <param name="interfaceName">Name of the interface.</param>
        /// <returns>IEnumerable&lt;IGraphType&gt;.</returns>
        IEnumerable<IObjectGraphType> FindGraphTypesByInterface(string interfaceName);

        /// <summary>
        /// Determines whether the specified graph type name exists in this collection.
        /// </summary>
        /// <param name="graphTypeName">Name of the graph type.</param>
        /// <returns><c>true</c> if the specified graph type name contains key; otherwise, <c>false</c>.</returns>
        bool Contains(string graphTypeName);

        /// <summary>
        /// Determines whether this collection contains a <see cref="Type" /> refrence for the
        /// provided concrete type as an object type reference (different from an input type).
        /// </summary>
        /// <param name="concreteType">Type of the concrete.</param>
        /// <param name="kind">The kind of graph type to create from the supplied concrete type. If not supplied the concrete type will
        /// attempt to auto assign a type of scalar, enum or object as necessary.</param>
        /// <returns><c>true</c> if the type collection contains a reference to the concrete type for the given kind; otherwise, <c>false</c>.</returns>
        bool Contains(Type concreteType, TypeKind? kind = null);

        /// <summary>
        /// Determines whether this collection contains a <see cref="IGraphType" /> as an object type reference (different from an input type).
        /// </summary>
        /// <param name="graphType">the graph type to search for.</param>
        /// <returns><c>true</c> if the graph type is found; otherwise, <c>false</c>.</returns>
        bool Contains(IGraphType graphType);

        /// <summary>
        /// Gets the total number of <see cref="IGraphType"/> in this collection.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>
        /// Gets the count of queued <see cref="IGraphField"/>.
        /// </summary>
        /// <value>The un registered field count.</value>
        int QueuedExtensionFieldCount { get; }

        /// <summary>
        /// Gets a set of all the unique concrete types in this collection.
        /// </summary>
        /// <value>The type references.</value>
        IEnumerable<Type> TypeReferences { get; }
    }
}