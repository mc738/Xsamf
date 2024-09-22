namespace Xsamf.V1.Store.Shared

open FsToolbox.Core.Results
open Xsamf.V1.Store.Shared.Domain
open Xsamf.V1.Store.Shared.Domain.Dms

type IXsamfStoreReader =
    
    abstract member ReaderTest: unit -> unit
    
    abstract member GetDmsWatcher: Reference: string -> FetchResult<DmsWatcherDetails> 
    
    //abstract member 