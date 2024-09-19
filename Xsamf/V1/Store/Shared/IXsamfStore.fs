namespace Xsamf.V1.Store.Shared

open FsToolbox.Core.Results
open Xsamf.V1.Store.Shared.Domain

type IXsamfStore =
    
    abstract member AddTenant: NewTenant -> ActionResult<unit>
    
    abstract member GetTenant: Reference: string ->  FetchResult<TenantDetails>
    
    abstract member ListTenants: Request: ListTenantsRequest -> FetchResult<TenantOverview list>