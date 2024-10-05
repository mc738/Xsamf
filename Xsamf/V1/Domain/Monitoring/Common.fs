namespace Xsamf.V1.Domain.Monitoring

[<AutoOpen>]
module Common =

    open System.Text.Json
    open FsToolbox.Core
    open FsToolbox.Extensions.Strings
    open Xsamf.V1.Common.Utils
    
    [<RequireQualifiedAccess>]
    type ActionOutcome =
        | CreateIncident
        | CloseIncident
        | CloseAllIncidentsForEntity
        | ActivateHeartBeatWatcher
        | DeactivateHeartBeatWatcher
        | ActivateActivityWatcher
        | DeactivateActivityWatcher
        | ActivateProbeWatcher
        | DeactivateProbeWatcher

        static member FromJson(element: JsonElement) =
            match Json.tryGetStringProperty "type" element with
            | Some t ->
                match t with
                | "create-incident" -> Ok ActionOutcome.CreateIncident
                | "close-incident" -> Ok ActionOutcome.CloseIncident
                | "close-all-incidents-for-entity" -> Ok ActionOutcome.CloseAllIncidentsForEntity
                | "activate-heart-beat-watcher" -> Ok ActionOutcome.ActivateHeartBeatWatcher
                | "deactivate-heart-beat-watcher" -> Ok ActionOutcome.DeactivateActivityWatcher
                | "activate-activity-watcher" -> Ok ActionOutcome.ActivateActivityWatcher
                | "deactivate-activity-watcher" -> Ok ActionOutcome.DeactivateActivityWatcher
                | "activate-probe-watcher" -> Ok ActionOutcome.ActivateProbeWatcher
                | "deactivate-probe-watcher" -> Ok ActionOutcome.DeactivateProbeWatcher
                | t -> Error $"Unknown action outcome type: `{t}`"
            | None -> Error "Missing `type` property"

        static member Deserialize(str: string) =
            Json.tryParseToElement str
            |> Result.bind (fun el ->
                ActionOutcome.FromJson el
                |> Result.mapError (fun e ->
                    { Message = e
                      DisplayMessage = e
                      Exception = None }))

        member ao.WriteToJsonValue(writer: Utf8JsonWriter) =
            Json.writeObject
                (fun w ->
                    match ao with
                    | CreateIncident -> w.WriteString("type", "create-incident")
                    | CloseIncident -> w.WriteString("type", "close-incident")
                    | CloseAllIncidentsForEntity -> w.WriteString("type", "close-all-incidents-for-entity")
                    | ActivateHeartBeatWatcher -> w.WriteString("type", "activate-heart-beat-watcher")
                    | DeactivateHeartBeatWatcher -> w.WriteString("type", "deactivate-heart-beat-watcher")
                    | ActivateActivityWatcher -> w.WriteString("type", "activate-activity-watcher")
                    | DeactivateActivityWatcher -> w.WriteString("type", "deactivate-activity-watcher")
                    | ActivateProbeWatcher -> w.WriteString("type", "activate-probe-watcher")
                    | DeactivateProbeWatcher -> w.WriteString("type", "deactivate-probe-watcher"))
                writer

    [<RequireQualifiedAccess>]
    type MonitoringAuthType =
        | None
        | Token of string


    [<RequireQualifiedAccess>]
    type HashAlgorithm =
        | SHA256
        | None

        static member Deserialize(str: string) =
            match str.ToLower() with
            | "sha256" -> HashAlgorithm.SHA256
            | "none" -> HashAlgorithm.None
            | _ -> HashAlgorithm.None

        member ha.Serialize() =
            match ha with
            | SHA256 -> "sha256"
            | None -> "none"

        member ha.HashString(str: string) =
            match ha with
            | SHA256 -> str.GetSHA256Hash()
            | None -> str


//static member FromJson(element: JsonElement) =
//    match element.tryGet
//
//
//    ()
