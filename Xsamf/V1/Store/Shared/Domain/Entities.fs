namespace Xsamf.V1.Store.Shared.Domain

module Entities =

    open Common

    type NewEntityRequest =
        { Reference: NewReferenceType
          TenantReference: string
          Name: string
          CreatedTimestamp: TimestampType }

    