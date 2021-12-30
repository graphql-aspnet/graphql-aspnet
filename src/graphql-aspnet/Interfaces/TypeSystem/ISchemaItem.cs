﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Internal.TypeTemplates;

    /// <summary>
    /// An item that is part of a <see cref="ISchema"/>.
    /// </summary>
    public interface ISchemaItem
    {
        /// <summary>
        /// Gets the formal name of this item as it exists in the schema.
        /// </summary>
        /// <value>The publically referenced name of this field in the graph.</value>
        string Name { get; }

        /// <summary>
        /// Gets the human-readable description distributed with this item
        /// when requested. The description should accurately describe the contents of this field
        /// to consumers.
        /// </summary>
        /// <value>The publically referenced description of this field in the type system.</value>
        string Description { get; }

        /// <summary>
        /// Gets a collection of directives applied to this schema item
        /// when it was instantiated in a schema.
        /// </summary>
        /// <value>The directives.</value>
        IAppliedDirectiveCollection AppliedDirectives { get; }
    }
}