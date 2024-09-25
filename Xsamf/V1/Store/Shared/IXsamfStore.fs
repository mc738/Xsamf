namespace Xsamf.V1.Store.Shared

open Freql.Core.Common.Types
open FsToolbox.Core.Results

type IXsamfStore =

    inherit IXsamfStoreReader
    inherit IXsamfStoreWriter
    
    abstract ExecuteInTransaction<'T>: Fn: (IXsamfStore -> 'T) -> Result<'T, string>
