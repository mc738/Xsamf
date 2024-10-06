namespace Xsamf.V1.Domain.Monitoring

open System
open System.Text.Json
open System.Text.Json.Serialization
open FsToolbox.Core.Results

module Events =

    [<AutoOpen>]
    module private Internal =

        let strEquals (a: string) (b: string) =
            a.Equals(b, StringComparison.OrdinalIgnoreCase)

        let toJson (value: 'T) =
            try
                JsonSerializer.Serialize value |> Ok
            with exn ->
                Error(
                    { Message = exn.Message
                      DisplayMessage = "Failed to serialize object."
                      Exception = Some exn }
                    : FailureResult
                )

        let fromJson<'T> (json: string) =
            try
                JsonSerializer.Deserialize<'T> json |> Ok
            with exn ->
                Error(
                    { Message = exn.Message
                      DisplayMessage = "Failed to deserialize object."
                      Exception = Some exn }
                    : FailureResult
                )

    type MonitoringEvent =
        // Heart beat
        | NewHeartBeatWatcherCreated of NewHeartBeatWatcherCreatedEvent
        // Activities
        | NewActivityWatcherCreated
        // Probes
        | NewProbeWatcherCreated
        // Incidents

        static member Deserialize(name: string, data: string) =
            match name with
            | _ when NewHeartBeatWatcherCreatedEvent.Name() |> strEquals name -> fromJson<NewHeartBeatWatcherCreatedEvent> data
            | _ ->
                let message = $"Unknowing event type: `{name}`"

                Error(
                    { Message = message
                      DisplayMessage = message
                      Exception = None }
                    : FailureResult
                )

        static member TryDeserialize(name: string, data: string) =
            MonitoringEvent.Deserialize(name, data) |> Result.toOption

        member me.TrySerialize() =
            match me with
            | NewHeartBeatWatcherCreated data -> toJson data |> Result.map (fun r -> NewHeartBeatWatcherCreatedEvent.Name(), r)
            | NewActivityWatcherCreated -> failwith "todo"
            | NewProbeWatcherCreated -> failwith "todo"


    and [<CLIMutable>] NewHeartBeatWatcherCreatedEvent =
        { [<JsonPropertyName "reference">]
          Reference: string
          [<JsonPropertyName "createdDate">]
          CreatedDate: DateTime }

        static member Name() = "new-dms-watcher-created"
