namespace Xsamf.V1.Store.Shared.Domain

module Dms =

    open System
    open Common

    type NewDmsRequest =
        { TenantReference: string
          EntityReference: string
          Reference: NewReferenceType
          CreatedTimestamp: TimestampType
          CronPeriod: string          
          GracePeriod: TimeSpan
          Tags: string list }

    type DmsHeartBeat =
        { TenantReference: string
          Reference: string
          Timestamp: TimestampType }
