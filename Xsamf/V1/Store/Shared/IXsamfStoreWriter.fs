namespace Xsamf.V1.Store.Shared

open FsToolbox.Core.Results
open Xsamf.V1.Domain.Monitoring.Events
open Xsamf.V1.Store.Shared.Domain.HeartBeats

type IXsamfStoreWriter =


    abstract member WriteTest: unit -> unit


    abstract member HeartBeatCheckIn: NewHeartBeatCheckIn -> ActionResult<unit>

    abstract member AddMonitoringEvents: TenantReference: string * Events: MonitoringEvent list -> ActionResult<unit>
