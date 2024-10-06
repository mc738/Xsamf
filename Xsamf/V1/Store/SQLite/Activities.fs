namespace Xsamf.V1.Store.SQLite

[<RequireQualifiedAccess>]
module Activities =

    open System.Text
    open Freql.Sqlite
    open FsToolbox.Core.Results
    open Xsamf.V1.Common.Utils
    open Xsamf.V1.Domain.Monitoring
    open Xsamf.V1.Domain.Monitoring.Activities
    open Xsamf.V1.Store.Shared.Domain.Common
    open Xsamf.V1.Store.SQLite.Persistence

    module Internal =

        type WatcherOverview =
            { WatcherId: string
              WatcherActive: bool
              EntityId: string
              EntityActive: bool
              TenantId: string
              TenantActive: bool }

            static member Sql() =
                """
                SELECT
                    w.id AS watcher_id,
                    w.active AS watcher_active,
                    e.id AS entity_id,
                    e.active AS entity_active,
                    t.id AS tenant_id,
                    t.active AS tenant_active
                FROM activity_watchers w
                         LEFT JOIN entities e ON w.entity_id = e.id
                         LEFT JOIN tenants t ON e.tenant_id = t.id
                WHERE w.id = @0;
                """

        let getWatcherOverview (ctx: SqliteContext) (watcherId: string) =
            ctx.SelectSingleAnon<WatcherOverview>(WatcherOverview.Sql(), [ watcherId ])

        ()

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
        |> FetchResult.bind (fun ahv -> ahv.HasherBlob.Convert ActivityHasher.Deserialize |> FetchResult.fromResult)

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
            av.RuleBlob.Convert ActivityRule.Deserialize
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

    let getWatcherVersionActions (ctx: SqliteContext) (watcherVersionId: string) (version: ItemVersion) =
        Operations.selectActivityActionRecords ctx [ "WHERE watcher_version_id = @0" ] [ watcherVersionId ]
        |> List.map (fun aa -> getActionVersion ctx aa.WatcherVersionId version)
        |> FetchResult.ifAllSuccess "Failed to create watcher version actions"

    let getWatcherVersion (ctx: SqliteContext) (watcherId: string) (version: ItemVersion) =
        let (conditions, parameters) =
            match version with
            | ItemVersion.Latest -> [ "WHERE watcher_id = @0 ORDER BY version DESC LIMIT 1" ], [ box watcherId ]
            | ItemVersion.Specific version ->
                [ "WHERE watcher_id = @0 AND version = @1" ], [ box watcherId; box version ]

        Internal.getWatcherOverview ctx watcherId
        |> Option.bind (fun aw ->
            Operations.selectActivityWatcherVersionRecord ctx conditions parameters
            |> Option.map (fun awv -> aw, awv))
        |> FetchResult.fromOption "Failed to get watcher version"
        |> FetchResult.merge (fun (aw, awv) aas -> aw, awv, aas) (fun (aw, awv) ->
            getWatcherVersionActions ctx awv.Id version)
        |> FetchResult.map (fun (aw, awv, aas) ->
            ({ Reference = aw.WatcherId
               Name = awv.Name
               TenantReference = aw.TenantId
               EntityReference = aw.EntityId
               Actions = aas
               AdditionMetadata =
                 Operations.selectActivityWatcherVersionMetadataItemRecords ctx [ "WHERE version_id = @0" ] [ awv.Id ]
                 |> List.map (fun amd -> amd.ItemKey, amd.ItemValue)
                 |> Map.ofList
               AdditionTags =
                 Operations.selectActivityWatcherVersionTagsRecords ctx [ "WHERE version_id = @0" ] [ awv.Id ]
                 |> List.map (fun at -> at.Tag) }
            : ActivityWatcher))