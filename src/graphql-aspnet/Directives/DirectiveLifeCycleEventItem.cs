// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Directives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An ecapsulation of all the data related to one directive lifecycle event.
    /// Acts as a foundation for validating any potential candidate method.
    /// </summary>
    internal class DirectiveLifeCycleEventItem
    {
        /// <summary>
        /// Gets the life cycle event representing nothing or no event.
        /// </summary>
        /// <value>The none.</value>
        public static DirectiveLifeCycleEventItem None { get; }

        /// <summary>
        /// Initializes static members of the <see cref="DirectiveLifeCycleEventItem"/> class.
        /// </summary>
        static DirectiveLifeCycleEventItem()
        {
            None = new DirectiveLifeCycleEventItem();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DirectiveLifeCycleEventItem"/> class from being created.
        /// </summary>
        private DirectiveLifeCycleEventItem()
        {
            this.InvocableLocations = DirectiveLocation.NONE;
            this.Event = DirectiveLifeCycleEvent.Unknown;
            this.Phase = DirectiveLifeCyclePhase.Unknown;
            this.MethodName = string.Empty;
            this.SyblingEvents = new List<DirectiveLifeCycleEventItem>();
            this.HasRequiredSignature = false;
            this.Parameters = new List<Type>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveLifeCycleEventItem" /> class.
        /// </summary>
        /// <param name="locations">The locations where this event may be invoked.</param>
        /// <param name="evt">The event value representing this item.</param>
        /// <param name="phase">The phase this event occurs in.</param>
        /// <param name="methodName">Name of the method serviced by this event item.</param>
        /// <param name="requiredMethodParameters">The required method parameters.</param>
        internal DirectiveLifeCycleEventItem(
            DirectiveLocation locations,
            DirectiveLifeCycleEvent evt,
            DirectiveLifeCyclePhase phase,
            string methodName,
            IEnumerable<Type> requiredMethodParameters = null)
        {
            if (locations == DirectiveLocation.NONE)
                throw new ArgumentException("At least one known location must be supplied", nameof(locations));
            if (evt == DirectiveLifeCycleEvent.Unknown)
                throw new ArgumentException("the 'Unknown' event enumeration value is not allowed", nameof(evt));
            if (phase == DirectiveLifeCyclePhase.Unknown)
                throw new ArgumentException("the 'Unknown' phase enumeration value is not allowed", nameof(phase));

            this.InvocableLocations = locations;
            this.Event = evt;
            this.Phase = phase;
            this.MethodName = Validation.ThrowIfNullEmptyOrReturn(methodName, nameof(methodName));
            this.SyblingEvents = new List<DirectiveLifeCycleEventItem>();
            this.HasRequiredSignature = requiredMethodParameters != null;
            this.Parameters = requiredMethodParameters?.ToList() ?? new List<Type>();
        }

        /// <summary>
        /// Adds the item as a sylbing of this instance. Does not add this
        /// item to the provided sylbing.
        /// </summary>
        /// <param name="sylbingItem">The sylbing item to assign to this item.</param>
        internal void AddSybling(DirectiveLifeCycleEventItem sylbingItem)
        {
            this.SyblingEvents.Add(sylbingItem);
        }

        /// <summary>
        /// Gets the set of events that are considered syblings of this
        /// event. All sylbings must have identical signatures.
        /// </summary>
        /// <value>The sybling events.</value>
        public IList<DirectiveLifeCycleEventItem> SyblingEvents { get; }

        /// <summary>
        /// Gets the singular event represented by this item.
        /// </summary>
        /// <value>The event.</value>
        public DirectiveLifeCycleEvent Event { get; }

        /// <summary>
        /// Gets the lifecycle phase during which this <see cref="Event"/>
        /// occurs.
        /// </summary>
        /// <value>The phase.</value>
        public DirectiveLifeCyclePhase Phase { get; }

        /// <summary>
        /// Gets the set of bitwise directive locations where this event
        /// would be invocable.
        /// </summary>
        /// <value>The allowed locations.</value>
        public DirectiveLocation InvocableLocations { get; }

        /// <summary>
        /// Gets a dictionary, keyed on the names of acceptable methods,
        /// containing a list of required types in the method signature.
        /// </summary>
        /// <value>The method names.</value>
        public string MethodName { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has required signature. When
        /// true the type <see cref="Parameters"/> must be declared in the order
        /// encountered for the method to be acceptable.
        /// </summary>
        /// <value><c>true</c> if this instance has required signature; otherwise, <c>false</c>.</value>
        public bool HasRequiredSignature { get; }

        /// <summary>
        /// Gets the parameters, in order of appearance, that must be supplied.
        /// </summary>
        /// <value>The parameters.</value>
        public IList<Type> Parameters { get; }

        /// <summary>
        /// Gets the expected return type of the method that employs this event.
        /// </summary>
        /// <value>The expected type of the return.</value>
        public Type ExpectedReturnType => typeof(IGraphActionResult);
    }
}