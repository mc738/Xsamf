namespace Xsamf.V1.Common

module Utils =

    open System
    open System.Text.Json
    open FsToolbox.Core.Results

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

    module FetchResult =

        let fromOption<'T> (message: string) (value: 'T option) =
            match value with
            | Some v -> FetchResult.Success v
            | None ->
                { DisplayMessage = message
                  Message = message
                  Exception = None }
                |> FetchResult.Failure
            
        let ifAllSuccess<'T> (message: string) (results: FetchResult<'T> list) =
            results
            |> FetchResult.unzipResults
            |> fun (results, failures) ->
                match failures.IsEmpty with
                | true -> FetchResult.Success results
                | false ->
                    FailureResult.Aggregate(failures, message)
                    |> FetchResult.Failure 

    module Json =

        let tryParseToElement (str: string) =
            try
                JsonDocument.Parse(str).RootElement |> Ok
            with ex ->
                { Message = $"Failure to parse json element: {ex.Message}"
                  DisplayMessage = "Failure to parse json element"
                  Exception = Some ex }
                |> Error

    [<AutoOpen>]
    module Extensions =
        
        open System.Text
        open Freql.Core.Common.Types
        
        type BlobField with
        
            
            member bf.GetValueAsString() =
                bf.ToBytes()
                |> Encoding.UTF8.GetString
                
            member bf.Convert<'T>(handler: string -> Result<'T, FailureResult>) =
                bf.GetValueAsString() |> handler
        
        ()