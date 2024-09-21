namespace Xsamf.V1.Common

open System
open FsToolbox.Core.Results
open Quartz

module Cron =

    let parse (cronExpression: string) =
        try
            CronExpression cronExpression |> ActionResult.Success
        with
        | :? FormatException as ex ->
            { DisplayMessage = "Invalid cron expression format"
              Message = ex.Message
              Exception = Some ex }
            |> ActionResult.Failure
        | ex ->
            { DisplayMessage = "Unable to parse cron expression"
              Message = ex.Message
              Exception = Some ex }
            |> ActionResult.Failure
        |> ActionResult.toResult

    let tryParse (cronExpression: string) = parse cronExpression |> Result.toOption

    let isValidate (cronExpression: string) =
        CronExpression.IsValidExpression cronExpression

    let getNextExecutionDate (currentDateTime: DateTime) (cronExpression: string) =
        parse cronExpression
        |> Result.bind (fun ce ->
            match ce.GetNextValidTimeAfter currentDateTime |> Option.ofNullable with
            | Some dt -> Ok dt
            | None ->
                { Message = "No next execution date available"
                  DisplayMessage = "No next execution date available"
                  Exception = None }
                |> Error)

    let tryGetNextExecutionDate (currentDateTime: DateTime) (cronExpression: string) =
        tryParse cronExpression
        |> Option.bind (fun ce -> ce.GetNextValidTimeAfter currentDateTime |> Option.ofNullable)
