namespace Xsamf.V1.Domain.Monitoring

open System.Text.Json

[<AutoOpen>]
module Common =
    
    [<RequireQualifiedAccess>]
    type ActionOutcome =
        | CreateIncident
        | CloseIncident
    
    [<RequireQualifiedAccess>]
    type MonitoringAuthType =
        | None
        | Token of string
        
        //static member FromJson(element: JsonElement) =
        //    match element.tryGet
        //    
        //    
        //    ()