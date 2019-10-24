// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Contexts
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// A base class containing common logic for all document generation contexts.
    /// </summary>
    /// <typeparam name="T">The type of the primary item being acted on in this context.</typeparam>
    internal abstract class DocumentGenerationContext<T>
        where T : class
    {
        private T _item;
        private Type _itemType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentGenerationContext{T}" /> class.
        /// </summary>
        /// <param name="documentContext">The document context.</param>
        /// <param name="item">The item.</param>
        protected DocumentGenerationContext(DocumentContext documentContext, T item)
        {
            this.ContextItems = new Dictionary<Type, IDocumentPart>();
            this.DocumentContext = documentContext;
            this.Item = item;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentGenerationContext{T}"/> class.
        /// </summary>
        protected DocumentGenerationContext()
        {
        }

        /// <summary>
        /// Adds the new context item to the current instance. If an item of the given type already exists on
        /// this context, it is replaced with this new item. When a null vaue is supplied it is automatically
        /// skipped.
        /// </summary>
        /// <param name="item">The item in question.</param>
        protected void AddOrUpdateContextItem(IDocumentPart item)
        {
            this.AddOrUpdateContextItem(item, item?.GetType());
        }

        /// <summary>
        /// Adds the new context item to the current instance. If an item of the given type already exists on
        /// this context, it is replaced with this new item. When a null vaue is supplied for either the key
        /// or the item it is automatically skipped.
        /// </summary>
        /// <param name="item">The item in question.</param>
        /// <param name="keyAsType">The specific type to use as the key type for the item.</param>
        protected void AddOrUpdateContextItem(IDocumentPart item, Type keyAsType)
        {
            if (item == null || keyAsType == null)
                return;

            if (_itemType == keyAsType)
                throw new ArgumentException($"The active '{keyAsType.FriendlyName()}' cannot be changed.");

            if (this.ContextItems.ContainsKey(keyAsType))
                this.ContextItems.Remove(keyAsType);

            this.ContextItems.Add(keyAsType, item);
        }

        /// <summary>
        /// Retrieves the context item of the given type or null if it is not found in the current
        /// context.
        /// </summary>
        /// <typeparam name="TItem">The context item type to find.</typeparam>
        /// <returns>The found context item.</returns>
        public TItem FindContextItem<TItem>()
            where TItem : class, IDocumentPart
        {
            if (this.ContextItems.ContainsKey(typeof(TItem)))
                return this.ContextItems[typeof(TItem)] as TItem;

            return null;
        }

        /// <summary>
        /// Determines whether this instance contains a context item of the given type.
        /// </summary>
        /// <typeparam name="TContextitem">The type of the item to inspect for.</typeparam>
        /// <returns><c>true</c> if an item of the given type exists in this context; otherwise, <c>false</c>.</returns>
        public virtual bool Contains<TContextitem>()
            where TContextitem : class
        {
            if (_itemType == typeof(TContextitem))
                return this.Item is TContextitem;

            return this.ContextItems.ContainsKey(typeof(TContextitem));
        }

        /// <summary>
        /// Gets or sets the context items.
        /// </summary>
        /// <value>The context items.</value>
        protected Dictionary<Type, IDocumentPart> ContextItems { get; set; }

        /// <summary>
        /// Gets or sets the item in scope on this context.
        /// </summary>
        /// <value>The item.</value>
        protected T Item
        {
            get => _item;
            set
            {
                _item = value;
                _itemType = value?.GetType();
            }
        }

        /// <summary>
        /// Gets or sets the master document context this instance is attached to.
        /// </summary>
        /// <value>The document context.</value>
        public DocumentContext DocumentContext { get; protected set; }

        /// <summary>
        /// Gets the master document context message collection for any errors generating while using this
        /// context. (this is a convience property).
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages => this.DocumentContext.Messages;
    }
}