namespace Xsamf.V1.Store.Shared

open FsToolbox.Core.Results
open Xsamf.V1.Store.Shared.Domain.Dms

type IXsamfStoreWriter =
    
    
    abstract member WriteTest: unit -> unit
    
    
    abstract member DmsCheckIn: NewDmsCheckIn -> ActionResult<unit>
    
    