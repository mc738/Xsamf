namespace Xsamf.V1.Store.SQLite

open Freql.Sqlite
open FsToolbox.Core.Results
open Xsamf.V1.Domain.Monitoring.Activities
open Xsamf.V1.Store.Shared
open Xsamf.V1.Store.SQLite.Persistence
open Xsamf.V1.Store.Shared.Domain.Common

type SQLiteXsamfStore(ctx: SqliteContext) =



    interface IXsamfStore with

        member this.ExecuteInTransaction(fn) =
            // TODO this need to be check full
            ctx.ExecuteInTransaction(fun t -> SQLiteXsamfStore(t) :> IXsamfStore |> fn)
            |> Result.mapError (fun e -> e.Message)

        member this.GetDmsWatcher(reference) = failwith "todo"
        member this.ReaderTest() = failwith "todo"
        member this.WriteTest() = failwith "todo"
        member this.DmsCheckIn(var0) = failwith "todo"

        member this.GetActivityWatcher(reference, version) =
            let (conditions, parameters) =
                match version with
                | ItemVersion.Latest -> [ "WHERE watcher_id = @0 ORDER BY version DESC LIMIT 1" ], [ box reference ]
                | ItemVersion.Specific version ->
                    [ "WHERE watcher_id = @0 AND version = @1" ], [ box reference; box version ]

            Operations.selectActivityWatcherRecord ctx [ "WHERE id = @0" ] [ id ]
            |> Option.bind (fun aw ->
                Operations.selectActivityWatcherVersionRecord ctx conditions parameters
                |> Option.map (fun awv -> aw, awv))
            |> Option.map (fun (aw, awv) ->
                ({ Reference = aw.Id
                   Name = awv.Name
                   TenantReference = ""
                   EntityReference = aw.EntityId
                   Actions = failwith "todo"
                   AdditionMetadata =
                     Operations.selectActivityWatcherVersionMetadataItemRecords
                         ctx
                         [ "WHERE version_id = @0" ]
                         [ awv.Id ]
                     |> List.map (fun amd -> amd.ItemKey, amd.ItemValue)
                     |> Map.ofList
                   AdditionTags =
                     Operations.selectActivityWatcherVersionTagsRecords ctx [ "WHERE version_id = @0" ] [ awv.Id ]
                     |> List.map (fun at -> at.Tag) }
                : ActivityWatcher)
                |> FetchResult.Success)
            |> Option.defaultWith (fun _ ->
                { Message = ""
                  DisplayMessage = ""
                  Exception = None }
                |> FetchResult.Failure)
