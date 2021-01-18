// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Attributes
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A decorator attribute to identify a method as being attached directly to the subscription root operation,
    /// as apposed to being nested under its controller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SubscriptionRootAttribute : GraphFieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionRootAttribute" /> class.
        /// </summary>
        public SubscriptionRootAttribute()
            : this(Constants.Routing.ACTION_METHOD_META_NAME)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionRootAttribute" /> class.
        /// </summary>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        public SubscriptionRootAttribute(string template)
            : this(template, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionRootAttribute" /> class.
        /// </summary>
        /// <param name="returnType">The type of the object returned from this method. If this type implements
        /// <see cref="IGraphUnionProxy"/> this field will be declared as returning the union defined by the type.</param>
        public SubscriptionRootAttribute(Type returnType)
            : this(Constants.Routing.ACTION_METHOD_META_NAME, returnType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionRootAttribute" /> class.
        /// </summary>
        /// <param name="returnType">The type of the data object returned from this method. If this type implements
        /// <see cref="IGraphUnionProxy"/> this field will be declared as returning the union defined by the type. In the event that the return type is an interface
        /// be sure to supply any additional concrete types so that they may be included in the object graph.</param>
        /// <param name="additionalTypes">Any additional types to include in the object graph on behalf of this method.</param>
        public SubscriptionRootAttribute(Type returnType, params Type[] additionalTypes)
            : this(Constants.Routing.ACTION_METHOD_META_NAME, returnType, additionalTypes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionRootAttribute" /> class.
        /// </summary>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        /// <param name="returnType">The type of the object returned from this method. If this type implements
        /// <see cref="IGraphUnionProxy"/> this field will be declared as returning the union defined by the type.</param>
        public SubscriptionRootAttribute(string template, Type returnType)
            : base(true, GraphCollection.Subscription, template, returnType)
        {
            this.EventName = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionRootAttribute" /> class.
        /// </summary>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        /// <param name="returnType">The type of the object returned from this method. If this type implements
        /// <see cref="IGraphUnionProxy"/> this field will be declared as returning the union defined by the type. In the event that the return type is an interface
        /// be sure to supply any additional concrete types so that they may be included in the object graph.</param>
        /// <param name="additionalTypes">Any additional types to include in the object graph on behalf of this method.</param>
        public SubscriptionRootAttribute(string template, Type returnType, params Type[] additionalTypes)
            : base(true, GraphCollection.Subscription, template, returnType.AsEnumerable().Concat(additionalTypes).ToArray())
        {
            this.EventName = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionRootAttribute" /> class.
        /// </summary>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        /// <param name="unionTypeName">Name of the union type.</param>
        /// <param name="unionTypeA">The first of two required types to include in the union.</param>
        /// <param name="unionTypeB">The second of two required types to include in the union.</param>
        /// <param name="additionalUnionTypes">Any additional union types.</param>
        public SubscriptionRootAttribute(string template, string unionTypeName, Type unionTypeA, Type unionTypeB, params Type[] additionalUnionTypes)
            : base(
                true,
                GraphCollection.Subscription,
                template,
                unionTypeName,
                unionTypeA.AsEnumerable().Concat(unionTypeB.AsEnumerable()).Concat(additionalUnionTypes).ToArray())
        {
            this.EventName = null;
        }

        /// <summary>
        /// Gets or sets an alterate schema-specific name for this event that can be referenced
        /// when raising it, rather than the full path to the field.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; set; }
    }
}