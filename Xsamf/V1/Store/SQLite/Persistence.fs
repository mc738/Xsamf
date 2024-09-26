namespace Xsamf.V1.Store.SQLite.Persistence

open System
open System.Text.Json.Serialization
open Freql.Core.Common
open Freql.Sqlite

/// Module generated on 26/09/2024 21:46:24 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Records =
    /// A record representing a row in the table `tenant_user_claims`.
    type TenantUserClaim =
        { [<JsonPropertyName("tenantUserReference")>] TenantUserReference: string
          [<JsonPropertyName("claim")>] Claim: string }
    
        static member Blank() =
            { TenantUserReference = String.Empty
              Claim = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE "tenant_user_claims"
(
    tenant_user_reference TEXT not null
        constraint tenant_user_claims_tenant_users_reference_fk
            references tenant_users(reference),
    claim                 TEXT not null,
    constraint tenant_user_claims_pk
        primary key (tenant_user_reference, claim)
)
        """
    
        static member SelectSql() = """
        SELECT
              tenant_user_claims.`tenant_user_reference`,
              tenant_user_claims.`claim`
        FROM tenant_user_claims
        """
    
        static member TableName() = "tenant_user_claims"
    
    /// A record representing a row in the table `tenant_user_metadata`.
    type TenantUserMetadataItem =
        { [<JsonPropertyName("tenantUserReference")>] TenantUserReference: string
          [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: string }
    
        static member Blank() =
            { TenantUserReference = String.Empty
              ItemKey = String.Empty
              ItemValue = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE "tenant_user_metadata"
(
    tenant_user_reference TEXT not null
        constraint tenant_user_metadata_tenant_users_tenant_reference_fk
            references tenant_users (reference),
    item_key              TEXT not null,
    item_value            TEXT not null,
    constraint tenant_user_metadata_pk
        primary key (tenant_user_reference, item_key)
)
        """
    
        static member SelectSql() = """
        SELECT
              tenant_user_metadata.`tenant_user_reference`,
              tenant_user_metadata.`item_key`,
              tenant_user_metadata.`item_value`
        FROM tenant_user_metadata
        """
    
        static member TableName() = "tenant_user_metadata"
    
    /// A record representing a row in the table `tenant_users`.
    type TenantUser =
        { [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("tenantReference")>] TenantReference: string
          [<JsonPropertyName("userReference")>] UserReference: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("updatedOn")>] UpdatedOn: DateTime
          [<JsonPropertyName("active")>] Active: bool }
    
        static member Blank() =
            { Reference = Guid.NewGuid()
              TenantReference = String.Empty
              UserReference = String.Empty
              CreatedOn = DateTime.UtcNow
              UpdatedOn = DateTime.UtcNow
              Active = true }
    
        static member CreateTableSql() = """
        CREATE TABLE "tenant_users"
(
    reference        TEXT    not null
        constraint tenant_users_pk
            primary key,
    tenant_reference TEXT    not null
        constraint tenant_users_tenants_active_fk
            references tenants (reference),
    user_reference   TEXT    not null
        constraint tenant_users_users_reference_fk
            references users (reference),
    created_on       TEXT    not null,
    updated_on       TEXT    not null,
    active           integer not null,
    constraint tenant_users_uk
        unique (tenant_reference, user_reference)
)
        """
    
        static member SelectSql() = """
        SELECT
              tenant_users.`reference`,
              tenant_users.`tenant_reference`,
              tenant_users.`user_reference`,
              tenant_users.`created_on`,
              tenant_users.`updated_on`,
              tenant_users.`active`
        FROM tenant_users
        """
    
        static member TableName() = "tenant_users"
    
    /// A record representing a row in the table `tenants`.
    type Tenant =
        { [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("active")>] Active: bool }
    
        static member Blank() =
            { Reference = Guid.NewGuid()
              Name = String.Empty
              Active = true }
    
        static member CreateTableSql() = """
        CREATE TABLE "tenants"
(
    reference TEXT not null
        constraint tenants_pk
            primary key,
    name      TEXT not null
, active integer not null)
        """
    
        static member SelectSql() = """
        SELECT
              tenants.`reference`,
              tenants.`name`,
              tenants.`active`
        FROM tenants
        """
    
        static member TableName() = "tenants"
    
    /// A record representing a row in the table `users`.
    type User =
        { [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("updatedOn")>] UpdatedOn: DateTime
          [<JsonPropertyName("systemUser")>] SystemUser: int64
          [<JsonPropertyName("active")>] Active: bool }
    
        static member Blank() =
            { Reference = Guid.NewGuid()
              Name = String.Empty
              CreatedOn = DateTime.UtcNow
              UpdatedOn = DateTime.UtcNow
              SystemUser = 0L
              Active = true }
    
        static member CreateTableSql() = """
        CREATE TABLE users
(
    reference TEXT not null
        constraint users_pk
            primary key,
    name      TEXT not null
, created_on TEXT not null, updated_on TEXT not null, system_user integer not null, active integer not null)
        """
    
        static member SelectSql() = """
        SELECT
              users.`reference`,
              users.`name`,
              users.`created_on`,
              users.`updated_on`,
              users.`system_user`,
              users.`active`
        FROM users
        """
    
        static member TableName() = "users"
    

/// Module generated on 26/09/2024 21:46:24 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Parameters =
    /// A record representing a new row in the table `tenant_user_claims`.
    type NewTenantUserClaim =
        { [<JsonPropertyName("tenantUserReference")>] TenantUserReference: string
          [<JsonPropertyName("claim")>] Claim: string }
    
        static member Blank() =
            { TenantUserReference = String.Empty
              Claim = String.Empty }
    
    
    /// A record representing a new row in the table `tenant_user_metadata`.
    type NewTenantUserMetadataItem =
        { [<JsonPropertyName("tenantUserReference")>] TenantUserReference: string
          [<JsonPropertyName("itemKey")>] ItemKey: string
          [<JsonPropertyName("itemValue")>] ItemValue: string }
    
        static member Blank() =
            { TenantUserReference = String.Empty
              ItemKey = String.Empty
              ItemValue = String.Empty }
    
    
    /// A record representing a new row in the table `tenant_users`.
    type NewTenantUser =
        { [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("tenantReference")>] TenantReference: string
          [<JsonPropertyName("userReference")>] UserReference: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("updatedOn")>] UpdatedOn: DateTime
          [<JsonPropertyName("active")>] Active: bool }
    
        static member Blank() =
            { Reference = Guid.NewGuid()
              TenantReference = String.Empty
              UserReference = String.Empty
              CreatedOn = DateTime.UtcNow
              UpdatedOn = DateTime.UtcNow
              Active = true }
    
    
    /// A record representing a new row in the table `tenants`.
    type NewTenant =
        { [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("active")>] Active: bool }
    
        static member Blank() =
            { Reference = Guid.NewGuid()
              Name = String.Empty
              Active = true }
    
    
    /// A record representing a new row in the table `users`.
    type NewUser =
        { [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("updatedOn")>] UpdatedOn: DateTime
          [<JsonPropertyName("systemUser")>] SystemUser: int64
          [<JsonPropertyName("active")>] Active: bool }
    
        static member Blank() =
            { Reference = Guid.NewGuid()
              Name = String.Empty
              CreatedOn = DateTime.UtcNow
              UpdatedOn = DateTime.UtcNow
              SystemUser = 0L
              Active = true }
    
    
/// Module generated on 26/09/2024 21:46:24 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Operations =

    let buildSql (lines: string list) = lines |> String.concat Environment.NewLine

    /// Select a `Records.TenantUserClaim` from the table `tenant_user_claims`.
    /// Internally this calls `context.SelectSingleAnon<Records.TenantUserClaim>` and uses Records.TenantUserClaim.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTenantUserClaimRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectTenantUserClaimRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.TenantUserClaim.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.TenantUserClaim>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.TenantUserClaim>` and uses Records.TenantUserClaim.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTenantUserClaimRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectTenantUserClaimRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.TenantUserClaim.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.TenantUserClaim>(sql, parameters)
    
    let insertTenantUserClaim (context: SqliteContext) (parameters: Parameters.NewTenantUserClaim) =
        context.Insert("tenant_user_claims", parameters)
    
    /// Select a `Records.TenantUserMetadataItem` from the table `tenant_user_metadata`.
    /// Internally this calls `context.SelectSingleAnon<Records.TenantUserMetadataItem>` and uses Records.TenantUserMetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTenantUserMetadataItemRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectTenantUserMetadataItemRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.TenantUserMetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.TenantUserMetadataItem>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.TenantUserMetadataItem>` and uses Records.TenantUserMetadataItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTenantUserMetadataItemRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectTenantUserMetadataItemRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.TenantUserMetadataItem.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.TenantUserMetadataItem>(sql, parameters)
    
    let insertTenantUserMetadataItem (context: SqliteContext) (parameters: Parameters.NewTenantUserMetadataItem) =
        context.Insert("tenant_user_metadata", parameters)
    
    /// Select a `Records.TenantUser` from the table `tenant_users`.
    /// Internally this calls `context.SelectSingleAnon<Records.TenantUser>` and uses Records.TenantUser.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTenantUserRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectTenantUserRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.TenantUser.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.TenantUser>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.TenantUser>` and uses Records.TenantUser.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTenantUserRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectTenantUserRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.TenantUser.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.TenantUser>(sql, parameters)
    
    let insertTenantUser (context: SqliteContext) (parameters: Parameters.NewTenantUser) =
        context.Insert("tenant_users", parameters)
    
    /// Select a `Records.Tenant` from the table `tenants`.
    /// Internally this calls `context.SelectSingleAnon<Records.Tenant>` and uses Records.Tenant.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTenantRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectTenantRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Tenant.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Tenant>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Tenant>` and uses Records.Tenant.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectTenantRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectTenantRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Tenant.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Tenant>(sql, parameters)
    
    let insertTenant (context: SqliteContext) (parameters: Parameters.NewTenant) =
        context.Insert("tenants", parameters)
    
    /// Select a `Records.User` from the table `users`.
    /// Internally this calls `context.SelectSingleAnon<Records.User>` and uses Records.User.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectUserRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectUserRecord (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.User.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.User>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.User>` and uses Records.User.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectUserRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectUserRecords (context: SqliteContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.User.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.User>(sql, parameters)
    
    let insertUser (context: SqliteContext) (parameters: Parameters.NewUser) =
        context.Insert("users", parameters)
    