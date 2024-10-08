namespace Xsamf.V1.Domain.Monitoring

module Activities =

    open System
    open System.Text.Json
    open FsToolbox.Core
    open Xsamf.V1.Common.Utils

    type ActivityHasher =
        { Steps: ActivityIncidentHasherStep list
          Separator: string option
          HashAlgorithm: HashAlgorithm }

        static member FromJson(element: JsonElement) =
            Json.tryGetArrayProperty "steps" element
            |> Option.map (
                List.map ActivityIncidentHasherStep.FromJson
                >> aggregateStringErrors "Failed to create activity hasher steps."
            )
            |> Option.defaultValue (Error "Missing `steps` property")
            |> Result.map (fun steps ->
                { Steps = steps
                  Separator = Json.tryGetStringProperty "separator" element
                  HashAlgorithm =
                    Json.tryGetStringProperty "hashAlgorithm" element
                    |> Option.map HashAlgorithm.Deserialize
                    |> Option.defaultValue HashAlgorithm.None })
            
        static member Deserialize(str: string) =
            Json.tryParseToElement str
            |> Result.bind (fun el ->
                ActivityHasher.FromJson el
                |> Result.mapError (fun e ->
                    { Message = e
                      DisplayMessage = e
                      Exception = None }))

        member ah.WriteToJsonValue(writer: Utf8JsonWriter) =
            Json.writeObject
                (fun w ->
                    Json.writeArray (fun aw -> ah.Steps |> List.iter (fun s -> s.WriteToJsonValue aw)) "steps" w
                    ah.Separator |> Option.iter (fun v -> w.WriteString("separator", v))
                    w.WriteString("hashAlgorithm", ah.HashAlgorithm.Serialize()))
                writer

    and ActivityIncidentHasherStep =
        | AddTagIfExists of Tag: string * Default: string option
        | AddTimestamp of Format: string option
        | AddCategory
        | AddConstant of Value: string
        | AddMetadataValueIfExists of Key: string * Default: string option
        | AddWatcherReference
        | AddWatcherName
        | AddActionReference
        | AddActionName
        | AddType

        static member FromJson(element: JsonElement) =
            match Json.tryGetStringProperty "type" element with
            | Some t ->
                match t with
                | "add-tag-if-exists" ->
                    match Json.tryGetStringProperty "tag" element with
                    | None -> Error "Missing `tag` property"
                    | Some tag -> AddTagIfExists(tag, Json.tryGetStringProperty "default" element) |> Ok
                | "add-timestamp" -> AddTimestamp(Json.tryGetStringProperty "format" element) |> Ok
                | "add-category" -> Ok AddCategory
                | "add-constant" ->
                    match Json.tryGetStringProperty "value" element with
                    | None -> Error "Missing `value` property"
                    | Some value -> AddConstant value |> Ok
                | "add-metadata-value-if-exists" ->
                    match Json.tryGetStringProperty "key" element with
                    | None -> Error "Missing `key` property"
                    | Some key -> AddMetadataValueIfExists(key, Json.tryGetStringProperty "default" element) |> Ok
                | "add-watcher-reference" -> Ok AddWatcherReference
                | "add-watcher-name" -> Ok AddWatcherName
                | "add-action-reference" -> Ok AddActionReference
                | "add-action-name" -> Ok AddActionName
                | "add-type" -> Ok AddType
                | t -> Error $"Unknown type: `{t}`"
            | None -> Error "Missing `type` property"
        
        member ah.WriteToJsonValue(writer: Utf8JsonWriter) =
            Json.writeObject
                (fun w ->
                    match ah with
                    | AddTagIfExists(tag, ``default``) ->
                        w.WriteString("type", "add-tag-if-exists")
                        w.WriteString("tag", tag)
                        ``default`` |> Option.iter (fun v -> w.WriteString("default", v))
                    | AddTimestamp format ->
                        w.WriteString("type", "add-timestamp")
                        format |> Option.iter (fun v -> w.WriteString("format", v))
                    | AddCategory -> w.WriteString("type", "add-category")
                    | AddConstant value ->
                        w.WriteString("type", "add-constant")
                        w.WriteString("value", value)
                    | AddMetadataValueIfExists(key, ``default``) ->
                        w.WriteString("type", "add-metadata-value-if-exists")
                        w.WriteString("key", key)
                        ``default`` |> Option.iter (fun v -> w.WriteString("default", v))
                    | AddWatcherReference -> w.WriteString("type", "add-watcher-reference")
                    | AddWatcherName -> w.WriteString("type", "add-watcher-name")
                    | AddActionReference -> w.WriteString("type", "add-action-reference")
                    | AddActionName -> w.WriteString("type", "add-action-name")
                    | AddType -> w.WriteString("type", "add-type"))
                writer

    [<RequireQualifiedAccess>]
    type ActivityCategory =
        | Information
        | Success
        | Warning
        | Error
        | Failure
        | Critical
        | Bespoke of string

        static member Deserialize(str: string) =
            match str.ToLower() with
            | "info"
            | "information" -> ActivityCategory.Information
            | "success"
            | "ok" -> ActivityCategory.Success
            | "warn"
            | "warning" -> ActivityCategory.Warning
            | "err"
            | "error" -> ActivityCategory.Error
            | "fail"
            | "failure" -> ActivityCategory.Failure
            | "crit"
            | "critical" -> ActivityCategory.Critical
            | v -> ActivityCategory.Bespoke v

        member ac.Serialize() =
            match ac with
            | Information -> "information"
            | Success -> "success"
            | Warning -> "warning"
            | Error -> "error"
            | Failure -> "failure"
            | Critical -> "critical"
            | Bespoke s -> s

    type Activity =
        { Reference: string
          WatcherReference: string
          Timestamp: DateTime
          // TODO add more
          Auth: MonitoringAuthType
          Category: ActivityCategory
          Metadata: Map<string, string>
          Tags: string list }

    [<RequireQualifiedAccess>]
    type ActivityRule =
        | IsCategory of Category: ActivityCategory
        | HasTag of Tag: string
        | HasTags of Tags: string list
        | HasMetadataKey of Key: string
        | MetadataValueEquals of Key: string * Value: string
        | Bespoke of HandlerName: string
        | Not of Rule: ActivityRule
        | And of RuleA: ActivityRule * RuleB: ActivityRule
        | Or of RuleA: ActivityRule * RuleB: ActivityRule
        | All of Rules: ActivityRule list
        | Any of Rules: ActivityRule list

        static member FromJson(element: JsonElement) =
            match Json.tryGetStringProperty "type" element with
            | Some t ->
                match t with
                | "is-category" ->
                    match Json.tryGetStringProperty "category" element with
                    | Some c -> ActivityCategory.Deserialize c |> ActivityRule.IsCategory |> Ok
                    | None -> Error "Missing `category` property"

                | "has-tag" ->
                    match Json.tryGetStringProperty "tag" element with
                    | Some t -> ActivityRule.HasTag t |> Ok
                    | None -> Error "Missing `tag` property"
                | "has-tags" ->
                    Json.tryGetProperty "tags" element
                    |> Option.map Ok
                    |> Option.defaultValue (Error "Missing `tags` property")
                    |> Result.bind (
                        Json.tryGetStringArray
                        >> Option.map (ActivityRule.HasTags >> Ok)
                        >> Option.defaultValue (Error "Could not create tags")
                    )
                | "has-metadata-key" ->
                    match Json.tryGetStringProperty "key" element with
                    | Some key -> ActivityRule.HasMetadataKey key |> Ok
                    | None -> Error "Missing `key` property"
                | "metadata-value-equals" ->
                    match Json.tryGetStringProperty "key" element, Json.tryGetStringProperty "value" element with
                    | Some key, Some value -> ActivityRule.MetadataValueEquals(key, value) |> Ok
                    | None, _ -> Error "Missing `key` property"
                    | _, None -> Error "Missing `value` property"
                | "bespoke" ->
                    match Json.tryGetStringProperty "handlerName" element with
                    | Some hn -> ActivityRule.Bespoke hn |> Ok
                    | None -> Error "Missing `handlerName` property"
                | "not" ->
                    Json.tryGetProperty "rule" element
                    |> Option.map ActivityRule.FromJson
                    |> Option.defaultValue (Error "Missing `rule` property")
                    |> Result.map ActivityRule.Not
                | "and" ->
                    match
                        Json.tryGetProperty "ruleA" element
                        |> Option.map ActivityRule.FromJson
                        |> Option.defaultValue (Error "Missing `ruleA` property"),
                        Json.tryGetProperty "ruleB" element
                        |> Option.map ActivityRule.FromJson
                        |> Option.defaultValue (Error "Missing `ruleB` property")
                    with
                    | Ok ruleA, Ok ruleB -> ActivityRule.And(ruleA, ruleB) |> Ok
                    | Error e, _ -> Error e
                    | _, Error e -> Error e
                | "or" ->
                    match
                        Json.tryGetProperty "ruleA" element
                        |> Option.map ActivityRule.FromJson
                        |> Option.defaultValue (Error "Missing `ruleA` property"),
                        Json.tryGetProperty "ruleB" element
                        |> Option.map ActivityRule.FromJson
                        |> Option.defaultValue (Error "Missing `ruleB` property")
                    with
                    | Ok ruleA, Ok ruleB -> ActivityRule.Or(ruleA, ruleB) |> Ok
                    | Error e, _ -> Error e
                    | _, Error e -> Error e
                | "all" ->
                    match
                        Json.tryGetArrayProperty "rules" element
                        |> Option.map (List.map ActivityRule.FromJson >> aggregateStringErrors "Unable to create rules")
                        |> Option.defaultValue (Error "Missing `rules` property")
                    with
                    | Ok rules -> ActivityRule.All rules |> Ok
                    | Error e -> Error e
                | "any" ->
                    match
                        Json.tryGetArrayProperty "rules" element
                        |> Option.map (List.map ActivityRule.FromJson >> aggregateStringErrors "Unable to create rules")
                        |> Option.defaultValue (Error "Missing `rules` property")
                    with
                    | Ok rules -> ActivityRule.Any rules |> Ok
                    | Error e -> Error e
                | _ -> Error $"Unknown rule type: `{t}`"
            | None -> Error "Missing `type` property"

        static member Deserialize(str: string) =
            Json.tryParseToElement str
            |> Result.bind (fun el ->
                ActivityRule.FromJson el
                |> Result.mapError (fun e ->
                    { Message = e
                      DisplayMessage = e
                      Exception = None }))

        member ar.Test(activity: Activity, bespokeHandlers: Map<string, Activity -> bool>) =
            match ar with
            | IsCategory category -> activity.Category = category
            | HasTag tag -> activity.Tags |> List.contains tag
            | HasTags tags -> tags |> List.forall (fun t -> activity.Tags |> List.contains t)
            | HasMetadataKey key -> activity.Metadata |> Map.containsKey key
            | MetadataValueEquals(key, value) ->
                activity.Metadata.TryFind key
                |> Option.map (fun mdv -> mdv.Equals(value, StringComparison.OrdinalIgnoreCase))
                |> Option.defaultValue false
            | Bespoke handlerName ->
                bespokeHandlers.TryFind handlerName
                |> Option.map (fun h -> h activity)
                |> Option.defaultValue false
            | Not rule -> rule.Test(activity, bespokeHandlers) |> not
            | And(ruleA, ruleB) -> ruleA.Test(activity, bespokeHandlers) && ruleB.Test(activity, bespokeHandlers)
            | Or(ruleA, ruleB) -> ruleA.Test(activity, bespokeHandlers) || ruleB.Test(activity, bespokeHandlers)
            | All rules -> rules |> List.forall (fun r -> r.Test(activity, bespokeHandlers))
            | Any rules -> rules |> List.exists (fun r -> r.Test(activity, bespokeHandlers))

        member ar.Test(activity: Activity) = ar.Test(activity, Map.empty)

        member ar.WriteToJsonValue(writer: Utf8JsonWriter) =
            Json.writeObject
                (fun w ->
                    match ar with
                    | IsCategory category ->
                        w.WriteString("type", "is-category")
                        w.WriteString("category", category.Serialize())
                    | HasTag tag ->
                        w.WriteString("type", "has-tag")
                        w.WriteString("tag", tag)
                    | HasTags tags ->
                        w.WriteString("type", "has-tags")
                        Json.writeArray (fun aw -> tags |> List.iter aw.WriteStringValue) "tags" writer
                    | HasMetadataKey key ->
                        w.WriteString("type", "has-metadata-key")
                        w.WriteString("key", key)
                    | MetadataValueEquals(key, value) ->
                        w.WriteString("type", "metadata-value-equals")
                        w.WriteString("key", key)
                        w.WriteString("value", value)
                    | Bespoke handlerName ->
                        w.WriteString("type", "bespoke")
                        w.WriteString("handlerName", handlerName)
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

    type ActivityAction =
        {
            Reference: string
            Name: string
            Rule: ActivityRule
            Outcomes: ActionOutcome list
            Hasher: ActivityHasher
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

    and ActivityActionResult =
        { Name: string
          Hash: string
          Activity: Activity
          Outcomes: ActionOutcome list
          AdditionTags: string list
          AdditionMetadata: Map<string, string> }

        /// <summary>
        /// Get a list of all tags for the result.
        /// This includes the activity tags, the action tags and the watcher tags.
        /// </summary>
        member aar.GetTags() =
            List.distinct [ yield! aar.Activity.Tags; yield! aar.AdditionTags ]

        member aar.GetMetadata() =
            aar.Activity.Metadata |> Map.merge aar.AdditionMetadata

    type ActivityWatcher =
        {
            Reference: string
            Name: string
            TenantReference: string
            EntityReference: string
            Actions: ActivityAction list
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

        member aw.HandleActivity(activity, bespokeHandlers: Map<string, Activity -> bool>) =
            aw.Actions
            |> List.choose (fun a ->

                a.Rule.Test(activity, bespokeHandlers)
                |> Option.mapIfTrue (fun _ ->
                    let additionalTags = aw.AdditionTags @ a.AdditionTags
                    let additionalMetadata = a.AdditionMetadata |> Map.merge aw.AdditionMetadata

                    let allTags = activity.Tags @ additionalTags
                    let allMetadata = activity.Metadata |> Map.merge additionalMetadata

                    let hash =
                        a.Hasher.Steps
                        |> List.choose (fun s ->
                            match s with
                            | AddTagIfExists(tag, ``default``) ->
                                match allTags |> List.contains tag with
                                | true -> Some tag
                                | false -> ``default``
                            | AddTimestamp format ->
                                activity.Timestamp.ToString(format |> Option.defaultValue "u") |> Some
                            | AddCategory -> activity.Category.Serialize() |> Some
                            | AddConstant value -> Some value
                            | AddMetadataValueIfExists(key, ``default``) ->
                                allMetadata.TryFind key |> Option.orElse ``default``
                            | AddWatcherReference -> Some aw.Reference
                            | AddWatcherName -> Some aw.Name
                            | AddActionReference -> Some a.Reference
                            | AddActionName -> Some a.Name
                            | AddType -> Some "activity")
                        |> String.concat (a.Hasher.Separator |> Option.defaultValue "_")
                        |> a.Hasher.HashAlgorithm.HashString

                    ({ Name = a.Name
                       Hash = hash
                       Activity = activity
                       Outcomes = a.Outcomes
                       AdditionTags = additionalTags
                       AdditionMetadata = additionalMetadata }
                    : ActivityActionResult)))

(*
            aw.Actions
            |> List.map (fun a -> a.Handle(activity, bespokeHandlers))
            |> fun r ->
                if filterEmpty then
                    r |> List.filter (fun aar -> aar.HasOutcomes())
                else
                    r
            *)
