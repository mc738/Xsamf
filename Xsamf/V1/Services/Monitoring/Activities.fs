namespace Xsamf.V1.Services.Monitoring

open System
open FsToolbox.Core.Results
open Xsamf.V1.Domain.Monitoring.Activities
open Xsamf.V1.Store.Shared
open Xsamf.V1.Operations.Monitoring
open Xsamf.V1.Common.Utils

type ActivitiesService(store: IXsamfStore) =

    member _.HandleActivity(activity: Activity, bespokeHandlers: Map<string, Activity -> bool>) =
        Activities.ReadOperations.getWatcherForActivity store activity
        |> ActionResult.fromFetchResult
        |> ActionResult.ifSuccessful (fun _ -> Activities.CreateOperations.addActivity store activity)
        |> ActionResult.bind (fun watcher ->
            let filteredResults = watcher.HandleActivity(activity, bespokeHandlers)

            match filteredResults.IsEmpty with
            | true -> ()
            | false ->
                //let tags = activity.Tags @ watcher.AdditionTags
                //let metadata = activity.Metadata |> Map.merge watcher.AdditionMetadata

                filteredResults
                |> List.map (fun result ->
                    // Handle the results
                    //let resultTags = tags @ result.AdditionTags
                    //let resultMetadata = activity.Metadata |> Map.merge watcher.AdditionMetadata
                    
                    

                    ())

                ()


            // Mark the activity as handled.
            failwith "")
