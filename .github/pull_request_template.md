_This is an example pull request. Add to or remove from this in accordance with what you feel is necessary for your PR. Use your best judgement. A team member may ask for more details if they feel it is warranted._

[Helpful Resource on Submitting a PR](https://graphql-aspnet.github.io/docs/reference/contributing)

----

**Related Issues**
* #99999
* #99998
* #99997

## Changes Made

Previously, the add function would take in a `true/false` parameter to perform add or divide operations. I split functionality into two functions (`Add` and `Divide`) and updated all applicable unit tests.


**Usage**
```csharp
// add function
Calculator.Add(5, 3); // result 8

// divide function
Calculator.Divide(10, 2); // result 5
```

**Note:** Also added new unit tests for `Divide()`.