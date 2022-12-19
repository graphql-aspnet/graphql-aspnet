// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine.TypeMakers
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A base collection of possible field item dependencies.
    /// </summary>
    public abstract class BaseItemDependencyCollection : IGraphItemDependencies
    {
        private readonly List<DependentType> _dependentTypes;
        private readonly List<IGraphType> _abstractTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseItemDependencyCollection"/> class.
        /// </summary>
        public BaseItemDependencyCollection()
        {
            _dependentTypes = new List<DependentType>();
            _abstractTypes = new List<IGraphType>();
        }

        /// <summary>
        /// Merges the specified dependency set into this instance, adding all
        /// its contained types.
        /// </summary>
        /// <param name="dependencySet">The dependency set to merge.</param>
        public void MergeDependents(IGraphItemDependencies dependencySet)
        {
            if (dependencySet?.DependentTypes != null)
                this.AddDependentRange(dependencySet.DependentTypes);
            if (dependencySet?.AbstractGraphTypes != null)
                this.AddAbstractDependentRange(dependencySet.AbstractGraphTypes);
        }

        /// <summary>
        /// Adds the specified abstract graph type (Unions or interface) to this instance.
        /// </summary>
        /// <param name="abstractType">The abstract type to add.</param>
        public void AddAbstractDependent(IGraphType abstractType)
        {
            if (abstractType != null)
                _abstractTypes.Add(abstractType);
        }

        /// <summary>
        /// Adds the single dependent type to this instance.
        /// </summary>
        /// <param name="type">The concrete type of the dependent.</param>
        /// <param name="expectedKind">The expected kind of the dependent when its part of the graph.</param>
        public void AddDependent(Type type, TypeKind expectedKind)
        {
            if (type != null)
            {
                var dependentType = new DependentType(type, expectedKind);
                this.AddDependent(dependentType);
            }
        }

        /// <summary>
        /// Adds the single dependent type to this instance.
        /// </summary>
        /// <param name="dependentType">The dependent type item to include.</param>
        public void AddDependent(DependentType dependentType)
        {
            if (dependentType != null)
                _dependentTypes.Add(dependentType);
        }

        /// <summary>
        /// Adds the set of dependent types to this instance.
        /// </summary>
        /// <param name="dependentTypes">The set of dependent types.</param>
        public void AddDependentRange(IEnumerable<DependentType> dependentTypes)
        {
            if (dependentTypes != null)
                _dependentTypes.AddRange(dependentTypes);
        }

        /// <summary>
        /// Adds the set of abstract graph types (Unions and interfaces) to this instance.
        /// </summary>
        /// <param name="abstractTypes">The set of abstract types.</param>
        public void AddAbstractDependentRange(IEnumerable<IGraphType> abstractTypes)
        {
            if (abstractTypes != null)
                _abstractTypes.AddRange(abstractTypes);
        }

        /// <summary>
        /// Gets a collection of concrete types (and their associated type kinds) that this graph type is dependent on if/when a resolution against a field
        /// of this type occurs.
        /// </summary>
        /// <value>The dependent types.</value>
        public IEnumerable<DependentType> DependentTypes => _dependentTypes;

        /// <summary>
        /// Gets A collection of pre-created, abstract graph (Unions and Interfaces) types that this graph type is dependent on if/when a resolution
        /// against this type occurs.
        /// </summary>
        /// <value>The dependent graph types.</value>
        public IEnumerable<IGraphType> AbstractGraphTypes => _abstractTypes;
    }
}