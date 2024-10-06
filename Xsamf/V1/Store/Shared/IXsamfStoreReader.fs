namespace Xsamf.V1.Store.Shared

open FsToolbox.Core.Results
open Xsamf.V1.Domain.Monitoring.Activities
open Xsamf.V1.Store.Shared.Domain
open Xsamf.V1.Store.Shared.Domain.Common
open Xsamf.V1.Store.Shared.Domain.HeartBeats

type IXsamfStoreReader =
    
    abstract member ReaderTest: unit -> unit
    
    abstract member GetHeartBeatWatcher: Reference: string -> FetchResult<HeartBeatWatcherDetails> 
    
    abstract member GetActivityWatcher: Reference: string * Version: ItemVersion -> FetchResult<ActivityWatcher>
    
    
    
    //abstract member 