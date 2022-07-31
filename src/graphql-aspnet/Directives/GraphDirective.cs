﻿// *************************************************************
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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A base class from which all directives mush inherit.
    /// </summary>
    public abstract partial class GraphDirective : GraphControllerBase<IGraphDirectiveRequest>
    {
        private DirectiveResolutionContext _context;

        /// <inheritdoc />
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
        /// has been applied.
        /// </summary>
        /// <value>The currently active directive location.</value>
        [GraphSkip]
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
        /// <remarks>
        /// This value will be an <see cref="ISchemaItem"/> for type system directive locations
        /// or an <see cref="IDocumentPart"/> for execution directive locations.
        /// </remarks>
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