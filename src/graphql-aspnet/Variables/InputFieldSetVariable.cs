// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Variables
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A variable defined as a set of child key/value pair.
    /// </summary>
    [DebuggerDisplay("InputFieldSet: {Name} (Count = {Fields.Count})")]
    public class InputFieldSetVariable : InputVariable, IInputFieldSetVariable, IResolvableFieldSet
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

        /// <summary>
        /// Gets the dictionary of fields defined for this field set variable.
        /// </summary>
        /// <value>The fields.</value>
        public IReadOnlyDictionary<string, IInputVariable> Fields => _fields;

        /// <summary>
        /// Attempts to retrieve a field by its name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="field">The field that was found, if any.</param>
        /// <returns><c>true</c> if the field was found and successfully returned, <c>false</c> otherwise.</returns>
        bool IResolvableFieldSet.TryGetField(string fieldName, out IResolvableItem field)
        {
            field = null;
            var found = _fields.TryGetValue(fieldName, out var item);
            if (found)
                field = item;

            return found;
        }

        /// <summary>
        /// Gets the dictionary of fields defined for this field set variable.
        /// </summary>
        /// <value>The fields.</value>
        IEnumerable<KeyValuePair<string, IResolvableItem>> IResolvableFieldSet.Fields
        {
            get
            {
                foreach (var kvp in this.Fields)
                {
                    yield return new KeyValuePair<string, IResolvableItem>(kvp.Key, kvp.Value);
                }
            }
        }
    }
}