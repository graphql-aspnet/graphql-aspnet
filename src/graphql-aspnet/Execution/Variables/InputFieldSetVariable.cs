﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Variables
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Middleware.DirectiveExecution.Components;

    /// <summary>
    /// A variable defined as a set of child key/value pairs
    /// (such as those destined to populate an INPUT_OBJECT).
    /// </summary>
    [DebuggerDisplay("InputFieldSet: {Name} (Count = {Fields.Count})")]
    internal class InputFieldSetVariable : InputVariable, IInputFieldSetVariable, IWritableInputFieldSetVariable, IResolvableFieldSet
    {
        private readonly Dictionary<string, IInputVariable> _fields;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputFieldSetVariable"/> class.
        /// </summary>
        /// <param name="name">The name of the variable as defined by the user.</param>
        public InputFieldSetVariable(string name)
            : base(name)
        {
            _fields = new Dictionary<string, IInputVariable>();
        }

        /// <summary>
        /// Adds the new variable as a field of this field set variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public void AddVariable(IInputVariable variable)
        {
            _fields.Add(variable.Name, variable);
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IInputVariable> Fields => _fields;

        /// <inheritdoc />
        public bool TryGetField(string fieldName, out IResolvableValueItem field)
        {
            field = null;
            var found = _fields.TryGetValue(fieldName, out var item);
            if (found)
                field = item;

            return found;
        }

        /// <inheritdoc />
        public void Replace(string fieldName, IInputVariable newValue)
        {
            Validation.ThrowIfNull(fieldName, nameof(fieldName));
            Validation.ThrowIfNull(newValue, nameof(newValue));

            if (!_fields.ContainsKey(fieldName))
                throw new KeyNotFoundException(fieldName);

            _fields[fieldName] = newValue;
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, IResolvableValueItem>> ResolvableFields
        {
            get
            {
                foreach (var kvp in this.Fields)
                {
                    yield return new KeyValuePair<string, IResolvableValueItem>(kvp.Key, kvp.Value);
                }
            }
        }
    }
}