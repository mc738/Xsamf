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

    let tryGetNextExecutionDate (validAfter: DateTime) (cronExpression: string) =
        tryParse cronExpression
        |> Option.bind (fun ce -> ce.GetNextValidTimeAfter validAfter |> Option.ofNullable)
