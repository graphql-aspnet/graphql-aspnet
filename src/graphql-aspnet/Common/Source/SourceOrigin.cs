// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Source
{
    using System;

    /// <summary>
    /// A complete origin represention of a location within the source text.
    /// </summary>
    [Serializable]
    public class SourceOrigin
    {
        /// <summary>
        /// Gets a singleton origin point representing no point on an graph query.
        /// </summary>
        /// <value>The none.</value>
        public static SourceOrigin None { get; } = new SourceOrigin();

        /// <summary>
        /// Initializes static members of the <see cref="SourceOrigin"/> class.
        /// </summary>
        static SourceOrigin()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceOrigin"/> class.
        /// </summary>
        public SourceOrigin()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceOrigin"/> class.
        /// </summary>
        /// <param name="path">the field path int he source text represented by this origin location.</param>
        public SourceOrigin(SourcePath path)
            : this(null, path)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceOrigin"/> class.
        /// </summary>
        /// <param name="location">the indexed location in the source text represented by this origin..</param>
        public SourceOrigin(SourceLocation location)
            : this(location, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceOrigin" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="path">The path.</param>
        public SourceOrigin(SourceLocation location, SourcePath path)
        {
            this.Location = location ?? SourceLocation.None;
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