namespace Xsamf.V1.Store.Shared.Domain

open Xsamf.V1.Domain.Monitoring
open Xsamf.V1.Store.Shared.Domain.Common

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

    type NewDmsCheckIn =
        { WatcherReference: string
          Reference: string //NewReferenceType
          Timestamp: DateTime //TimestampType
          Tags: string list
          Metadata: Map<string, string> }
        
    
        
    type DmsWatcherDetails =
        {
            TenantReference: string
            EntityReference: string
            Reference: string
            Name: string
            PreviousCheckInTime: DateTime
            NextCheckInTime: DateTime
            ScheduleCron: string
            GracePeriod: TimeSpan
            Auth: MonitoringAuthType
            AllowAnonymous: bool
            Tags: string list
            /// <summary>
            /// A collection of addition metadata that will be added to an activity if the action is triggered.
            /// These will be used for any future processing from any rule hits from this watcher.
            /// </summary>
            AdditionMetadata: Map<string, string>
            /// <summary>
            /// A list of addition tags that will be added to an activity if the action is triggered.
            /// These will be used for any future processing from any rule hits from this watcher.
            /// </summary>
            AdditionTags: string list
        }
    
    type DmsWatcherAction =
        {
            Reference: string
            Name: string
            
        }
        
    //type DmsWatcherAction =
    //    {
            
    //    }
