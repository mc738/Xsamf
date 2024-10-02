namespace Xsamf.V1.Store.Shared.Domain

module Common =

    open System
    open Xsamf.V1.Common

    [<RequireQualifiedAccess>]
    type NewReferenceType =
        | Generated
        | Specific of string

        member nrt.Generate() =
            match nrt with
            | Generated -> Utils.createReference () |> NewReferenceType.Specific
            | Specific _ -> nrt

        member nrt.GetValue() =
            match nrt with
            | Generated -> Utils.createReference ()
            | Specific s -> s

    type TimestampType =
        | Generated
        | Specific of DateTime

        member tt.Generate() =
            match tt with
            | Generated -> DateTime.UtcNow |> TimestampType.Specific
            | Specific _ -> tt

        member tt.GetValue() =
            match tt with
            | Generated -> DateTime.UtcNow
            | Specific dt -> dt

    [<RequireQualifiedAccess>]
    type ItemVersion =
        | Latest
        | Specific of Version: int