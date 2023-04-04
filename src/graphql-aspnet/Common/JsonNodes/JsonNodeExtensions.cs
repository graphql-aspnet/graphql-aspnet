// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Nodes;
    using GraphQL.AspNet.Common;

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
        /// <param name="splitDotSeperatedString">if set to <c>true</c>, when the provide dstring contains a dot seperated
        /// <paramref name="queryPath"/> is provided it is split into seperate elements using the dot as a delimiter.</param>
        public static void SetChildNodeValue(this JsonNode rootNode, string queryPath, JsonNode value, bool splitDotSeperatedString = true)
        {
            queryPath = Validation.ThrowIfNullWhiteSpaceOrReturn(queryPath, nameof(queryPath));

            string[] segments;
            if (splitDotSeperatedString)
                segments = queryPath.Split('.', StringSplitOptions.RemoveEmptyEntries);
            else
                segments = new string[] { queryPath };

            rootNode.SetChildNodeValue(segments, value);
        }

        /// <summary>
        /// Using a algorithm similar to `object-path` places the provided <paramref name="value" /> into the node tree
        /// indicated by <paramref name="rootNode" /> assuming the query location represents a currently
        /// null or non-existant value. An exception is thrown is a non-null value already sits at that location.
        /// </summary>
        /// <param name="rootNode">The root node to path into.</param>
        /// <param name="queryPath">A json array of the path segments to navigate within <paramref name="rootNode" />.</param>
        /// <param name="value">The value to place.</param>
        /// <param name="splitSingleStringElement">if set to <c>true</c>, when an array of one element is provided,
        /// and that element is a string, its treated as if it were passed in solitary and not as an array. This means if
        /// said string contains a dot delimited string that string is parsed and split a second time.</param>
        public static void SetChildNodeValue(this JsonNode rootNode, JsonArray queryPath, JsonNode value, bool splitSingleStringElement = true)
        {
            Validation.ThrowIfNull(queryPath, nameof(queryPath));

            if (queryPath.Count == 1 && splitSingleStringElement)
            {
                if (queryPath[0].IsValue() && queryPath[0].AsValue().TryGetValue<string>(out var singleItem))
                {
                    rootNode.SetChildNodeValue(singleItem, value, true);
                    return;
                }
            }

            var items = new List<string>(queryPath.Count);
            foreach (var item in queryPath)
            {
                if (!item.IsValue())
                {
                    throw new JsonNodeException(
                        $"Expected a value representing an array index or " +
                        $"property name. Got '{item.ToJsonString()}' instead.");
                }

                // each member of the map array must be a property name
                // or an array index (boolean values arent allowed)
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

            rootNode.SetChildNodeValue(items, value);
        }

        /// <summary>
        /// Using a algorithm similar to `object-path` this method places the provided <paramref name="value"/> into the node tree
        /// indicated by <paramref name="rootNode"/> assuming the pathed location represents a currently
        /// null or non-existant value. An exception is thrown if a non-null value already sits at that location.
        /// </summary>
        /// <param name="rootNode">The root node to path into.</param>
        /// <param name="querySegments">The query path to place the value at.</param>
        /// <param name="value">The value to place.</param>
        public static void SetChildNodeValue(this JsonNode rootNode, IReadOnlyList<string> querySegments, JsonNode value)
        {
            Validation.ThrowIfNull(rootNode, nameof(rootNode));
            Validation.ThrowIfNull(querySegments, nameof(querySegments));

            if (rootNode.IsValue())
            {
                throw new JsonNodeException(
                    $"The supplied root node represents a single value but was expected to be " +
                    $"an object or an array.");
            }

            if (querySegments.Count < 1)
                throw new JsonNodeException($"At least one segment is required on {nameof(querySegments)}.");

            var currentNode = rootNode;
            for (int i = 0; i < querySegments.Count - 1; i++)
            {
                JsonNode nextNode = currentNode.FindSubNodeOrThrow(querySegments[i]);

                if (nextNode == null)
                {
                    // Create a new object or array node if the next segment is not found
                    var nextPieceIsArrayElement = querySegments[i + 1].All(char.IsDigit);
                    nextNode = nextPieceIsArrayElement ? new JsonArray() : new JsonObject();

                    currentNode.AddSubNode(querySegments[i], nextNode);
                }

                currentNode = nextNode;
            }

            var nodeAtFinalLocation = currentNode.FindSubNodeOrThrow(querySegments[querySegments.Count - 1]);
            if (nodeAtFinalLocation != null)
            {
                throw new JsonNodeException(
                       $"The location indicated by the query path already contains a value, " +
                       $"it cannot be replaced.");
            }

            currentNode.AddSubNode(querySegments[querySegments.Count - 1], value);
        }

        /// <summary>
        /// Adds the sub node as a child of the provided node. Will properly nest the subnode at the appropriate index
        /// or property name based on the supplied node type.
        /// </summary>
        /// <param name="rootNode">The node to apppend to.</param>
        /// <param name="propertyNameOrIndex">Index of the property name or.</param>
        /// <param name="subNode">The sub node to append.</param>
        public static void AddSubNode(this JsonNode rootNode, string propertyNameOrIndex, JsonNode subNode)
        {
            Validation.ThrowIfNull(rootNode, nameof(rootNode));
            if (rootNode.IsValue())
            {
                throw new JsonNodeException(
                    $"Expected an object or array in which to set a value for segment '{propertyNameOrIndex}' but instead received " +
                    $"a single value");
            }

            int? arrIndex = null;
            if (int.TryParse(propertyNameOrIndex, out var index) && rootNode.IsArray())
                arrIndex = index;

            if (arrIndex.HasValue)
            {
                var arr = rootNode.AsArray();
                arr[arrIndex.Value] = subNode;
                return;
            }

            if (rootNode.IsObject())
            {
                rootNode[propertyNameOrIndex] = subNode;
                return;
            }

            throw new JsonNodeException(
                $"An attempt to set a value at location '{propertyNameOrIndex}' failed. This is usually due to " +
                $"trying to set an explicit property against a declared array.");
        }

        private static JsonNode FindSubNodeOrThrow(this JsonNode currentNode, string propertyNameOrIndex)
        {
            Validation.ThrowIfNull(currentNode, nameof(currentNode));
            if (currentNode.IsValue())
            {
                throw new JsonNodeException(
                    $"Expected an object or array to search for segment '{propertyNameOrIndex}' but instead received " +
                    $"a single value");
            }

            int? arrIndex = null;
            if (int.TryParse(propertyNameOrIndex, out var index) && currentNode.IsArray())
                arrIndex = index;

            if (arrIndex.HasValue)
            {
                var arr = currentNode.AsArray();
                while (arr.Count <= arrIndex.Value)
                    arr.Add(null);

                return arr[arrIndex.Value];
            }

            if (currentNode.IsObject())
            {
                return currentNode.AsObject()[propertyNameOrIndex];
            }

            return null;
        }
    }
}