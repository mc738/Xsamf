namespace Xsamf.V1.Store.Shared.Domain

module Tenants =

    open System
        
    type NewTenant =
        { Reference: string
          Name: string
          CreatedOn: DateTime option
          Metadata: Map<string, string>
          Tags: string list }

    type TenantDetails =
        { Reference: string
          Name: string
          Active: bool
          CreatedOn: DateTime
          UpdatedOn: DateTime
          Metadata: Map<string, string> }

    type TenantOverview = { Reference: string; Name: string }

    [<RequireQualifiedAccess>]
    type ListTenantsRequest =
        | Wildcard
        | WithTag of Tag: string
        | WithActiveStatus of Status: ListTenantsRequestActiveStatus
        | All of ListTenantsRequest list
        | Any of ListTenantsRequest list

    and [<RequireQualifiedAccess>] ListTenantsRequestActiveStatus =
        | Active
        | NotActive
        | Any

