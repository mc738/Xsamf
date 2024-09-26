namespace Xsamf.V1.Store.SQLite

open Freql.Sqlite
open FsToolbox.Core.Results
open Xsamf.V1.Store.Shared

type SQLiteXsamfStore(ctx: SqliteContext) =



    interface IXsamfStore with

        member this.ExecuteInTransaction(fn) =
            // TODO this need to be check full
            ctx.ExecuteInTransaction(fun t -> SQLiteXsamfStore(t) :> IXsamfStore |> fn)
            |> Result.mapError (fun e -> e.Message)

        member this.GetDmsWatcher(reference) = failwith "todo"
        member this.ReaderTest() = failwith "todo"
        member this.WriteTest() = failwith "todo"
