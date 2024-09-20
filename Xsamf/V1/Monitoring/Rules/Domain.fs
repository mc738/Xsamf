namespace Xsamf.V1.Monitoring.Rules

module Domain =

    [<RequireQualifiedAccess>]
    type RuleCondition<'T> =
        | OnSuccess
        | OnFailure
        | Predict of fn: ('T -> bool)
        | Not of RuleCondition<'T>
        | And of RuleCondition<'T> * RuleCondition<'T>
        | Or of RuleCondition<'T> * RuleCondition<'T>
        | Any of RuleCondition<'T> list
        | All of RuleCondition<'T> list

        member rc.Test(value: 'T, wasSuccess: bool) =
            match rc with
            | OnSuccess -> wasSuccess
            | OnFailure -> wasSuccess |> not
            | Predict fn -> fn value
            | Not ruleCondition -> ruleCondition.Test(value, wasSuccess) |> not
            | And(a, b) -> a.Test(value, wasSuccess) && b.Test(value, wasSuccess)
            | Or(a, b) -> a.Test(value, wasSuccess) && b.Test(value, wasSuccess)
            | Any ruleConditions -> ruleConditions |> List.exists (fun r -> r.Test(value, wasSuccess))
            | All ruleConditions -> ruleConditions |> List.exists (fun r -> r.Test(value, wasSuccess) |> not)

    [<RequireQualifiedAccess>]
    type RuleOutcome =
        | CreateIncident
        | CloseIncident
