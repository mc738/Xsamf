namespace Xsamf.V1.Operations.Common

open FsToolbox.Core.Results

[<AutoOpen>]
module Shared =
    
    let toActionResult<'T> (name: string) (result: Result<Result<'T, FailureResult>, string>) =
        match result with
        | Ok r1 -> ActionResult.fromResult r1
        | Error e ->
            ({ Message = $"{name} action failed: {e}"
               DisplayMessage = $"{name} action failed"
               Exception = None }
            : FailureResult)
            |> ActionResult.Failure

    let unwrapTransactionActionResult<'T> (name: string)  (result: Result<ActionResult<'T>, string>) =
        match result with
        | Ok r -> r
        | Error e ->
            ({ Message = $"{name} transaction action failed: {e}"
               DisplayMessage = $"{name} action failed"
               Exception = None }
            : FailureResult)
            |> ActionResult.Failure
        