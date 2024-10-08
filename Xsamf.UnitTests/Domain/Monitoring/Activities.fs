namespace Xsamf.UnitTests.Domain.Monitoring.Activities

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open Xsamf.V1.Domain.Monitoring
open Xsamf.V1.Domain.Monitoring.Activities

[<AutoOpen>]
module private Internal =

    let defaultActivity =
        ({ Reference = ""
           WatcherReference = ""
           Timestamp = DateTime.MinValue
           Auth = MonitoringAuthType.None
           Category = ActivityCategory.Information
           Metadata = Map.empty
           Tags = [] }
        : Activity)


[<TestClass>]
type ActivityDomainRulesTests() =

    [<TestMethod>]
    member _.``ActivityRule.IsCategory test when Activity has specified category``() =
        let activity =
            { defaultActivity with
                Category = ActivityCategory.Critical }

        let rule = ActivityRule.IsCategory ActivityCategory.Critical

        Assert.IsTrue(rule.Test activity)
        
    
    [<TestMethod>]
    member _.``ActivityRule.IsCategory test when Activity has not specified category``() =
        let activity =
            { defaultActivity with
                Category = ActivityCategory.Critical }

        let rule = ActivityRule.IsCategory ActivityCategory.Error

        Assert.IsFalse(rule.Test activity)
        
        

