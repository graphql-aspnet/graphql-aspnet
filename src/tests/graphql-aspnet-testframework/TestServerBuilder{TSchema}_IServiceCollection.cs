// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A builder class to configure a scenario and generate a test server to execute unit tests against.
    /// </summary>
    public partial class TestServerBuilder<TSchema>
    {
        /// <inheritdoc />
        public int IndexOf(ServiceDescriptor item)
        {
            return this.SchemaOptions.ServiceCollection.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, ServiceDescriptor item)
        {
            this.SchemaOptions.ServiceCollection.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            this.SchemaOptions.ServiceCollection.RemoveAt(index);
        }

        /// <inheritdoc />
        public void Add(ServiceDescriptor item)
        {
            this.SchemaOptions.ServiceCollection.Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            this.SchemaOptions.ServiceCollection.Clear();
        }

        /// <inheritdoc />
        public bool Contains(ServiceDescriptor item)
        {
            return this.SchemaOptions.ServiceCollection.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            this.SchemaOptions.ServiceCollection.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(ServiceDescriptor item)
        {
            return this.SchemaOptions.ServiceCollection.Remove(item);
        }

        /// <inheritdoc />
        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return this.SchemaOptions.ServiceCollection.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.SchemaOptions.ServiceCollection.GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => this.SchemaOptions.ServiceCollection.Count;

        /// <inheritdoc />
        public bool IsReadOnly => this.SchemaOptions.ServiceCollection.IsReadOnly;

        /// <inheritdoc />
        public ServiceDescriptor this[int index]
        {
            get
            {
                return this.SchemaOptions.ServiceCollection[index];
            }

            set
            {
                this.SchemaOptions.ServiceCollection[index] = value;
            }
        }
    }
}