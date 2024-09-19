namespace Xsamf.V1.Store.Shared

open FsToolbox.Core.Results

type IXsamfStore =
    
    abstract member AddTenant: string -> ActionResult<unit>