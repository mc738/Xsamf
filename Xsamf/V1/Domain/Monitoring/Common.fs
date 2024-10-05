namespace Xsamf.V1.Domain.Monitoring

open System.Text.Json
open FsToolbox.Core
open FsToolbox.Extensions.Strings

[<AutoOpen>]
module Common =

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
