// *************************************************************
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
    using GraphQL.AspNet.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A declaration of the usage of a single graph type (with appropriate wrappers).
    /// This is an object represention of the field and variable declaration
    /// schema syntax (e.g. '[SomeType]!').
    /// </summary>
    [Serializable]
    public partial class GraphTypeExpression
    {
        /// <summary>
        /// Parses an expression in syntax definition language (e.g. '[SomeType!]!') into a usable <see cref="GraphTypeExpression" />.
        /// </summary>
        /// <param name="typeExpression">The type expression.</param>
        /// <returns>GraphTypeDeclaration.</returns>
        public static GraphTypeExpression FromDeclaration(string typeExpression)
        {
            ReadOnlySpan<char> data = ReadOnlySpan<char>.Empty;
            if (!string.IsNullOrWhiteSpace(typeExpression))
                data = typeExpression.Trim().AsSpan();

            return FromDeclaration(data);
        }

        /// <summary>
        /// Parses an expression in syntax definition language (e.g. '[SomeType!]!') into a usable <see cref="GraphTypeExpression" />.
        /// </summary>
        /// <param name="typeExpression">The type expression to parse.</param>
        /// <returns>GraphTypeDeclaration.</returns>
        public static GraphTypeExpression FromDeclaration(ReadOnlySpan<char> typeExpression)
        {
            if (typeExpression.IsEmpty)
                return GraphTypeExpression.Invalid;

            // inspect the first and last characters for type modifiers and extract as necessary
            if (typeExpression[typeExpression.Length - 1] == TokenTypeNames.BANG)
            {
                var ofType = FromDeclaration(typeExpression.Slice(0, typeExpression.Length - 1));

                // the expression 'SomeType!!' is invalid.
                if (ofType.Wrappers.Any() && ofType.Wrappers[0] == MetaGraphTypes.IsNotNull)
                    return GraphTypeExpression.Invalid;

                ofType.WrapExpression(MetaGraphTypes.IsNotNull);
                return ofType;
            }

            if (typeExpression[0] == TokenTypeNames.BRACKET_LEFT)
            {
                // must be at a minimum  '[a]'
                if (typeExpression.Length < 3 || typeExpression[typeExpression.Length - 1] != TokenTypeNames.BRACKET_RIGHT)
                    return GraphTypeExpression.Invalid;

                var ofType = FromDeclaration(typeExpression.Slice(1, typeExpression.Length - 2));
                ofType.WrapExpression(MetaGraphTypes.IsList);
                return ofType;
            }

            // account for an erronous close bracket, dont parse for anything else at this stage
            // meaning typename could be 'BOB![SMITH' and be expected in this method.
            if (typeExpression[typeExpression.Length - 1] == TokenTypeNames.BRACKET_RIGHT)
                return GraphTypeExpression.Invalid;

            return new GraphTypeExpression(typeExpression.ToString());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeExpression" /> class.
        /// </summary>
        /// <param name="typeName">Name of the core type.</param>
        /// <param name="wrappers">A collection of wrappers (from outermost to inner most) to apply to this expression.</param>
        public GraphTypeExpression(string typeName, IEnumerable<MetaGraphTypes> wrappers)
        {
            this.TypeName = typeName;
            Wrappers = wrappers?.ToArray() ?? new MetaGraphTypes[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeExpression" /> class.
        /// </summary>
        /// <param name="typeName">Name of the core type.</param>
        /// <param name="wrappers">A collection of wrappers (from outermost to inner most) to apply to this expression.</param>
        public GraphTypeExpression(string typeName, params MetaGraphTypes[] wrappers)
        {
            this.TypeName = typeName;
            Wrappers = wrappers?.ToArray() ?? new MetaGraphTypes[0];
        }

        /// <summary>
        /// Wraps this instance with a new wrapper.
        /// </summary>
        /// <param name="wrapper">The new modifier.</param>
        public void WrapExpression(MetaGraphTypes wrapper)
        {
            var newArray = new MetaGraphTypes[Wrappers.Length + 1];
            Wrappers.CopyTo(newArray, 1);
            newArray[0] = wrapper;
            Wrappers = newArray;
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
        /// Clones this expression but with a new, core graph type name.
        /// </summary>
        /// <param name="graphTypeName">The new graph type name.</param>
        /// <returns>GraphTypeExpression.</returns>
        public GraphTypeExpression CloneTo(string graphTypeName)
        {
            return new GraphTypeExpression(graphTypeName, this.Wrappers);
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
        /// MUST be provided (e.g. it cannot be null).
        /// </summary>
        /// <value><c>true</c> if this instance is required; otherwise, <c>false</c>.</value>
        public bool IsRequired => !this.IsNullable;

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