// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.TestData.VariableExecutionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.Extensions.Hosting;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class VariableKitchenSinkTests
    {
        private static List<object[]> _allBuildInScalarTestCases;
        private static List<object[]> _deeplyNestedInputObjectTestCases;

        static VariableKitchenSinkTests()
        {
            _allBuildInScalarTestCases = new List<object[]>();
            _deeplyNestedInputObjectTestCases = new List<object[]>();

            _allBuildInScalarTestCases.Add(
            new object[]
            {
                 @"{
                    ""regularSbyte"": -3e1,
                    ""nullSbyte"": 3,

                    ""regularShort"": -3e2,
                    ""nullShort"": -1,

                    ""regularInt"": 5,
                    ""nullInt"": 3,

                    ""regularLong"": -5,
                    ""nullLong"": 3,

                    ""regularByte"": 15,
                    ""nullByte"": 4,

                    ""regularUshort"": 300,
                    ""nullUshort"": 5,

                    ""regularUint"": 99,
                    ""nullUint"": 100,

                    ""regularUlong"": 2000,
                    ""nullUlong"": 3123,

                    ""regularFloat"": 1.2,
                    ""nullFloat"": 3,

                    ""regularDouble"": 12.4,
                    ""nullDouble"": 5,

                    ""regularDecimal"": 19.8,
                    ""nullDecimal"": 6,

                    ""regularBoolean"": true,
                    ""nullBoolean"": true,

                    ""regularString"": ""bob"",
                    ""nullString"": ""smith"",

                    ""regularId"": ""id1"",
                    ""nullId"": ""id2"",

                    ""regularUri"": ""http://sample.com"",
                    ""nullUri"": ""/segment1/segment2"",

                    ""regularGuid"" : ""3348e83f-8ccd-4f69-a580-76988c619aee"",
                    ""nullGuid"":  ""f7e18db6-0cfe-4467-bdab-74e66915cad2"",

                    ""regularDateTime"": ""2022-12-30T18:30:38.259+00:00"",
                    ""nullDateTime"": ""2022-12-31T19:30:39.255+00:00"",

                    ""regularDateTimeNumber"": 1672425038259,
                    ""nullDateTimeNumber"": 1672515039255,

                    ""regularDateTimeOffset"": ""2022-12-30T18:30:38.259+00:00"",
                    ""nullDateTimeOffset"": ""2022-12-31T19:30:39.255+00:00"",

                    ""regularDateTimeOffsetNumber"": 1672425038259,
                    ""nullDateTimeOffsetNumber"": 1672515039255,

                    ""regularDateOnly"": ""2022-12-30T18:30:38.259+00:00"",
                    ""nullDateOnly"": ""2022-12-31T19:30:39.255+00:00"",

                    ""regularDateOnlyNumber"": 1672425038259,
                    ""nullDateOnlyNumber"": 1672515039255,

                    ""regularTimeOnly"": ""18:30:38.259"",
                    ""nullTimeOnly"": ""19:30:39.255"",

                    ""regularEnum"": ""VALUE1"",
                    ""nullEnum"": ""VALUE2"",
                }",
                 @"{
                    ""data"" : {
                        ""parseSbytes"": -27,
                        ""parseShorts"": -301,
                        ""parseInts"": 8,
                        ""parseLongs"": -2,

                        ""parseBytes"": 19,
                        ""parseUshorts"": 305,
                        ""parseUints"": 199,
                        ""parseUlongs"": 5123,

                        ""parseFloats"": 4.2,
                        ""parseDoubles"": 17.4,
                        ""parseDecimals"": 25.8,

                        ""parseBooleans"": 1010,

                        ""parseStrings"" : ""bob smith"",

                        ""parseIds"": ""id1 id2"",

                        ""parseUris"": ""http://sample.com/segment1/segment2"",

                        ""parseGuids"": ""3348e83f-8ccd-4f69-a580-76988c619aee f7e18db6-0cfe-4467-bdab-74e66915cad2"",

                        ""parseDateTimes"" : ""2022-12-30T18:30:38.259+00:00 2022-12-31T19:30:39.255+00:00"",
                        ""parseDateTimesAsNumbers"" : ""2022-12-30T18:30:38.259+00:00 2022-12-31T19:30:39.255+00:00"",

                        ""parseDateTimeOffsets"" : ""2022-12-30T18:30:38.259+00:00 2022-12-31T19:30:39.255+00:00"",
                        ""parseDateTimeOffsetsAsNumbers"" : ""2022-12-30T18:30:38.259+00:00 2022-12-31T19:30:39.255+00:00"",

                        ""parseDateOnlys"" : ""2022-12-30 2022-12-31"",
                        ""parseDateOnlysAsNumbers"" : ""2022-12-30 2022-12-31"",

                        ""parseTimeOnlys"" : ""18:30:38.259 19:30:39.255"",

                        ""parseEnums"": ""VALUE3""
                    }
                }",
            });
            _allBuildInScalarTestCases.Add(
            new object[]
            {
                 @"{
                    ""regularSbyte"": -3e1,
                    ""nullSbyte"": null,

                    ""regularShort"": -3e2,
                    ""nullShort"": null,

                    ""regularInt"": 5,
                    ""nullInt"": null,

                    ""regularLong"": -5,
                    ""nullLong"": null,

                    ""regularByte"": 15,
                    ""nullByte"": null,

                    ""regularUshort"": 300,
                    ""nullUshort"": null,

                    ""regularUint"": 99,
                    ""nullUint"": null,

                    ""regularUlong"": 2000,
                    ""nullUlong"": null,

                    ""regularFloat"": 1.2,
                    ""nullFloat"": null,

                    ""regularDouble"": 12.4,
                    ""nullDouble"": null,

                    ""regularDecimal"": 19.8,
                    ""nullDecimal"": null,

                    ""regularBoolean"": true,
                    ""nullBoolean"": null,

                    ""regularString"": ""bob"",
                    ""nullString"": null,

                    ""regularId"": ""id1"",
                    ""nullId"": null,

                    ""regularUri"": ""http://sample.com"",
                    ""nullUri"": null,

                    ""regularGuid"" : ""3348e83f-8ccd-4f69-a580-76988c619aee"",
                    ""nullGuid"":  null,

                    ""regularDateTime"": ""2022-12-30T18:30:38.259+00:00"",
                    ""nullDateTime"": null,

                    ""regularDateTimeNumber"": 1672425038259,
                    ""nullDateTimeNumber"": null,

                    ""regularDateTimeOffset"": ""2022-12-30T18:30:38.259+00:00"",
                    ""nullDateTimeOffset"": null,

                    ""regularDateTimeOffsetNumber"": 1672425038259,
                    ""nullDateTimeOffsetNumber"": null,

                    ""regularDateOnly"": ""2022-12-30T18:30:38.259+00:00"",
                    ""nullDateOnly"": null,

                    ""regularDateOnlyNumber"": 1672425038259,
                    ""nullDateOnlyNumber"": null,

                    ""regularTimeOnly"": ""18:30:38.259"",
                    ""nullTimeOnly"": null,

                    ""regularEnum"": ""VALUE1"",
                    ""nullEnum"": null,
                }",
                 @"{
                    ""data"" : {
                        ""parseSbytes"": -30,
                        ""parseShorts"": -300,
                        ""parseInts"": 5,
                        ""parseLongs"": -5,

                        ""parseBytes"": 15,
                        ""parseUshorts"": 300,
                        ""parseUints"": 99,
                        ""parseUlongs"": 2000,

                        ""parseFloats"": 1.2,
                        ""parseDoubles"": 12.4,
                        ""parseDecimals"": 19.8,

                        ""parseBooleans"": 10,

                        ""parseStrings"" : ""bob "",

                        ""parseIds"": ""id1 "",

                        ""parseUris"": ""http://sample.com/"",

                        ""parseGuids"": ""3348e83f-8ccd-4f69-a580-76988c619aee "",

                        ""parseDateTimes"" : ""2022-12-30T18:30:38.259+00:00 "",
                        ""parseDateTimesAsNumbers"" : ""2022-12-30T18:30:38.259+00:00 "",

                        ""parseDateTimeOffsets"" : ""2022-12-30T18:30:38.259+00:00 "",
                        ""parseDateTimeOffsetsAsNumbers"" : ""2022-12-30T18:30:38.259+00:00 "",

                        ""parseDateOnlys"" : ""2022-12-30 "",
                        ""parseDateOnlysAsNumbers"" : ""2022-12-30 "",

                        ""parseTimeOnlys"" : ""18:30:38.259 "",

                        ""parseEnums"": ""VALUE1""
                    }
                }",
            });

            _deeplyNestedInputObjectTestCases.Add(
                new object[]
                {
                    @"{
                        ""nestedField"" : ""nestedFieldValue"",
                        ""doubleNestedField"" : ""doubleNestedFieldValue"",

                        ""nestedObject"" : {
                            ""dataField"": ""nestedObjectDataValue"",
                            ""dataObject"": null
                        },

                        ""veryDeepField"": ""veryDeepFieldValue"",

                        ""veryDeepObject"" : {
                            ""dataField"": ""veryDeepObjectFieldValue"",
                            ""dataObject"": {
                                ""dataField"" : ""veryVeryDeepObjectFieldValue"",
                                ""dataObject"": null
                            }
                        },

                        ""nullItem"": {
                            ""dataField"" : ""nullItemFieldValue"",
                            ""dataObject"": null
                        }
                    }",

                    @"{
                      ""data"": {
                        ""parseObject"": {
                          ""dataField"": ""nestedFieldValue"",
                          ""dataObject"": {
                            ""dataField"": ""doubleNestedFieldValue"",
                            ""dataObject"": {
                              ""dataField"": ""thirdLevel"",
                              ""dataObject"": {
                                ""dataField"": ""veryDeepFieldValue"",
                                ""dataObject"": {
                                  ""dataField"": ""veryDeepObjectFieldValue"",
                                  ""dataObject"": {
                                    ""dataField"" : ""veryVeryDeepObjectFieldValue"",
                                  }
                                }
                              }
                            }
                          },
                          ""secondDataObject"": {
                            ""dataField"": ""nestedObjectDataValue""
                          }
                        }
                      }
                    }",
                });

            _deeplyNestedInputObjectTestCases.Add(
                new object[]
                {
                    @"{
                        ""nestedField"" : ""nestedFieldValue"",
                        ""doubleNestedField"" : ""doubleNestedFieldValue"",

                        ""nestedObject"" : null,

                        ""veryDeepField"": ""veryDeepFieldValue"",

                        ""veryDeepObject"" : null,

                        ""nullItem"": null
                    }",

                    @"{
                      ""data"": {
                        ""parseObject"": {
                          ""dataField"": ""nestedFieldValue"",
                          ""dataObject"": {
                            ""dataField"": ""doubleNestedFieldValue"",
                            ""dataObject"": {
                              ""dataField"": ""thirdLevel"",
                              ""dataObject"": {
                                ""dataField"": ""veryDeepFieldValue"",
                                ""dataObject"": null
                              }
                            }
                          },
                          ""secondDataObject"": null
                        }
                      }
                    }",
                });
        }

        [TestCaseSource(nameof(_allBuildInScalarTestCases))]
        public async Task AllBuildInScalars(string variableSet, string expectedJson)
        {
            var server = new TestServerBuilder()
                .AddGraphController<AllVariablesController>()
                .Build();

            var context = server.CreateQueryContextBuilder()
                .AddQueryText(@"
                query(
                    # Signed Integer Numbers
                    $regularSbyte: SignedByte!,
                    $nullSbyte: SignedByte,
                    $regularShort: Short!,
                    $nullShort: Short,
                    $regularInt: Int!,
                    $nullInt: Int,
                    $regularLong: Long!,
                    $nullLong: Long,

                    # Unsigned Integer Numbers
                    $regularByte: Byte!,
                    $nullByte: Byte,
                    $regularUshort: UShort!,
                    $nullUshort: UShort,
                    $regularUint: UInt!,
                    $nullUint: UInt,
                    $regularUlong: ULong!,
                    $nullUlong: ULong,

                    # Floating Point Numbers
                    $regularFloat: Float!,
                    $nullFloat: Float,
                    $regularDouble: Double!,
                    $nullDouble: Double,
                    $regularDecimal: Decimal!,
                    $nullDecimal: Decimal,

                    $regularBoolean: Boolean!,
                    $nullBoolean: Boolean,

                    $regularString: String!,
                    $nullString: String

                    $regularId: ID!,
                    $nullId: ID

                    $regularUri: Uri!,
                    $nullUri: Uri

                    $regularGuid: Guid!,
                    $nullGuid: Guid

                    $regularDateTime: DateTime!,
                    $nullDateTime: DateTime,

                    $regularDateTimeNumber: DateTime!,
                    $nullDateTimeNumber: DateTime

                    $regularDateTimeOffset: DateTimeOffset!,
                    $nullDateTimeOffset: DateTimeOffset,

                    $regularDateTimeOffsetNumber: DateTimeOffset!,
                    $nullDateTimeOffsetNumber: DateTimeOffset

                    $regularDateOnly: DateOnly!,
                    $nullDateOnly: DateOnly,

                    $regularDateOnlyNumber: DateOnly!,
                    $nullDateOnlyNumber: DateOnly

                    $regularTimeOnly: TimeOnly!,
                    $nullTimeOnly: TimeOnly,

                    $regularEnum: VariableSuppliedEnum!,
                    $nullEnum: VariableSuppliedEnum,
                    ) {
                        # Signed Integer Numbers
                        parseSbytes(regularSbyte: $regularSbyte, nullSbyte: $nullSbyte)
                        parseShorts(regularShort: $regularShort, nullShort: $nullShort)
                        parseInts(regularInt: $regularInt, nullInt: $nullInt)
                        parseLongs(regularLong: $regularLong, nullLong: $nullLong)

                        # Unsigned Integer Numbers
                        parseBytes(regularByte: $regularByte, nullByte: $nullByte)
                        parseUshorts(regularUshort: $regularUshort, nullUshort: $nullUshort)
                        parseUints(regularUint: $regularUint, nullUint: $nullUint)
                        parseUlongs(regularUlong: $regularUlong, nullUlong: $nullUlong)

                        # Floating Point Numbers
                        parseFloats(regularFloat: $regularFloat, nullFloat: $nullFloat)
                        parseDoubles(regularDouble: $regularDouble, nullDouble: $nullDouble)
                        parseDecimals(regularDecimal: $regularDecimal, nullDecimal: $nullDecimal)

                        # Odds and Ends
                        parseBooleans(regularBoolean: $regularBoolean, nullBoolean: $nullBoolean)

                        parseStrings(regularString: $regularString, nullString: $nullString)

                        parseIds(regularId: $regularId, nullId: $nullId)

                        parseUris(regularUri: $regularUri, nullUri: $nullUri)

                        parseGuids(regularGuid: $regularGuid, nullGuid: $nullGuid)

                        # DateTime
                        parseDateTimes(regularDateTime: $regularDateTime, nullDateTime: $nullDateTime)

                        parseDateTimesAsNumbers:
                            parseDateTimes(regularDateTime: $regularDateTimeNumber, nullDateTime: $nullDateTimeNumber)

                        # DateTimeOffset
                        parseDateTimeOffsets(regularDateTimeOffset: $regularDateTimeOffset, nullDateTimeOffset: $nullDateTimeOffset)

                        parseDateTimeOffsetsAsNumbers:
                            parseDateTimeOffsets(regularDateTimeOffset: $regularDateTimeOffsetNumber, nullDateTimeOffset: $nullDateTimeOffsetNumber)

                        # DateOnly
                        parseDateOnlys(regularDateOnly: $regularDateOnly, nullDateOnly: $nullDateOnly)

                        parseDateOnlysAsNumbers:
                            parseDateOnlys(regularDateOnly: $regularDateOnlyNumber, nullDateOnly: $nullDateOnlyNumber)

                        # TimeOnly
                        parseTimeOnlys(regularTimeOnly: $regularTimeOnly, nullTimeOnly: $nullTimeOnly)

                        # Enum
                       parseEnums(regularEnum: $regularEnum, nullEnum: $nullEnum)
                }
            ")
                .AddVariableData(variableSet)
                .Build();

            var result = await server.RenderResult(context);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [TestCaseSource(nameof(_deeplyNestedInputObjectTestCases))]
        public async Task DeeplyNestedInputObjectTests(string variableSet, string expectedJson)
        {
            var server = new TestServerBuilder()
                .AddGraphController<AllVariablesController>()
                .Build();

            var context = server.CreateQueryContextBuilder()
                .AddQueryText(@"
                query (
                    $nestedField : String,
                    $doubleNestedField : String,

                    $nestedObject: Input_VariableSuppliedRepeatableObject,

                    $veryDeepField: String,
                    $veryDeepObject: Input_VariableSuppliedRepeatableObject

                    $nullItem: Input_VariableSuppliedRepeatableObject
                    ) {
                        parseObject(item: {
                                dataField: $nestedField
                                dataObject: {
                                    dataField: $doubleNestedField
                                    dataObject: {
                                        dataField: ""thirdLevel"",
                                        dataObject: {
                                            dataField: $veryDeepField
                                            dataObject: $veryDeepObject
                                        }
                                    }
                                }
                                secondDataObject: $nestedObject
                            },
                            nullItem: $nullItem)

                        {
                            dataField
                            dataObject {
                                dataField
                                dataObject {
                                    dataField
                                    dataObject {
                                        dataField
                                        dataObject {
                                            dataField
                                            dataObject {
                                                dataField
                                            }
                                        }
                                    }
                                }
                            }
                            secondDataObject {
                                dataField
                            }
                        }
                    }")
                .AddVariableData(variableSet)
                .Build();

            var result = await server.RenderResult(context);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }
    }
}