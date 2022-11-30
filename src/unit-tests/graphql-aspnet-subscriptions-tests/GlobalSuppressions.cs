﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

using System.Diagnostics.CodeAnalysis;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1600:Elements should be documented",
    Justification = "Documenting test methods is unwarranted at this time.",
    Scope = "module")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.NamingRules",
    "SA1313:Parameter names should begin with lower-case letter",
    Justification = "Testing of Alternative Naming schemas is required for unit tests",
    Scope = "module")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.NamingRules",
    "SA1300:Element should begin with upper-case letter",
    Justification = "Testing of Alternative Naming schemas is required for unit tests",
    Scope = "module")]

[assembly: SuppressMessage(
    "Design",
    "CA1063:Implement IDisposable Correctly",
    Justification = "Proper IDisposable not necessary for unit test project",
    Scope = "module")]
