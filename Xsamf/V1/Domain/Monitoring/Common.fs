namespace Xsamf.V1.Domain.Monitoring

[<AutoOpen>]
module Common =
    
    [<RequireQualifiedAccess>]
    type ActionOutcome =
        | CreateIncident
        | CloseIncident
    
    [<RequireQualifiedAccess>]
    type AuthType =
        | None
        | Token of string