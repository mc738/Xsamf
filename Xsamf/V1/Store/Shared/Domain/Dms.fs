namespace Xsamf.V1.Store.Shared.Domain

module Dms =

    open Common

    type NewDmsRequest =
        { TenantReference: string
          EntityReference: string
          Reference: NewReferenceType
          CreatedTimestamp: TimestampType }

    type DmsHeartBeat =
        { TenantReference: string
          Reference: string }
