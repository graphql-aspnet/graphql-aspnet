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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A builder for constructing hte individual pipelines the schema will use when executing a query.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1619:Generic type parameters should be documented partial class", Justification = "Partial class quirk.")]
    public partial class SchemaBuilder<TSchema>
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Retrives the index of the given descriptor.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>System.Int32.</returns>
        public int IndexOf(ServiceDescriptor item)
        {
            return this.Options.ServiceCollection.IndexOf(item);
        }

        /// <summary>
        /// Inserts the descriptor at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        public void Insert(int index, ServiceDescriptor item)
        {
            this.Options.ServiceCollection.Insert(index, item);
        }

        /// <summary>
        /// Removes the descriptor at the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveAt(int index)
        {
            this.Options.ServiceCollection.RemoveAt(index);
        }

        /// <summary>
        /// Adds the specified item to the collection.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(ServiceDescriptor item)
        {
            this.Options.ServiceCollection.Add(item);
        }

        /// <summary>
        /// Clears the service collection instance.
        /// </summary>
        public void Clear()
        {
            this.Options.ServiceCollection.Clear();
        }

        /// <summary>
        /// Determines whether this instance contains the descriptor.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the service collection contains the descriptor; otherwise, <c>false</c>.</returns>
        public bool Contains(ServiceDescriptor item)
        {
            return this.Options.ServiceCollection.Contains(item);
        }

        /// <summary>
        /// Copies the service collection to the provided array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            this.Options.ServiceCollection.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the specified item from the service collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns><c>true</c> if removed, <c>false</c> otherwise.</returns>
        public bool Remove(ServiceDescriptor item)
        {
            return this.Options.ServiceCollection.Remove(item);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>IEnumerator&lt;ServiceDescriptor&gt;.</returns>
        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return this.Options.ServiceCollection.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>IEnumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Options.ServiceCollection.GetEnumerator();
        }

        /// <summary>
        /// Gets the number of descriptors in the collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => this.Options.ServiceCollection.Count;

        /// <summary>
        /// Gets a value indicating whether this service collection is read only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        public bool IsReadOnly => this.Options.ServiceCollection.IsReadOnly;

        /// <summary>
        /// Gets or sets the <see cref="ServiceDescriptor"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the descriptor to retrieve.</param>
        /// <returns>ServiceDescriptor.</returns>
        public ServiceDescriptor this[int index]
        {
            get
            {
                return this.Options.ServiceCollection[index];
            }

            set
            {
                this.Options.ServiceCollection[index] = value;
            }
        }
    }
}