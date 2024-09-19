namespace Xsamf.V1.Store.Shared.Domain

module Incidents =
    
    type NewIncident =
        {
            Reference: string
            TenantReference: string
            /// <summary>
            /// A unique identifier for an incident.
            /// This is used to monitor ongoing incidents,
            /// so should be deterministically generated.
            ///
            /// This means multiple incidents (such as different failures) can be recorded from 
            ///
            /// A simple example might be using an exit code has the hash,
            /// so if a program exits with a different code it will be treated as different incidents.  
            /// </summary>
            Hash: string
        }
        
    //type CloseIcidnets =
    
    ()

