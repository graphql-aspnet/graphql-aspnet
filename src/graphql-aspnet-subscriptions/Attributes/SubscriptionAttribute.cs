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
    /// A decorator attribute to identify a method as a field on the subscription graph root. The
    /// field will be nested inside a field or set of fields field(s) representing the controller that
    /// defines the method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class SubscriptionAttribute : GraphFieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionAttribute" /> class.
        /// </summary>
        /// <param name="eventName">The schema-unique name of the event that published whenever
        /// subscribed clients should receive new data.</param>
        public SubscriptionAttribute(string eventName)
         : this(eventName, Constants.Routing.ACTION_METHOD_META_NAME)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionAttribute" /> class.
        /// </summary>
        /// <param name="eventName">The schema-unique name of the event that published whenever
        /// subscribed clients should receive new data.</param>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        public SubscriptionAttribute(string eventName, string template)
         : this(eventName, template, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionAttribute" /> class.
        /// </summary>
        /// <param name="returnType">The type of the data object returned from this method. If this type implements
        /// <see cref="IGraphUnionProxy"/> this field will be declared as returning the union defined by the type.</param>
        public SubscriptionAttribute(string eventName, Type returnType)
        : this(eventName, Constants.Routing.ACTION_METHOD_META_NAME, returnType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionAttribute" /> class.
        /// </summary>
        /// <param name="eventName">The schema-unique name of the event that published whenever
        /// subscribed clients should receive new data.</param>
        /// <param name="returnType">The type of the data object returned from this method. If this type implements
        /// <see cref="IGraphUnionProxy"/> this field will be declared as returning the union defined by the type. In the event that the return type is an interface
        /// be sure to supply any additional concrete types so that they may be included in the object graph.</param>
        /// <param name="additionalTypes">Any additional types to include in the object graph on behalf of this method.</param>
        public SubscriptionAttribute(string eventName, Type returnType, params Type[] additionalTypes)
            : this(eventName, Constants.Routing.ACTION_METHOD_META_NAME, returnType, additionalTypes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionAttribute" /> class.
        /// </summary>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        /// <param name="returnType">The type of the data object returned from this method. If this type implements
        /// <see cref="IGraphUnionProxy"/> this field will be declared as returning the union defined by the type.</param>
        public SubscriptionAttribute(string eventName, string template, Type returnType)
            : base(false, GraphCollection.Subscription, template, returnType)
        {
            this.EventName = eventName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionAttribute" /> class.
        /// </summary>
        /// <param name="eventName">The schema-unique name of the event that published whenever
        /// subscribed clients should receive new data.</param>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        /// <param name="returnType">The type of the data object returned from this method. If this type implements
        /// <see cref="IGraphUnionProxy"/> this field will be declared as returning the union defined by the type. In the event that the return type is an interface
        /// be sure to supply any additional concrete types so that they may be included in the object graph.</param>
        /// <param name="additionalTypes">Any additional types to include in the object graph on behalf of this method.</param>
        public SubscriptionAttribute(string eventName, string template, Type returnType, params Type[] additionalTypes)
            : base(false, GraphCollection.Subscription, template, returnType.AsEnumerable().Concat(additionalTypes).ToArray())
        {
            this.EventName = eventName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionAttribute" /> class.
        /// </summary>
        /// <param name="eventName">The schema-unique name of the event that published whenever
        /// subscribed clients should receive new data.</param>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        /// <param name="unionTypeName">Name of the union type.</param>
        /// <param name="unionTypeA">The first of two required types to include in the union.</param>
        /// <param name="unionTypeB">The second of two required types to include in the union.</param>
        /// <param name="additionalUnionTypes">Any additional union types to include.</param>
        public SubscriptionAttribute(string eventName, string template, string unionTypeName, Type unionTypeA, Type unionTypeB, params Type[] additionalUnionTypes)
         : base(
               false,
               GraphCollection.Subscription,
               template,
               unionTypeName,
               unionTypeA.AsEnumerable().Concat(unionTypeB.AsEnumerable()).Concat(additionalUnionTypes).ToArray())
        {
            this.EventName = eventName;
        }

        /// <summary>
        /// Gets the schema-unique name of the event that should be raised by the runtime
        /// whenever a object should be sent to clients subscribed to this graph route.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; }
    }
}