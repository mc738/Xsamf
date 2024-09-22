namespace Xsamf.UnitTests.Domain.Monitoring.Dms

open System
open System.IO
open System.Text
open System.Text.Json
open Microsoft.VisualStudio.TestTools.UnitTesting
open Xsamf.V1.Domain.JsonConverters
open Xsamf.V1.Domain.Monitoring.Activities
open Xsamf.V1.Domain.Monitoring.Dms

[<AutoOpen>]
module private Internal =

    let writeJson (fn: Utf8JsonWriter -> unit) =
        use ms = new MemoryStream()
        use writer = new Utf8JsonWriter(ms)

        fn writer

        writer.Flush()
        ms.ToArray() |> Encoding.UTF8.GetString

    let tryToJsonElement (str: string) =
        try
            (JsonDocument.Parse str).RootElement |> Ok
        with ex ->
            Error ex.Message

[<TestClass>]
type DmsDomainRulesTests() =

    [<TestMethod>]
    member _.``DmsRule.CheckInPeriodPassed serialize and deserialize``() =
        let expected = DmsRule.CheckInPeriodPassed

        let serializedRule = writeJson expected.WriteToJsonValue

        match serializedRule |> tryToJsonElement |> Result.bind DmsRule.FromJson with
        | Ok actual -> Assert.AreEqual(expected, actual)
        | Error e -> Assert.Fail $"Failed to deserialize: {e}"

    [<TestMethod>]
    member _.``DmsRuleConverter DmsRule.CheckInPeriodPassed deserialized successfully``() =
        let option = JsonSerializerOptions()
        option.Converters.Add(DmsRuleConverter())

        let rule = """{"type": "check-in-period-passed"}"""

        let expected = DmsRule.CheckInPeriodPassed

        let actual = JsonSerializer.Deserialize<DmsRule>(rule, option)

        Assert.AreEqual(expected, actual)
        
    [<TestMethod>]
    member _.``DmsRuleConverter DmsRule.CheckInPeriodPassed serialized successfully``() =
        let option = JsonSerializerOptions()
        option.Converters.Add(DmsRuleConverter())

        let expected = """{"type":"check-in-period-passed"}"""

        let rule = DmsRule.CheckInPeriodPassed

        let actual = JsonSerializer.Serialize(rule, option)

        Assert.AreEqual(expected, actual)
