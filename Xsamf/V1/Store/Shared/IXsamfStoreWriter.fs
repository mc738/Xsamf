namespace Xsamf.V1.Store.Shared

open FsToolbox.Core.Results
open Xsamf.V1.Store.Shared.Domain.HeartBeats

type IXsamfStoreWriter =
    
    
    abstract member WriteTest: unit -> unit
    
    
    abstract member HeartBeatCheckIn: NewHeartBeatCheckIn -> ActionResult<unit>
    
    