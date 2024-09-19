namespace Xsamf.V1.Common

module Utils =

    open System
    
    let createReference () = Guid.NewGuid().ToString("n")
    
    let createTimestamp () = DateTime.UtcNow
