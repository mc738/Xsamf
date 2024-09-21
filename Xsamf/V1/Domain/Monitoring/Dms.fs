namespace Xsamf.V1.Domain.Monitoring

open System
open System.Text.Json
open System.Text.Json.Serialization
open FsToolbox.Core

// Suppress
#nowarn "50001"

module Dms =

    type DmsCheckIn =
        { Reference: string
          TenantReference: string
          WatcherReference: string
          Timestamp: DateTime
          Auth: AuthType
          Metadata: Map<string, string>
          Tags: string list }

    [<RequireQualifiedAccess>]
    type DmsRule =
        | CheckInPeriodPassed
        | GracePeriodPassed
        | Not of Rule: DmsRule
        | And of RuleA: DmsRule * RuleB: DmsRule
        | Or of RuleA: DmsRule * RuleB: DmsRule
        | All of Rules: DmsRule list
        | Any of Rules: DmsRule list

        static member FromJson(element: JsonElement) =
            match Json.tryGetStringProperty "type" element with
            | Some t ->
                match t with
                | "check-in-period-passed" -> Ok DmsRule.CheckInPeriodPassed
                | "grace-period-passed" -> Ok DmsRule.GracePeriodPassed
                | "not" ->
                    Json.tryGetProperty "rule" element
                    |> Option.map DmsRule.FromJson
                    |> Option.defaultValue (Error "Missing `rule` property")
                    |> Result.map DmsRule.Not
                | "and" ->
                    match
                        Json.tryGetProperty "ruleA" element
                        |> Option.map DmsRule.FromJson
                        |> Option.defaultValue (Error "Missing `ruleA` property"),
                        Json.tryGetProperty "ruleB" element
                        |> Option.map DmsRule.FromJson
                        |> Option.defaultValue (Error "Missing `ruleB` property")
                    with
                    | Ok ruleA, Ok ruleB -> DmsRule.And (ruleA, ruleB) |> Ok
                    | Error e, _ -> Error e
                    | _, Error e -> Error e
                | "or" ->
                    match
                        Json.tryGetProperty "ruleA" element
                        |> Option.map DmsRule.FromJson
                        |> Option.defaultValue (Error "Missing `ruleA` property"),
                        Json.tryGetProperty "ruleB" element
                        |> Option.map DmsRule.FromJson
                        |> Option.defaultValue (Error "Missing `ruleB` property")
                    with
                    | Ok ruleA, Ok ruleB -> DmsRule.Or (ruleA, ruleB) |> Ok
                    | Error e, _ -> Error e
                    | _, Error e -> Error e
                | _ -> Error $"Unknown rule type: `{t}`"
            | None -> Error "Missing `type` property"

        [<CompilerMessage("This method exists for preliminary for testing purposes or when a deterministic check is require. For general use, use DmsRule.Test(previousCheckIn) To remove this warning add `#nowarn \"50001\"` to the source file.",
                          50001)>]
        member dr.Test(currentDateTime: DateTime, nextCheckIn: DateTime, gracePeriod: TimeSpan) =
            match dr with
            | CheckInPeriodPassed -> currentDateTime > nextCheckIn
            | GracePeriodPassed -> currentDateTime > nextCheckIn.Add(gracePeriod)
            | Not rule -> rule.Test(currentDateTime, nextCheckIn, gracePeriod) |> not
            | And(ruleA, ruleB) ->
                ruleA.Test(currentDateTime, nextCheckIn, gracePeriod)
                && ruleB.Test(currentDateTime, nextCheckIn, gracePeriod)
            | Or(ruleA, ruleB) ->
                ruleA.Test(currentDateTime, nextCheckIn, gracePeriod)
                || ruleB.Test(currentDateTime, nextCheckIn, gracePeriod)
            | All rules ->
                rules
                |> List.forall (fun r -> r.Test(currentDateTime, nextCheckIn, gracePeriod))
            | Any rules ->
                rules
                |> List.exists (fun r -> r.Test(currentDateTime, nextCheckIn, gracePeriod))

        member dr.Test(nextCheckIn: DateTime, gracePeriod: TimeSpan) =
            dr.Test(DateTime.UtcNow, nextCheckIn, gracePeriod)

        member dr.WriteToJsonValue(writer: Utf8JsonWriter) =
            Json.writeObject
                (fun w ->
                    match dr with
                    | CheckInPeriodPassed -> w.WriteString("type", "check-in-period-passed")
                    | GracePeriodPassed -> w.WriteString("type", "grace-period-passed")
                    | Not rule ->
                        w.WriteString("type", "not")
                        w.WritePropertyName("rule")
                        rule.WriteToJsonValue(w)
                    | And(ruleA, ruleB) ->
                        w.WriteString("type", "and")
                        w.WritePropertyName("ruleA")
                        ruleA.WriteToJsonValue(w)
                        w.WritePropertyName("ruleB")
                        ruleB.WriteToJsonValue(w)
                    | Or(ruleA, ruleB) ->
                        w.WriteString("type", "or")
                        w.WritePropertyName("ruleA")
                        ruleA.WriteToJsonValue(w)
                        w.WritePropertyName("ruleB")
                        ruleB.WriteToJsonValue(w)
                    | All rules ->
                        w.WriteString("type", "all")
                        Json.writeArray (fun aw -> rules |> List.iter (fun r -> r.WriteToJsonValue aw)) "rules" w
                    | Any rules ->
                        w.WriteString("type", "any")
                        Json.writeArray (fun aw -> rules |> List.iter (fun r -> r.WriteToJsonValue aw)) "rules" w)
                writer

    type DmsAction =
        {
            Name: string
            Rule: DmsRule
            Outcomes: ActionOutcome list
            /// <summary>
            /// A collection of addition metadata that will be added to an activity if the action is triggered.
            /// These will be used for any future processing from this action specifically.
            /// </summary>
            AdditionMetadata: Map<string, string>
            /// <summary>
            /// A list of addition tags that will be added to an activity if the action is triggered.
            /// These will be used for any future processing from this action specifically.
            /// </summary>
            AdditionTags: string list
        }

        member da.Handle(newCheckInDate: DateTime, gracePeriod: TimeSpan) =
            match da.Rule.Test(newCheckInDate, gracePeriod) with
            | true ->
                ({ Name = da.Name
                   Outcomes = da.Outcomes
                   AdditionMetadata = da.AdditionMetadata
                   AdditionTags = da.AdditionTags }
                : DmsActionResult)
            | false -> DmsActionResult.Empty da.Name

    and DmsActionResult =
        { Name: string
          Outcomes: ActionOutcome list
          AdditionMetadata: Map<string, string>
          AdditionTags: string list }

        static member Empty(name: string) =
            { Name = name
              Outcomes = []
              AdditionMetadata = Map.empty
              AdditionTags = [] }

        member dar.HasOutcomes() = dar.Outcomes.IsEmpty |> not

    type DmsWatcher =
        {
            TenantReference: string
            EntityReference: string
            Reference: string
            Name: string
            NextCheckInTime: DateTime
            GracePeriod: TimeSpan
            Actions: DmsAction list
            Tags: string list
            /// <summary>
            /// A collection of addition metadata that will be added to an activity if the action is triggered.
            /// These will be used for any future processing from any rule hits from this watcher.
            /// </summary>
            AdditionMetadata: Map<string, string>
            /// <summary>
            /// A list of addition tags that will be added to an activity if the action is triggered.
            /// These will be used for any future processing from any rule hits from this watcher.
            /// </summary>
            AdditionTags: string list
        }

        member dw.Handle(filterEmpty: bool) =
            dw.Actions
            |> List.map (fun a -> a.Handle(dw.NextCheckInTime, dw.GracePeriod))
            |> fun r ->
                if filterEmpty then
                    r |> List.filter (fun aar -> aar.HasOutcomes())
                else
                    r
