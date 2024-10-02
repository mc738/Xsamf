namespace Xsamf.V1.Operations.Monitoring.Dms

open FsToolbox.Core.Results
open Xsamf.V1.Common
open Xsamf.V1.Domain.Monitoring.Dms
open Xsamf.V1.Operations.Common
open Xsamf.V1.Store.Shared

[<RequireQualifiedAccess>]
module UpdateOperations =

    let checkIn (store: IXsamfStore) (checkIn: DmsCheckIn) =
        // Fetch the dms watcher
        //store.

        ()

    let anonymousCheckIn (store: IXsamfStore) (checkIn: AnonymousDmsCheckIn) =
        store.ExecuteInTransaction(fun t ->
            t.GetDmsWatcher checkIn.WatcherReference
            |> FetchResult.toResult
            |> Result.bind (fun dw ->
                let verifiers =
                    [ Verification.isTrue
                          ({ Message = "Watcher does not allow anonymous check ins"
                             DisplayMessage = "Watcher does not allow anonymous check ins"
                             Exception = None }
                          : FailureResult)
                          dw.AllowAnonymous ]

                VerificationResult.verify verifiers dw)
            |> Result.bind (fun dw ->
                match
                    t.DmsCheckIn
                        { Reference = checkIn.Reference
                          WatcherReference = dw.Reference
                          Timestamp = checkIn.Timestamp
                          Metadata = checkIn.Metadata
                          Tags = [] }
                with
                | ActionResult.Success unit ->
                    // For the new next check in date first try and create it from the last new check-in date.
                    // However, if that is in the past create it from the current datetime
                    let nextCheckInTimestamp = Cron.tryParse dw.ScheduleCron
                    
                    
                    
                    
                    // Add events.
                    
                    Ok()
                | ActionResult.Failure failureResult -> Error failureResult))
        |> toActionResult "Anonymous DMS check in"


    //|> ActionResult.fromResult

    ()
