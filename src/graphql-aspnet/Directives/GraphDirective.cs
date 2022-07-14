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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A base class from which all directives mush inherit.
    /// </summary>
    public abstract partial class GraphDirective : GraphControllerBase<IGraphDirectiveRequest>
    {
        private DirectiveResolutionContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirective"/> class.
        /// </summary>
        protected GraphDirective()
        {
        }

        /// <inheritdoc />
        [GraphSkip]
        internal override Task<object> InvokeActionAsync(
            IGraphMethod actionToInvoke,
            BaseResolutionContext<IGraphDirectiveRequest> context)
        {
            Validation.ThrowIfNull(context, nameof(context));
            Validation.ThrowIfNotCastable<DirectiveResolutionContext>(context.GetType(), nameof(context));

            _context = context as DirectiveResolutionContext;
            return base.InvokeActionAsync(actionToInvoke, context);
        }

        /// <summary>
        /// Gets the current phase of execution this directive is processing.
        /// </summary>
        /// <value>The directive phase.</value>
        [GraphSkip]
        public DirectiveInvocationPhase DirectivePhase
        {
            get
            {
                return _context?.Request?.DirectivePhase ?? DirectiveInvocationPhase.Unknown;
            }
        }

        /// <summary>
        /// Gets the location in the active query document or the type system where this directive
        /// is executing from.
        /// </summary>
        /// <value>The currently active directive location.</value>
        public DirectiveLocation DirectiveLocation
        {
            get
            {
                return _context?.Request?.InvocationContext?.Location ?? DirectiveLocation.NONE;
            }
        }

        /// <summary>
        /// Gets or sets the object that is the target of this directive. If altered
        /// the new value will be persisted.
        /// </summary>
        /// <value>The directive target.</value>
        [GraphSkip]
        public object DirectiveTarget
        {
            get
            {
                return _context?.Request?.DirectiveTarget;
            }

            set
            {
                if (_context?.Request != null)
                    _context.Request.DirectiveTarget = value;
            }
        }
    }
}