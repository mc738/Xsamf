namespace Xsamf.V1.Common

open System.Text.Json
open FsToolbox.Core.Results

module Utils =

    open System

    let createReference () = Guid.NewGuid().ToString("n")

    let createTimestamp () = DateTime.UtcNow

    let aggregateResults<'T, 'U> (results: Result<'T, 'U> list) =
        results
        |> List.fold
            (fun (oks, errors) result ->
                match result with
                | Ok resultValue -> resultValue :: oks, errors
                | Error errorValue -> oks, errorValue :: errors)
            ([], [])
        |> fun (oks, errors) ->
            match errors.IsEmpty with
            | true -> Error errors
            | false -> Ok oks

    let aggregateStringErrors (message: string) (results: Result<'T, string> list) =
        aggregateResults results
        |> Result.mapError (fun es ->
            [ message; "The following errors occurred: "; yield! es ]
            |> String.concat Environment.NewLine)

    module Map =

        let merge<'TKey, 'TValue when 'TKey: comparison> (mapB: Map<'TKey, 'TValue>) (mapA: Map<'TKey, 'TValue>) =
            mapA |> Map.fold (fun (state: Map<'TKey, 'TValue>) k v -> state.Add(k, v)) mapB

    module Option =

        let fromBool (value: bool) =
            match value with
            | true -> Some()
            | false -> None

        let bindIfTrue<'T> (fn: unit -> 'T option) (value: bool) = value |> fromBool |> Option.bind fn

        let mapIfTrue<'T> (fn: unit -> 'T) (value: bool) = value |> fromBool |> Option.map fn
        
    module ActionResult =

        let ifSuccessful<'T, 'U> (fn: unit -> ActionResult<'U>) (result: ActionResult<'T>) =
            match result with
            | ActionResult.Failure failureResult -> result
            | ActionResult.Success foo -> fn () |> ActionResult.bind (fun _ -> result)
            
