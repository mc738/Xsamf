namespace Xsamf.UnitTests.Domain.Monitoring.HeartBeat

open System
open System.IO
open System.Text
open System.Text.Json
open Microsoft.VisualStudio.TestTools.UnitTesting
open Xsamf.V1.Domain.JsonConverters
open Xsamf.V1.Domain.Monitoring.HeartBeats

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
type HeartBeatDomainRulesTests() =

    [<TestMethod>]
    member _.``HeartBeatRule.CheckInPeriodPassed serialize and deserialize``() =
        let expected = HeartBeatRule.CheckInPeriodPassed

        let serializedRule = writeJson expected.WriteToJsonValue

        match serializedRule |> tryToJsonElement |> Result.bind HeartBeatRule.FromJson with
        | Ok actual -> Assert.AreEqual(expected, actual)
        | Error e -> Assert.Fail $"Failed to deserialize: {e}"

    [<TestMethod>]
    member _.``HeartBeatRuleConverter HeartBeatRule.CheckInPeriodPassed deserialized successfully``() =
        let option = JsonSerializerOptions()
        option.Converters.Add(HeartBeatRuleConverter())

        let rule = """{"type": "check-in-period-passed"}"""

        let expected = HeartBeatRule.CheckInPeriodPassed

        let actual = JsonSerializer.Deserialize<HeartBeatRule>(rule, option)

        Assert.AreEqual(expected, actual)
        
    [<TestMethod>]
    member _.``HeartBeatRuleConverter HeartBeatRule.CheckInPeriodPassed serialized successfully``() =
        let option = JsonSerializerOptions()
        option.Converters.Add(HeartBeatRuleConverter())

        let expected = """{"type":"check-in-period-passed"}"""

        let rule = HeartBeatRule.CheckInPeriodPassed

        let actual = JsonSerializer.Serialize(rule, option)

        Assert.AreEqual(expected, actual)
