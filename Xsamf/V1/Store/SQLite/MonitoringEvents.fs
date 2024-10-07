namespace Xsamf.V1.Store.SQLite

open System
open System.IO
open Freql.Core.Common.Types
open Xsamf.V1.Domain.Monitoring.Events


module MonitoringEvents =

    open Freql.Sqlite
    open FsToolbox.Extensions.Strings
    open Xsamf.V1.Store.SQLite.Persistence

    let addEvents (ctx: SqliteContext) (tenantId: string) (events: MonitoringEvent list) =
        events
        |> List.iter (fun e ->
            match e.TrySerialize() with
            | Ok(eventType, data) ->

                use ms = new MemoryStream()

                ({ Id = Guid.NewGuid().ToString("n")
                   TenantId = tenantId
                   EventType = eventType
                   EventBlob = BlobField.FromStream ms
                   EventBlobHash = data.GetSHA256Hash()
                   CreatedOn = DateTime.UtcNow }
                : Parameters.NewMonitoringEvents)
                |> Operations.insertMonitoringEvents ctx
            | Error e ->
                // TODO handle errors?
                ())
