namespace Xsamf.V1.Operations.Monitoring.Activities

open Xsamf.V1.Store.Shared

module ReadOperations =

    open FsToolbox.Core.Results
    open Xsamf.V1.Domain.Monitoring.Activities
    open Xsamf.V1.Store.Shared
        
    let getWatcherForActivity (reader: IXsamfStoreReader) (activity: Activity) : FetchResult<ActivityWatcher> =
        
        
        failwith "TODO"

