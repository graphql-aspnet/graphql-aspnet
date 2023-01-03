// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Source
{
    using System;

    /// <summary>
    /// A complete represention (location and path) of a place within a query's source text.
    /// </summary>
    [Serializable]
    public readonly struct SourceOrigin
    {
        /// <summary>
        /// Gets a singleton origin point representing no point on an graph query.
        /// </summary>
        /// <value>The none.</value>
        public static SourceOrigin None { get; } = new SourceOrigin(SourceLocation.None, SourcePath.None);

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceOrigin"/> struct.
        /// </summary>
        /// <param name="path">the field path int he source text represented by this origin location.</param>
        public SourceOrigin(SourcePath path)
            : this(SourceLocation.None, path)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceOrigin"/> struct.
        /// </summary>
        /// <param name="location">the indexed location in the source text represented by this origin..</param>
        public SourceOrigin(SourceLocation location)
            : this(location, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceOrigin" /> struct.
        /// </summary>
        /// <param name="location">the indexed location in the source text represented by this origin..</param>
        /// <param name="path">the field path int he source text represented by this origin location.</param>
        public SourceOrigin(SourceLocation location, SourcePath path)
        {
            this.Location = location;
            this.Path = path ?? SourcePath.None;
        }

        /// <summary>
        /// Gets the indexed location in the source text represented by this origin.
        /// </summary>
        /// <value>The location.</value>
        public SourceLocation Location { get; }

        /// <summary>
        /// Gets the field path int he source text represented by this origin location.
        /// </summary>
        /// <value>The path.</value>
        public SourcePath Path { get; }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            if (this.Path.Count > 0)
                return this.Path.ToString();

            return this.Location.ToString();
        }
    }
}