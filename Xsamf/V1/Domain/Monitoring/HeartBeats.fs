namespace Xsamf.V1.Domain.Monitoring

// Suppress
#nowarn "50001"

module HeartBeats =

    open System
    open System.Text.Json
    open System.Text.Json.Serialization
    open FsToolbox.Core
    open Xsamf.V1.Common.Utils

    type HeartBeatCheckIn =
        { Reference: string
          TenantReference: string
          WatcherReference: string
          Timestamp: DateTime
          Auth: MonitoringAuthType
          Metadata: Map<string, string>
          Tags: string list }

    /// <summary>
    /// A special type of HeartBeat check in that requires no auth or verification.
    /// This can be useful for simple situations,
    /// however there are not checks to validate where the check in comes from.
    /// This requires the related watcher to have AllowAnonymous set to true. 
    /// </summary>
    type AnonymousHeartBeatCheckIn =
        { Reference: string
          WatcherReference: string
          Timestamp: DateTime
          Metadata: Map<string, string>
          Tags: string list }

    [<RequireQualifiedAccess>]
    type HeartBeatRule =
        | CheckInPeriodPassed
        | GracePeriodPassed
        | Not of Rule: HeartBeatRule
        | And of RuleA: HeartBeatRule * RuleB: HeartBeatRule
        | Or of RuleA: HeartBeatRule * RuleB: HeartBeatRule
        | All of Rules: HeartBeatRule list
        | Any of Rules: HeartBeatRule list

        static member FromJson(element: JsonElement) =
            match Json.tryGetStringProperty "type" element with
            | Some t ->
                match t with
                | "check-in-period-passed" -> Ok HeartBeatRule.CheckInPeriodPassed
                | "grace-period-passed" -> Ok HeartBeatRule.GracePeriodPassed
                | "not" ->
                    Json.tryGetProperty "rule" element
                    |> Option.map HeartBeatRule.FromJson
                    |> Option.defaultValue (Error "Missing `rule` property")
                    |> Result.map HeartBeatRule.Not
                | "and" ->
                    match
                        Json.tryGetProperty "ruleA" element
                        |> Option.map HeartBeatRule.FromJson
                        |> Option.defaultValue (Error "Missing `ruleA` property"),
                        Json.tryGetProperty "ruleB" element
                        |> Option.map HeartBeatRule.FromJson
                        |> Option.defaultValue (Error "Missing `ruleB` property")
                    with
                    | Ok ruleA, Ok ruleB -> HeartBeatRule.And(ruleA, ruleB) |> Ok
                    | Error e, _ -> Error e
                    | _, Error e -> Error e
                | "or" ->
                    match
                        Json.tryGetProperty "ruleA" element
                        |> Option.map HeartBeatRule.FromJson
                        |> Option.defaultValue (Error "Missing `ruleA` property"),
                        Json.tryGetProperty "ruleB" element
                        |> Option.map HeartBeatRule.FromJson
                        |> Option.defaultValue (Error "Missing `ruleB` property")
                    with
                    | Ok ruleA, Ok ruleB -> HeartBeatRule.Or(ruleA, ruleB) |> Ok
                    | Error e, _ -> Error e
                    | _, Error e -> Error e
                | "all" ->
                    match
                        Json.tryGetArrayProperty "rules" element
                        |> Option.map (List.map HeartBeatRule.FromJson >> aggregateStringErrors "Unable to create rules")
                        |> Option.defaultValue (Error "Missing `rules` property")
                    with
                    | Ok rules -> HeartBeatRule.All rules |> Ok
                    | Error e -> Error e
                | "any" ->
                    match
                        Json.tryGetArrayProperty "rules" element
                        |> Option.map (List.map HeartBeatRule.FromJson >> aggregateStringErrors "Unable to create rules")
                        |> Option.defaultValue (Error "Missing `rules` property")
                    with
                    | Ok rules -> HeartBeatRule.Any rules |> Ok
                    | Error e -> Error e
                | _ -> Error $"Unknown rule type: `{t}`"
            | None -> Error "Missing `type` property"

        [<CompilerMessage("This method exists for preliminary for testing purposes or when a deterministic check is require. For general use, use HeartBeatRule.Test(previousCheckIn) To remove this warning add `#nowarn \"50001\"` to the source file.",
                          50001)>]
        member hbr.Test(currentDateTime: DateTime, nextCheckIn: DateTime, gracePeriod: TimeSpan) =
            match hbr with
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

        member hbr.Test(nextCheckIn: DateTime, gracePeriod: TimeSpan) =
            hbr.Test(DateTime.UtcNow, nextCheckIn, gracePeriod)

        member hbr.WriteToJsonValue(writer: Utf8JsonWriter) =
            Json.writeObject
                (fun w ->
                    match hbr with
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

    type HeartBeatAction =
        {
            Name: string
            Rule: HeartBeatRule
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

        member hba.Handle(newCheckInDate: DateTime, gracePeriod: TimeSpan) =
            match hba.Rule.Test(newCheckInDate, gracePeriod) with
            | true ->
                ({ Name = hba.Name
                   Outcomes = hba.Outcomes
                   AdditionMetadata = hba.AdditionMetadata
                   AdditionTags = hba.AdditionTags }
                : HeartBeatActionResult)
            | false -> HeartBeatActionResult.Empty hba.Name

    and HeartBeatActionResult =
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

    type HeartBeatWatcher =
        {
            TenantReference: string
            EntityReference: string
            Reference: string
            Name: string
            AllowAnonymous: bool
            NextCheckInTime: DateTime
            GracePeriod: TimeSpan
            Actions: HeartBeatAction list
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

        member hbw.Handle(filterEmpty: bool) =
            hbw.Actions
            |> List.map (fun a -> a.Handle(hbw.NextCheckInTime, hbw.GracePeriod))
            |> fun r ->
                if filterEmpty then
                    r |> List.filter (fun aar -> aar.HasOutcomes())
                else
                    r
