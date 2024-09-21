namespace Xsamf.V1.Domain.Monitoring

open System

module Activities =

    type ActivityCategory =
        | Information = 0
        | Success = 1
        | Warning = 2
        | Error = 3
        | Critical = 4

    type Activity =
        { Reference: string
          WatcherReference: string
          Timestamp: DateTime
          // TODO add more
          Category: ActivityCategory
          Metadata: Map<string, string>
          Tags: string list }

    [<RequireQualifiedAccess>]
    type ActivityRule =
        | IsCategory of Category: ActivityCategory
        | HasTag of Tag: string
        | HasTags of Tags: string list
        | HasMetadata of Key: string
        | MetadataValueEquals of Key: string * Value: string
        | Bespoke of HandlerName: string
        | Not of Rule: ActivityRule
        | And of RuleA: ActivityRule * RuleB: ActivityRule
        | Or of RuleA: ActivityRule * RuleB: ActivityRule
        | All of Rules: ActivityRule list
        | Any of Rules: ActivityRule list

        member ar.Test(activity: Activity, bespokeHandlers: Map<string, Activity -> bool>) =
            match ar with
            | IsCategory category -> activity.Category = category
            | HasTag tag -> failwith "todo"
            | HasTags tags -> failwith "todo"
            | HasMetadata key -> failwith "todo"
            | MetadataValueEquals(key, value) -> failwith "todo"
            | Bespoke handlerName ->
                bespokeHandlers.TryFind handlerName
                |> Option.map (fun h -> h activity)
                |> Option.defaultValue false
            | Not rule -> failwith "todo"
            | And(ruleA, ruleB) -> failwith "todo"
            | Or(ruleA, ruleB) -> failwith "todo"
            | All rules -> failwith "todo"
            | Any rules -> failwith "todo"

        member ar.Test(activity: Activity) = ar.Test(activity, Map.empty)

    type ActivityOutcome =
        | CreateIncident
        | CloseIncident

    type ActivityAction =
        { Name: string
          Rule: ActivityRule
          Outcomes: ActivityOutcome list
          /// <summary>
          /// A collection of addition metadata that will be added to an activity if the action is triggered.
          /// These will be used for any future processing from this action specifically.
          /// </summary>
          AdditionMetadata: Map<string, string>
          /// <summary>
          /// A list of addition tags that will be added to an activity if the action is triggered.
          /// These will be used for any future processing from this action specifically.
          /// </summary>
          AdditionTags: string list }

        member aa.Handle(activity: Activity, bespokeHandlers: Map<string, Activity -> bool>) =
            match aa.Rule.Test(activity, bespokeHandlers) with
            | true ->
                ({ Name = aa.Name
                   Outcomes = aa.Outcomes
                   AdditionMetadata = aa.AdditionMetadata
                   AdditionTags = aa.AdditionTags }
                : ActivityActionResult)
            | false -> ActivityActionResult.Empty aa.Name

    and ActivityActionResult =
        { Name: string
          Outcomes: ActivityOutcome list
          AdditionMetadata: Map<string, string>
          AdditionTags: string list }

        static member Empty(name: string) =
            { Name = name
              Outcomes = []
              AdditionMetadata = Map.empty
              AdditionTags = [] }

        member aar.HasOutcomes() = aar.Outcomes.IsEmpty |> not

    type ActivityWatcher =
        { Reference: string
          TenantReference: string
          EntityReference: string
          Actions: ActivityAction list
          /// <summary>
          /// A collection of addition metadata that will be added to an activity if the action is triggered.
          /// These will be used for any future processing from any rule hits from this watcher.
          /// </summary>
          AdditionMetadata: Map<string, string>
          /// <summary>
          /// A list of addition tags that will be added to an activity if the action is triggered.
          /// These will be used for any future processing from any rule hits from this watcher.
          /// </summary>
          AdditionTags: string list  }

        member aw.HandleActivity(activity, bespokeHandlers: Map<string, Activity -> bool>, filterEmpty: bool) =
            aw.Actions
            |> List.map (fun a -> a.Handle(activity, bespokeHandlers))
            |> fun r ->
                if filterEmpty then
                    r |> List.filter (fun aar -> aar.HasOutcomes())
                else
                    r
