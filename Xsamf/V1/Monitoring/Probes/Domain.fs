namespace Xsamf.V1.Monitoring.Probes

open System

module Domain =

    type ProbeType = Http of Settings: ProbeHttpSettings

    and ProbeHttpSettings =
        { Url: string
          AdditionalHeaders: Map<string, string> }

    type Probe =
        { Id: string
          Name: string
          Type: ProbeType
          ScheduleCron: string
          LastRun: DateTime
          Tags: string list }
