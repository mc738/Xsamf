namespace Xsamf.V1.Services.Monitoring

open FsToolbox.Core.Results
open Xsamf.V1.Domain.Monitoring.HeartBeats
open Xsamf.V1.Operations.Monitoring.Dms
open Xsamf.V1.Store.Shared


type HeartBeatService(store: IXsamfStore) =
    
    
    member _.AnonymousCheckIn(checkIn: AnonymousHeartBeatCheckIn) =
        // First get the watcher
        ReadOperations.getWatcher store checkIn.WatcherReference
        |> FetchResult.toActionResult
        |> ActionResult.bind (fun watcher ->
            
            
            
            ActionResult.Success ())
        
        
        // First try and add the check in
        
        
        //()

