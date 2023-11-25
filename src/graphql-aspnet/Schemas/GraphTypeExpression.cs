﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A declaration of the usage of a single graph type (with appropriate wrappers).
    /// This is an object represention of the field and variable declaration
    /// syntax used by GraphQL (e.g. '[SomeType]!').
    /// </summary>
    [Serializable]
    public partial class GraphTypeExpression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeExpression" /> class.
        /// </summary>
        /// <param name="typeName">Name of the core type.</param>
        /// <param name="wrappers">A collection of wrappers (from outermost to inner most) to apply to this expression.</param>
        public GraphTypeExpression(string typeName, IEnumerable<MetaGraphTypes> wrappers)
        {
            this.TypeName = typeName;
            this.Wrappers = wrappers?.ToArray() ?? new MetaGraphTypes[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeExpression" /> class.
        /// </summary>
        /// <param name="typeName">Name of the core type.</param>
        /// <param name="wrappers">A collection of wrappers (from outermost to inner most) to apply to this expression.</param>
        public GraphTypeExpression(string typeName, params MetaGraphTypes[] wrappers)
        {
            this.TypeName = typeName;
            this.Wrappers = wrappers?.ToArray() ?? new MetaGraphTypes[0];
        }

        /// <summary>
        /// Wraps this instance with a new wrapper.
        /// </summary>
        /// <param name="wrapper">The new modifier.</param>
        /// <returns>The new type expression wrapped in the provided value.</returns>
        public GraphTypeExpression WrapExpression(MetaGraphTypes wrapper)
        {
            var newArray = new MetaGraphTypes[Wrappers.Length + 1];
            this.Wrappers.CopyTo(newArray, 1);
            newArray[0] = wrapper;
            return new GraphTypeExpression(this.TypeName, newArray);
        }

        /// <summary>
        /// Validates that the supplied object represents a type chain that can be represented by the required modifiers on this
        /// expression. This method, by default, only validates list and nullability requirements it DOES NOT validate the core object reference.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="matchFunction">An additional function that, if supplied, will be called for each core value unwrapped
        /// during validation for additional custom processing. Return true or false from this function to indicate
        /// if the unwrapped value matches the required additional validation.</param>
        /// <returns><c>true</c> if the type matches this declaration, <c>false</c> otherwise.</returns>
        public bool Matches(object value, Func<object, bool> matchFunction = null)
        {
            return this.MatchAndExtract(value, new Span<MetaGraphTypes>(this.Wrappers), matchFunction);
        }

        /// <summary>
        /// Matches values and unwraps meta types as necessary. If/when a core value is found it is sent back through
        /// the optional provided action for additional processing if needed.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="modifiers">The modifiers.</param>
        /// <param name="rootValueFound">The root value found.</param>
        /// <returns><c>true</c> if the value matches the modifer collection, <c>false</c> otherwise.</returns>
        private bool MatchAndExtract(object value, Span<MetaGraphTypes> modifiers, Func<object, bool> rootValueFound)
        {
            while (true)
            {
                // no modifiers/restrictions? then anything is valid
                if (modifiers.IsEmpty)
                {
                    if (rootValueFound != null)
                        return rootValueFound.Invoke(value);
                    return true;
                }

                // is the result not to be null?
                if (modifiers[0] == MetaGraphTypes.IsNotNull)
                {
                    if (value == null)
                        return false;

                    modifiers = modifiers.Slice(1);
                    continue;
                }

                // null is valid at this point and no processing can occur
                if (value == null)
                    return true;

                // should the value be a list of items?
                if (modifiers[0] == MetaGraphTypes.IsList)
                {
                    // a string is IEnumerable<char> but for the sake of graphql it is NOT a
                    // 'list' type
                    if (value is IEnumerable list && value.GetType() != typeof(string))
                    {
                        if (modifiers.Length > 1)
                        {
                            // check each child item in the enumeration against the next level down
                            var childModifers = modifiers.Slice(1);
                            foreach (var item in list)
                            {
                                if (!this.MatchAndExtract(item, childModifers, rootValueFound))
                                    return false;
                            }
                        }

                        return true;
                    }

                    return false;
                }

                throw new ArgumentOutOfRangeException(
                    nameof(modifiers),
                    $"Invalid {typeof(MetaGraphTypes)} when attempting to validate an object.");
            }
        }

        /// <summary>
        /// <para>
        /// Inspects the supplied type expression and determines that if the structure of the
        /// type expression is compatiable with this instance.
        /// </para>
        /// <para>
        /// Two type expressions are considered structurally compatiable if their type names are the same
        /// and their list modifiers are the same. Whether a list or type is nullable is not
        /// considered.
        /// </para>
        /// <para>
        /// Example:   <br />
        /// [[Type]] and [[Type!]!] ARE structurally compatiable.<br/>
        /// [[Type]] and [Type] ARE NOT structurally compatiable.<br/>
        /// </para>
        /// </summary>
        /// <param name="typeExpression">The updated type expression.</param>
        /// <returns><c>true</c> if [is structrual match] [the specified updated type expression]; otherwise, <c>false</c>.</returns>
        public bool IsStructruallyCompatiable(GraphTypeExpression typeExpression)
        {
            Validation.ThrowIfNull(typeExpression, nameof(typeExpression));

            if (typeExpression.TypeName != this.TypeName)
                return false;

            var left = this;
            var right = typeExpression;

            while (left.IsNonNullable)
                left = left.UnWrapExpression();

            while (right.IsNonNullable)
                right = right.UnWrapExpression();

            if (left.IsListOfItems && right.IsListOfItems)
            {
                left = left.UnWrapExpression();
                right = right.UnWrapExpression();

                return left.IsStructruallyCompatiable(right);
            }

            if (left.IsListOfItems || right.IsListOfItems)
                return false;

            return true;
        }

        /// <summary>
        /// Clones this instance into a new copy of itself.
        /// </summary>
        /// <returns>GraphTypeExpression.</returns>
        public GraphTypeExpression Clone()
        {
            return new GraphTypeExpression(this.TypeName, this.Wrappers);
        }

        /// <summary>
        /// Clones this instance into a new copy of itself.
        /// </summary>
        /// <param name="nullabilityStrategy">The nullability strategy to apply to the
        /// cloned instance.</param>
        /// <returns>GraphTypeExpression.</returns>
        public GraphTypeExpression Clone(GraphTypeExpressionNullabilityStrategies nullabilityStrategy)
        {
            return this.Clone(this.TypeName, nullabilityStrategy);
        }

        /// <summary>
        /// Clones this expression but with a new, core graph type name.
        /// </summary>
        /// <param name="graphTypeName">A new graph type name to apply to the cloned instance.</param>
        /// <param name="nullabilityStrategy">The nullability strategy to apply to the
        /// cloned instance.</param>
        /// <returns>GraphTypeExpression.</returns>
        public GraphTypeExpression Clone(string graphTypeName, GraphTypeExpressionNullabilityStrategies nullabilityStrategy = GraphTypeExpressionNullabilityStrategies.None)
        {
            graphTypeName = Validation.ThrowIfNullWhiteSpaceOrReturn(graphTypeName, nameof(graphTypeName));

            var wrappers = this.Wrappers.ToList();

            if (nullabilityStrategy.HasFlag(GraphTypeExpressionNullabilityStrategies.NonNullLists))
            {
                if (wrappers.Count > 0)
                {
                    var wrappersNew = new List<MetaGraphTypes>();
                    if (wrappers[0] == MetaGraphTypes.IsList)
                        wrappersNew.Add(MetaGraphTypes.IsNotNull);

                    wrappersNew.Add(wrappers[0]);

                    for (var i = 1; i < wrappers.Count; i++)
                    {
                        var prevWrapper = wrappers[i - 1];
                        var thisWrapper = wrappers[i];

                        // ensure every list is prefixed with a not-null
                        if (thisWrapper == MetaGraphTypes.IsList
                            && prevWrapper != MetaGraphTypes.IsNotNull)
                        {
                            wrappersNew.Add(MetaGraphTypes.IsNotNull);
                        }

                        wrappersNew.Add(thisWrapper);
                    }

                    wrappers = wrappersNew;
                }
            }

            if (nullabilityStrategy.HasFlag(GraphTypeExpressionNullabilityStrategies.NonNullType))
            {
                if (wrappers.Count == 0
                  || wrappers[wrappers.Count - 1] != MetaGraphTypes.IsNotNull)
                {
                    wrappers.Add(MetaGraphTypes.IsNotNull);
                }
            }

            return new GraphTypeExpression(graphTypeName, wrappers);
        }

        /// <summary>
        /// Generates a new copy of this type expression removing the outer most layer of required wrappers.
        /// e.g.  [IsList, IsNull, IsList] becomes [IsNull, IsList].
        /// </summary>
        /// <param name="wrapperToRemove">If supplied, type expression wrappers will strip until
        /// the given wrapper is encountered, which is also stripped. That which remains is returned.</param>
        /// <returns>GraphTypeExpression.</returns>
        public GraphTypeExpression UnWrapExpression(MetaGraphTypes? wrapperToRemove = null)
        {
            if (this.Wrappers.Length == 0)
                return new GraphTypeExpression(this.TypeName);

            if (!wrapperToRemove.HasValue)
                return new GraphTypeExpression(this.TypeName, this.Wrappers.Skip(1));

            var wrappers = this.Wrappers.AsSpan();
            while (wrappers.Length > 0)
            {
                var wrapper0 = wrappers[0];
                wrappers = wrappers.Slice(1);
                if (wrapper0 == wrapperToRemove.Value)
                    break;
            }

            return new GraphTypeExpression(this.TypeName, wrappers.ToArray());
        }

        /// <summary>
        /// <para>Gets the ordered set of modifiers that express the wrapping conditions of this type expression with
        /// index 0 representing the outer most wrapped set. </para>
        /// <para>Example: [[SomeType!]] => [IsList, IsList, NotNull].</para>
        /// </summary>
        /// <value>The type modifiers.</value>
        public MetaGraphTypes[] Wrappers { get; private set; }

        /// <summary>
        /// Gets the core name of the graph type. This value may or may not represent a real graph type
        /// depending on usage and declaration location.
        /// </summary>
        /// <value>The name of the type.</value>
        public string TypeName { get; }

        /// <summary>
        /// Gets a value indicating whether this expression is valid and represents a possible type name.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid => !string.IsNullOrWhiteSpace(this.TypeName);

        /// <summary>
        /// Gets a value indicating whether this type expression represents a value that
        /// MUST be not null.
        /// </summary>
        /// <value><c>true</c> if this instance is not nullable; otherwise, <c>false</c>.</value>
        public bool IsNonNullable => !this.IsNullable;

        /// <summary>
        /// Gets a value indicating whether this type expression represents a value that, as a whole, can be null.
        /// </summary>
        /// <value><c>true</c> if this instance is nullable; otherwise, <c>false</c>.</value>
        public bool IsNullable => this.Wrappers.Length == 0 || this.Wrappers[0] != MetaGraphTypes.IsNotNull;

        /// <summary>
        /// Gets a value indicating whether this type expression represents a value that is a list of other items.
        /// </summary>
        /// <value><c>true</c> if this instance is list; otherwise, <c>false</c>.</value>
        public bool IsListOfItems => (this.Wrappers.Length > 0 && this.Wrappers[0] == MetaGraphTypes.IsList) ||
                                     (this.Wrappers.Length > 1 && this.Wrappers[1] == MetaGraphTypes.IsList);

        /// <summary>
        /// Determines whether the specified <see cref="GraphTypeExpression" /> is equal to this instance.
        /// </summary>
        /// <param name="expression">The expression to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="GraphTypeExpression" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(GraphTypeExpression expression)
        {
            if (expression == null)
                return false;

            return this.TypeName == expression.TypeName &&
                   this.Wrappers.SequenceEqual(expression.Wrappers);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is GraphTypeExpression gte)
                return this.Equals(gte);

            return this.ToString() == obj.ToString();
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance. (i.e. '[SomeType!]').
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            if (!this.IsValid)
                return string.Empty;

            var builder = new StringBuilder();
            builder.Append(this.TypeName);

            for (var i = this.Wrappers.Length - 1; i >= 0; i--)
            {
                switch (this.Wrappers[i])
                {
                    case MetaGraphTypes.IsNotNull:
                        builder.Append(TokenTypeNames.BANG);
                        break;

                    case MetaGraphTypes.IsList:
                        builder.Insert(0, TokenTypeNames.BRACKET_LEFT);
                        builder.Append(TokenTypeNames.BRACKET_RIGHT);
                        break;
                }
            }

            return builder.ToString();
        }
    }
}