// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.Json.Nodes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Exceptions;

    /// <summary>
    /// Helper methods for use when parsing json nodes on the multipart request
    /// extension.
    /// </summary>
    internal static class JsonNodeExtensions
    {
        /// <summary>
        /// Determines whether the specified node is really an array node.
        /// </summary>
        /// <param name="node">The node to inspect.</param>
        /// <returns><c>true</c> if the specified node is an array; otherwise, <c>false</c>.</returns>
        public static bool IsArray(this JsonNode node)
        {
            return node != null && node is JsonArray;
        }

        /// <summary>
        /// Determines whether the specified node is really an object node.
        /// </summary>
        /// <param name="node">The node to inspect.</param>
        /// <returns><c>true</c> if the specified node is an object; otherwise, <c>false</c>.</returns>
        public static bool IsObject(this JsonNode node)
        {
            return node != null && node is JsonObject;
        }

        /// <summary>
        /// Determines whether the specified node is really a single value node.
        /// </summary>
        /// <param name="node">The node to inspect.</param>
        /// <returns><c>true</c> if the specified node is a single value; otherwise, <c>false</c>.</returns>
        public static bool IsValue(this JsonNode node)
        {
            return node != null && node is JsonValue;
        }

        /// <summary>
        /// Using a algorithm similar to `object-path` places the provided <paramref name="value"/> into the node tree
        /// indicated by <paramref name="rootNode"/> assuming the query location represents a currently
        /// null or non-existant value. An exception is thrown is a non-null value already sits at that location.
        /// </summary>
        /// <param name="rootNode">The root node to path into.</param>
        /// <param name="queryPath">A dot seperated string indicating the path segments to navigate within <paramref name="rootNode"/>.</param>
        /// <param name="value">The value to place.</param>
        public static void SetJsonNode(JsonNode rootNode, string queryPath, JsonNode value)
        {
            queryPath = Validation.ThrowIfNullWhiteSpaceOrReturn(queryPath, nameof(queryPath));

            var segments = queryPath.Split('.', StringSplitOptions.RemoveEmptyEntries);
            SetJsonNode(rootNode, segments, value);
        }

        /// <summary>
        /// Using a algorithm similar to `object-path` places the provided <paramref name="value"/> into the node tree
        /// indicated by <paramref name="rootNode"/> assuming the query location represents a currently
        /// null or non-existant value. An exception is thrown is a non-null value already sits at that location.
        /// </summary>
        /// <param name="rootNode">The root node to path into.</param>
        /// <param name="queryPath">A json array of the path segments to navigate within <paramref name="rootNode"/>.</param>
        /// <param name="value">The value to place.</param>
        public static void SetJsonNode(JsonNode rootNode, JsonArray queryPath, JsonNode value)
        {
            Validation.ThrowIfNull(queryPath, nameof(queryPath));

            var items = new List<string>(queryPath.Count);
            foreach (var item in queryPath)
            {
                if (!item.IsValue())
                {
                    throw new JsonNodeException(
                        $"Expected a value representing an array index or " +
                        $"property name. Got '{item.ToJsonString()}' instead.");
                }

                var v = item.AsValue();
                if (v.TryGetValue<string>(out var str))
                {
                    items.Add(str);
                }
                else if (v.TryGetValue<int>(out var i))
                {
                    items.Add(i.ToString());
                }
                else
                {
                    throw new JsonNodeException(
                        $"Expected a value representing an array index or " +
                        $"property name. Got '{v}' instead.");
                }
            }

            SetJsonNode(rootNode, items, value);
        }

        /// <summary>
        /// Using a algorithm similar to `object-path` this method places the provided <paramref name="value"/> into the node tree
        /// indicated by <paramref name="rootNode"/> assuming the pathed location represents a currently
        /// null or non-existant value. An exception is thrown if a non-null value already sits at that location.
        /// </summary>
        /// <param name="rootNode">The root node to path into.</param>
        /// <param name="querySegments">The query path to place the value at.</param>
        /// <param name="value">The value to place.</param>
        public static void SetJsonNode(JsonNode rootNode, IReadOnlyList<string> querySegments, JsonNode value)
        {
            Validation.ThrowIfNull(querySegments, nameof(querySegments));
            if (querySegments.Count < 1)
                throw new JsonNodeException($"At least one segment is required on {nameof(querySegments)}.");

            var currentNode = rootNode;
            for (int i = 0; i < querySegments.Count - 1; i++)
            {
                var segment = querySegments[i];
                int? arrIndex = null;
                JsonNode nextNode = null;

                if (int.TryParse(segment, out var index) && currentNode.IsArray())
                    arrIndex = index;

                if (arrIndex.HasValue)
                {
                    var arr = currentNode.AsArray();
                    while (arr.Count <= arrIndex.Value)
                        arr.Add(null);

                    nextNode = arr[arrIndex.Value];
                }
                else if (currentNode.IsObject())
                {
                    nextNode = currentNode.AsObject()[segment];
                }
                else if (currentNode.IsValue())
                {
                    throw new JsonNodeException(
                        $"Expected an object or array to search for segment '{segment}' (index: {i}) but instead received " +
                        $"a single value");
                }

                if (nextNode == null)
                {
                    // Create a new object or array node if the next segment is not found
                    var nextPieceIsArrayElement = querySegments[i + 1].All(char.IsDigit);

                    nextNode = nextPieceIsArrayElement ? new JsonArray() : new JsonObject();
                    if (arrIndex.HasValue)
                    {
                        var arr = currentNode.AsArray();
                        arr[arrIndex.Value] = nextNode;
                    }
                    else if (currentNode.IsObject())
                    {
                        currentNode[segment] = nextNode;
                    }
                }

                currentNode = nextNode;
            }

            int? finalIndex = null;
            var finalSegment = querySegments[querySegments.Count - 1];
            if (int.TryParse(finalSegment, out var parsedFinalIndex) && currentNode.IsArray())
                finalIndex = parsedFinalIndex;

            // Set the value of the final segment to the specified value
            if (finalIndex.HasValue)
            {
                var arr = currentNode.AsArray();

                while (arr.Count <= finalIndex.Value)
                    arr.Add(null);

                if (arr[finalIndex.Value] != null)
                {
                    throw new JsonNodeException(
                       $"The location indicated by the query path already contains a value, it cannot be replaced.");
                }

                arr[finalIndex.Value] = value;
                return;
            }

            if (currentNode.IsObject())
            {
                if (currentNode[finalSegment] != null)
                {
                    throw new JsonNodeException(
                       $"The location indicated by the query path already contains a value, it cannot be replaced.");
                }

                currentNode[finalSegment] = value;
                return;
            }

            throw new JsonNodeException(
                       $"Expected an object or array to assign the final value but instead received " +
                       $"a single value");
        }
    }
}