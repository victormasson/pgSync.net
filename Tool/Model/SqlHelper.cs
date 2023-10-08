
namespace PgSync.Model;

public static class SqlHelper
{
    public static string GetTable()
        => @"
        SELECT
            table_schema AS schema,
            table_name AS table
        FROM
            information_schema.tables
        WHERE
            table_type = 'BASE TABLE' AND
            table_schema NOT IN ('information_schema', 'pg_catalog')
        ORDER BY 1, 2
        ";

    public static string GetSchema()
        => @"
        SELECT
            schema_name
        FROM
            information_schema.schemata
        ORDER BY 1
        ";

    public static string GetTriggers(string table)
        => @$"
        SELECT
            tgname AS name,
            tgisinternal AS internal,
            tgenabled != 'D' AS enabled,
            tgconstraint != 0 AS integrity
        FROM
            pg_trigger
        WHERE
            pg_trigger.tgrelid = {table}
        ";

    public static string GetSequences()
        => @"
        SELECT
            nt.nspname as schema,
            t.relname as table,
            a.attname as column,
            n.nspname as sequence_schema,
            s.relname as sequence
        FROM
            pg_class s
        INNER JOIN
            pg_depend d ON d.objid = s.oid
        INNER JOIN
            pg_class t ON d.objid = s.oid AND d.refobjid = t.oid
        INNER JOIN
            pg_attribute a ON (d.refobjid, d.refobjsubid) = (a.attrelid, a.attnum)
        INNER JOIN
            pg_namespace n ON n.oid = s.relnamespace
        INNER JOIN
            pg_namespace nt ON nt.oid = t.relnamespace
        WHERE
            s.relkind = 'S'
        ";

    public static string GetPk()
        => @"
        SELECT
            nspname AS schema,
            relname AS table,
            pg_attribute.attname AS column,
            format_type(pg_attribute.atttypid, pg_attribute.atttypmod),
            pg_attribute.attnum,
            pg_index.indkey
        FROM
            pg_index, pg_class, pg_attribute, pg_namespace
        WHERE
            indrelid = pg_class.oid AND
            pg_class.relnamespace = pg_namespace.oid AND
            pg_attribute.attrelid = pg_class.oid AND
            pg_attribute.attnum = any(pg_index.indkey) AND
            indisprimary
        ";
    public static string GetColumns()
        => @"
        SELECT
            table_schema AS schema,
            table_name AS table,
            column_name AS column,
            data_type AS type
        FROM
            information_schema.columns
        WHERE
            is_generated = 'NEVER'
        ORDER BY 1, 2, 3
        ";

    public static string GetNonDeferrableConstraints()
        => @"
        SELECT
            table_schema AS schema,
            table_name AS table,
            constraint_name
        FROM
            information_schema.table_constraints
        WHERE
            constraint_type = 'FOREIGN KEY' AND
            is_deferrable = 'NO'
        ";

    #region Action

    public static string CreateSchema(string schema)
        => @$"CREATE SCHEMA {schema}";

    public static string TruncateTable(string table)
        => @$"TRUNCATE {table} CASCADE";

    public static string DeleteTable(string table, string condition)
        => @$"DELETE FROM {table} WHERE {condition}";

    #endregion Action
}