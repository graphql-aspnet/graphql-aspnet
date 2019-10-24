// *************************************************************
// project:  grahpql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.StarwarsAPI.Common.Services
{
    using System;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Builder;

    public static class StarWarsExtensions
    {
        /// <summary>
        /// Adds a friendly startup message to the console when the app starts in debug mode. Helpful for individuals
        /// that may download the source and just run it.
        /// </summary>
        /// <param name="appBuilder">The application builder.</param>
        public static void AddStarWarsStartedMessageToConsole(this IApplicationBuilder appBuilder)
        {
            if (Debugger.IsAttached)
            {
                WriteLineWithPadding("", 50, '*', "**");
                WriteLineWithPadding("Star Wars Sample GraphQL Application Started", 50, ' ', "**");
                WriteLineWithPadding("", 50, '-', "**");
                WriteLineWithPadding("Point Your GraphQL tool of choice at", 50, ' ', "**");
                WriteLineWithPadding($"http://{{hostname}}{Constants.Routing.DEFAULT_HTTP_ROUTE}", 50, ' ', "**");
                WriteLineWithPadding("", 50, '*', "**");
                Console.WriteLine();
            }
        }

        private static void WriteLineWithPadding(string text, int totalLength, char padChar, string endCap)
        {
            Console.Write(endCap);
            Console.Write(" ");
            Console.Write(text);
            if (text.Length > totalLength + endCap.Length)
            {
                Console.WriteLine();
                return;
            }

            var remainingLength = totalLength + endCap.Length - text.Length;
            Console.Write("".PadLeft(remainingLength, padChar));
            Console.Write(" ");
            Console.WriteLine(endCap);
        }
    }
}