namespace Xsamf.V1.Store.Shared

open FsToolbox.Core.Results
open Xsamf.V1.Domain.Monitoring.Activities
open Xsamf.V1.Store.Shared.Domain
open Xsamf.V1.Store.Shared.Domain.Common
open Xsamf.V1.Store.Shared.Domain.Dms

type IXsamfStoreReader =
    
    abstract member ReaderTest: unit -> unit
    
    abstract member GetDmsWatcher: Reference: string -> FetchResult<DmsWatcherDetails> 
    
    abstract member GetActivityWatcher: Reference: string * Version: ItemVersion -> FetchResult<ActivityWatcher>
    
    
    
    //abstract member 