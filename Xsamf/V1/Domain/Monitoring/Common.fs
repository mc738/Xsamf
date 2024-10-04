namespace Xsamf.V1.Domain.Monitoring

open System.Text.Json
open FsToolbox.Core
open FsToolbox.Extensions.Strings

[<AutoOpen>]
module Common =
    
    [<RequireQualifiedAccess>]
    type ActionOutcome =
        | CreateIncident
        | CloseIncident
        | CloseAllIncidentsForEntity
        | ActivateDmsWatcher
        | DeactivateDmsWatcher
        | ActivateActivityWatcher
        | DeactivateActivityWatcher
        | ActivateProbeWatcher
        | DeactivateProbeWatcher
    
    [<RequireQualifiedAccess>]
    type MonitoringAuthType =
        | None
        | Token of string
        
        
    [<RequireQualifiedAccess>]
    type HashAlgorithm =
        | SHA256
        | None
        
        static member Deserialize(str: string) =
            match str.ToLower() with
            | "sha256" -> HashAlgorithm.SHA256
            | "none" -> HashAlgorithm.None
            | _ -> HashAlgorithm.None
            
        member ha.Serialize() =
            match ha with
            | SHA256 -> "sha256"
            | None -> "none"
        
        member ha.HashString(str: string) =
            match ha with
            | SHA256 -> str.GetSHA256Hash()
            | None -> str
            
        
        //static member FromJson(element: JsonElement) =
        //    match element.tryGet
        //    
        //    
        //    ()