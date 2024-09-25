namespace Xsamf.V1.Operations.Monitoring.Activities

open FsToolbox.Core.Results
open Xsamf.V1.Domain.Monitoring.Activities
open Xsamf.V1.Store.Shared
open Xsamf.V1.Operations.Common

module CreateOperations =
    
    let addActivity (store: IXsamfStore) (activity: Activity) : ActionResult<unit> =
        
        failwith ""
    
    let addActivityTransaction (store: IXsamfStore) (activity: Activity) : ActionResult<unit> =
        store.ExecuteInTransaction (fun t -> addActivity t activity)
        |> unwrapTransactionActionResult "Add activity"
        

