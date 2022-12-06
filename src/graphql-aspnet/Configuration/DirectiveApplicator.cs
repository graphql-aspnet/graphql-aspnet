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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A class that encapsulates the late binding of a directive to a schema item
    /// and relavant fields there in.
    /// </summary>
    public sealed class DirectiveApplicator : ISchemaConfigurationExtension
    {
        // a set of default filters applied to any directive applicator unless explicitly removed
        // by the developer. Used to auto filter items down to those reasonably assumed
        // to be included by the developer, the classes and items they have defined not those defined
        // or maintained by the library
        private static IReadOnlyList<Func<ISchemaItem, bool>> _defaultFilters;

        /// <summary>
        /// Initializes static members of the <see cref="DirectiveApplicator"/> class.
        /// </summary>
        static DirectiveApplicator()
        {
            var list = new List<Func<ISchemaItem, bool>>();

            // auto remove introspection data, system level items and any virtual items added to the graph
            list.Add(x => !x.IsIntrospectionItem());
            list.Add(x => !x.IsSystemItem());
            list.Add(x => !x.IsDirective());

            // allow graph operations even through they are technically virtual
            list.Add(x => !x.IsVirtualItem() || (x is IGraphOperation));

            _defaultFilters = list;
        }

        private Type _directiveType;
        private string _directiveName;
        private Func<ISchemaItem, object[]> _argumentFunction;
        private List<Func<ISchemaItem, bool>> _customFilters;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveApplicator"/> class.
        /// </summary>
        /// <param name="directiveType">Type of the directive being applied in this instnace.</param>
        public DirectiveApplicator(Type directiveType)
            : this()
        {
            _directiveType = Validation.ThrowIfNullOrReturn(directiveType, nameof(directiveType));
            Validation.ThrowIfNotCastable<GraphDirective>(_directiveType, nameof(directiveType));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveApplicator" /> class.
        /// </summary>
        /// <param name="directiveName">Name of the directive as it is declared in the schema
        /// where it is being applied.</param>
        public DirectiveApplicator(string directiveName)
            : this()
        {
            _directiveName = Validation.ThrowIfNullWhiteSpaceOrReturn(directiveName, nameof(directiveName));
        }

        private DirectiveApplicator()
        {
            _customFilters = new List<Func<ISchemaItem, bool>>();
            this.WithArguments();
        }

        /// <inheritdoc />
        void ISchemaConfigurationExtension.Configure(ISchema schema)
        {
            var allFilters = _defaultFilters.Concat(_customFilters).ToList();
            foreach (var schemaItem in schema.AllSchemaItems(includeDirectives: false))
            {
                // ensure the current item matches all supplied filters
                var shouldBeApplied = true;
                foreach (var filter in allFilters)
                {
                    if (!filter(schemaItem))
                    {
                        shouldBeApplied = false;
                        break;
                    }
                }

                if (!shouldBeApplied)
                    continue;

                // calculate the arguments for this schema item
                var arguments = _argumentFunction(schemaItem);
                if (arguments == null)
                    arguments = new object[0];

                // bind the directive to the schema item
                IAppliedDirective appliedDirective;
                if (_directiveType != null)
                    appliedDirective = new AppliedDirective(_directiveType, arguments);
                else
                    appliedDirective = new AppliedDirective(_directiveName, arguments);

                schemaItem.AppliedDirectives.Add(appliedDirective);
            }
        }

        /// <summary>
        /// Assigns a static set of arguments to all applications of this directive.
        /// </summary>
        /// <remarks>
        /// If called more than once, the last set of arguments will be kept and the others discarded.
        /// </remarks>
        /// <param name="arguments">The arguments to supply to the directive when its
        /// executed.</param>
        /// <returns>IDirectiveInjector.</returns>
        public DirectiveApplicator WithArguments(params object[] arguments)
        {
            arguments = arguments ?? new object[0];
            _argumentFunction = x => arguments;
            return this;
        }

        /// <summary>
        /// Assigns a static set of arguments to all applications of this directive.
        /// </summary>
        /// <remarks>
        /// If called more than once, the last set of arguments will be kept and the others discarded.
        /// </remarks>
        /// <param name="argsCreator">A function that will be used to
        /// create a new unique set of arguments per schema item the directive is applied to.</param>
        /// <returns>IDirectiveInjector.</returns>
        public DirectiveApplicator WithArguments(Func<ISchemaItem, object[]> argsCreator)
        {
            Validation.ThrowIfNull(argsCreator, nameof(argsCreator));
            _argumentFunction = argsCreator;
            return this;
        }

        /// <summary>
        /// Adds the given filter function to the applicator. Only <see cref="ISchemaItem"/>
        /// that match this filter will receive the directive.
        /// </summary>
        /// <remarks>
        /// Every call to this method adds an additional filter. Only items that match ALL
        /// filters will receive the directive.
        /// </remarks>
        /// <param name="itemFilter">The item filter.</param>
        /// <returns>DirectiveApplicator.</returns>
        public DirectiveApplicator ToItems(Func<ISchemaItem, bool> itemFilter)
        {
            Validation.ThrowIfNull(itemFilter, nameof(itemFilter));
            _customFilters.Add(itemFilter);
            return this;
        }

        /// <summary>
        /// Clears this instance of any directive arguments and filter criteria.
        /// </summary>
        /// <returns>DirectiveApplicator.</returns>
        public DirectiveApplicator Clear()
        {
            _customFilters.Clear();
            this.WithArguments();
            return this;
        }
    }
}