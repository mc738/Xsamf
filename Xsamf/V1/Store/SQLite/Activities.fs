module Xsamf.V1.Store.SQLite

open System.Text
open Xsamf.V1.Domain.Monitoring
open Xsamf.V1.Domain.Monitoring.Activities
open Xsamf.V1.Store.Shared.Domain.Common

[<RequireQualifiedAccess>]
module Activities =

    open Freql.Sqlite
    open FsToolbox.Core.Results
    open Xsamf.V1.Common.Utils
    open Xsamf.V1.Domain.Monitoring.Activities
    open Xsamf.V1.Store.Shared.Domain.Common
    open Xsamf.V1.Store.SQLite.Persistence

    let getActionVersionOutcomes (ctx: SqliteContext) (actionVersionId: string) (version: ItemVersion) =
        Operations.selectActivityActionOutcomeRecords ctx [ "WHERE action_version_id = @0" ] [ actionVersionId ]
        |> List.map (fun ao ->
            let (conditions, parameters) =
                match version with
                | ItemVersion.Latest -> [ "WHERE outcome_id = @0 ORDER BY version DESC LIMIT 1" ], [ box ao.Id ]
                | ItemVersion.Specific version ->
                    [ "WHERE outcome_id = @0 AND version = @1" ], [ box ao.Id; box version ]

            Operations.selectActivityActionOutcomeVersionRecord ctx conditions parameters
            |> FetchResult.fromOption "Failed to find activity action outcome version"
            |> FetchResult.bind (fun aov ->
                aov.OutcomeBlob.Convert ActionOutcome.Deserialize |> FetchResult.fromResult))
        |> FetchResult.ifAllSuccess "Failed to deserialize activity action outcome version"


    let getActivityHasher (ctx: SqliteContext) (versionId: string) =
        Operations.selectActivityHasherVersionRecord ctx [ "WHERE id = @0" ] [ versionId ]
        |> FetchResult.fromOption "Failed to find activity hasher version"
        |> FetchResult.bind (fun ahv ->
            ahv.HasherBlob.ToBytes()
            |> Encoding.UTF8.GetString
            |> ActivityHasher.Deserialize
            |> FetchResult.fromResult)

    let getActionVersion (ctx: SqliteContext) (actionId: string) (version: ItemVersion) =
        let (conditions, parameters) =
            match version with
            | ItemVersion.Latest -> [ "WHERE action_id = @0 ORDER BY version DESC LIMIT 1" ], [ box actionId ]
            | ItemVersion.Specific version -> [ "WHERE action_id = @0 AND version = @1" ], [ box actionId; box version ]


        Operations.selectActivityActionVersionRecord ctx conditions parameters
        |> FetchResult.fromOption "Failed to find action version"
        |> FetchResult.merge (fun av ahv -> av, ahv) (fun av -> getActivityHasher ctx av.HasherVersionId)
        |> FetchResult.merge (fun (av, ahv) aos -> av, ahv, aos) (fun (av, _) ->
            getActionVersionOutcomes ctx av.Id ItemVersion.Latest)
        |> FetchResult.bind (fun (av, ahv, aos) ->
            av.RuleBlob.ToBytes()
            |> Encoding.UTF8.GetString
            |> ActivityRule.Deserialize
            |> Result.map (fun ar ->
                ({ Reference = av.Id
                   Name = av.Name
                   Rule = ar
                   Outcomes = aos
                   Hasher = ahv
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
