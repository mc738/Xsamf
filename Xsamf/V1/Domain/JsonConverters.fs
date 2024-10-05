namespace Xsamf.V1.Domain

open Xsamf.V1.Domain.Monitoring
open Xsamf.V1.Domain.Monitoring.Activities

module JsonConverters =

    open System.Text.Json
    open System.Text.Json.Serialization
    open Xsamf.V1.Domain.Monitoring.Dms
 
    type ActionOutcomeConverter() =
        inherit JsonConverter<ActionOutcome>()
        
        override this.Read(reader, typeToConvert, options) =
            if reader.TokenType <> JsonTokenType.StartObject then
                raise (JsonException())

            use doc = JsonDocument.ParseValue(&reader)

            match ActionOutcome.FromJson doc.RootElement with
            | Ok outcome -> outcome
            | Error errorValue -> raise (JsonException(errorValue))

        override this.Write(writer, value, options) = value.WriteToJsonValue writer
    
    type DmsRuleConverter() =

        inherit JsonConverter<DmsRule>()

        override this.Read(reader, typeToConvert, options) =
            if reader.TokenType <> JsonTokenType.StartObject then
                raise (JsonException())

            use doc = JsonDocument.ParseValue(&reader)

            match DmsRule.FromJson doc.RootElement with
            | Ok rule -> rule
            | Error errorValue -> raise (JsonException(errorValue))

        override this.Write(writer, value, options) = value.WriteToJsonValue writer

    type ActivityRuleConverter() =

        inherit JsonConverter<ActivityRule>()

        override this.Read(reader, typeToConvert, options) =
            if reader.TokenType <> JsonTokenType.StartObject then
                raise (JsonException())

            use doc = JsonDocument.ParseValue(&reader)

            match ActivityRule.FromJson doc.RootElement with
            | Ok rule -> rule
            | Error errorValue -> raise (JsonException(errorValue))

        override this.Write(writer, value, options) = value.WriteToJsonValue writer
        
    type ActivityHasherConverter() =

        inherit JsonConverter<ActivityHasher>()

        override this.Read(reader, typeToConvert, options) =
            if reader.TokenType <> JsonTokenType.StartObject then
                raise (JsonException())

            use doc = JsonDocument.ParseValue(&reader)

            match ActivityHasher.FromJson doc.RootElement with
            | Ok hasher -> hasher
            | Error errorValue -> raise (JsonException(errorValue))

        override this.Write(writer, value, options) = value.WriteToJsonValue writer
