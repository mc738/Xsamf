module Xsamf.V1.Store.SQLite

open System.Text
open Xsamf.V1.Domain.Monitoring.Activities

[<RequireQualifiedAccess>]
module Activities =

    open Freql.Sqlite
    open FsToolbox.Core.Results
    open Xsamf.V1.Common.Utils
    open Xsamf.V1.Domain.Monitoring.Activities
    open Xsamf.V1.Store.Shared.Domain.Common
    open Xsamf.V1.Store.SQLite.Persistence

    let getActionVersion (ctx: SqliteContext) (action: Records.ActivityAction) (version: ItemVersion) =
        let (conditions, parameters) =
            match version with
            | ItemVersion.Latest -> [ "WHERE action_id = @0 ORDER BY version DESC LIMIT 1" ], [ box action.Id ]
            | ItemVersion.Specific version ->
                [ "WHERE action_id = @0 AND version = @1" ], [ box action.Id; box version ]


        Operations.selectActivityActionVersionRecord ctx conditions parameters
        |> FetchResult.fromOption "Failed to find action version"
        |> FetchResult.map (fun av ->
            Operations.selectActivityHasherVersionRecord ctx [ "WHERE id = @0" ] [ av.HasherVersionId ]
            |> FetchResult.fromOption "Failed to find actvitiy hasher version"
            |> FetchResult.map (fun ahv ->
                ahv.HasherBlob.ToBytes()
                |> Encoding.UTF8.GetString
                |> ActivityHasher
                
                )
            
            )
        |> FetchResult.bind (fun av ->
            av.RuleBlob.ToBytes()
            |> Encoding.UTF8.GetString
            |> ActivityRule.Deserialize
            |> Result.map (fun ar ->
                ({ Reference = av.Id
                   Name = av.Name
                   Rule = ar
                   Outcomes = failwith "todo"
                   Hasher = failwith "todo"
                   AdditionMetadata =
                     Operations.selectActivityActionVersionMetadataItemRecords
                         ctx
                         [ "WHERE version_id = @0" ]
                         [ av.Id ]
                     |> List.map (fun md -> md.ItemKey, md.ItemValue)
                     |> Map.ofList
                   AdditionTags =
                     Operations.selectActivityActionVersionTagRecords ctx [ "WHERE version_id = @0" ] [ av.Id ]
                     |> List.map (fun t -> t.Tag) }
                : ActivityAction))
            |> FetchResult.fromResult)

    let getWatcherVersion (ctx: SqliteContext) (watcher: Records.ActivityWatcher) (version: ItemVersion) =
        let (conditions, parameters) =
            match version with
            | ItemVersion.Latest -> [ "WHERE watcher_id = @0 ORDER BY version DESC LIMIT 1" ], [ box watcher.Id ]
            | ItemVersion.Specific version ->
                [ "WHERE watcher_id = @0 AND version = @1" ], [ box watcher.Id; box version ]

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
                 Operations.selectActivityWatcherVersionMetadataItemRecords ctx [ "WHERE version_id = @0" ] [ awv.Id ]
                 |> List.map (fun amd -> amd.ItemKey, amd.ItemValue)
                 |> Map.ofList
               AdditionTags =
                 Operations.selectActivityWatcherVersionTagsRecords ctx [ "WHERE version_id = @0" ] [ awv.Id ]
                 |> List.map (fun at -> at.Tag) }
            : ActivityWatcher))
        |> FetchResult.fromOption "Failed to get watcher version"


    let getWatcher (ctx: SqliteContext) (reference: string) (version: ItemVersion) =



        ()
