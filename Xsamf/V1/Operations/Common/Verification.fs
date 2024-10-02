namespace Xsamf.V1.Operations.Common

open FsToolbox.Core.Results

[<RequireQualifiedAccess>]
type VerificationResult =
    | Success
    | MissingPermission of Name: string
    | WrongTenant
    | ItemInactive of ItemType: string
    | Failure of FailureResult

    member vr.ToResult() =
        match vr with
        | Success -> Ok()
        | MissingPermission name ->
            ({ Message = $"`{name}` permission is missing."
               DisplayMessage = "A permission is missing."
               Exception = None }
            : FailureResult)
            |> Error
        | WrongTenant ->
            ({ Message = "Item's tenant is wrong"
               DisplayMessage = "Item's tenant is wrong"
               Exception = None }
            : FailureResult)
            |> Error
        | ItemInactive itemType ->
            ({ Message = $"{itemType} is inactive"
               DisplayMessage = $"{itemType} is inactive"
               Exception = None }
            : FailureResult)
            |> Error
        | Failure failure -> Error failure

module VerificationResult =

    let chain (fn: unit -> VerificationResult) (result: VerificationResult) =
        match result with
        | VerificationResult.Success -> fn ()
        | _ -> result

    let toResult<'T> (value: 'T) (verificationResult: VerificationResult) =
        verificationResult.ToResult() |> Result.map (fun _ -> value)

    let verify<'T> (verifiers: (unit -> VerificationResult) list) (value: 'T) =
        verifiers
        |> List.fold
            (fun r v ->
                match r with
                | VerificationResult.Success -> v ()
                | _ -> r)
            (VerificationResult.Success)
        |> toResult value

[<RequireQualifiedAccess>]
module Verification =
    
    let isTrue (onFalse: FailureResult) (result: bool) () =
        match result with
        | true -> VerificationResult.Success
        | false -> VerificationResult.Failure onFalse

    let isFalse (onTrue: FailureResult) (result: bool) () =
        match result with
        | true -> VerificationResult.Failure onTrue
        | false -> VerificationResult.Success
    
    //let stringMatches 
    
    //let tenantMatches = ()
    
    ()

