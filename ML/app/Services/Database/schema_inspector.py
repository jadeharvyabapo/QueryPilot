from functools import lru_cache

from sqlalchemy import inspect

from .connection import engine


DATABASE_SCHEMA = "public"

class SchemaInspector:
    @staticmethod    
    def get_database_relations() -> tuple[str, ...]:
        inspector = inspect(engine)
        tables = inspector.get_table_names(schema=DATABASE_SCHEMA)
        views = inspector.get_view_names(schema=DATABASE_SCHEMA)
        return tuple(
            sorted(
                relation
                for relation in [*tables, *views]
                if relation != "_import_manifest"
            )
        )


    @lru_cache(maxsize=1)
    @staticmethod
    def get_database_schema() -> str:
        inspector = inspect(engine)
        lines = [f"Database schema: {DATABASE_SCHEMA}"]

        tables = [
            table
            for table in inspector.get_table_names(schema=DATABASE_SCHEMA)
            if table != "_import_manifest"
        ]
        for table in sorted(tables):
            lines.append(f"\nTable: {table}")

            columns = inspector.get_columns(table, schema=DATABASE_SCHEMA)
            primary_key_columns = set(
                inspector.get_pk_constraint(table, schema=DATABASE_SCHEMA).get(
                    "constrained_columns"
                )
                or []
            )
            lines.append("  Columns:")
            for column in columns:
                description = f"    - {column['name']}: {column['type']}"
                if column["name"] in primary_key_columns:
                    description += " (primary key)"
                lines.append(description)

            foreign_keys = inspector.get_foreign_keys(table, schema=DATABASE_SCHEMA)
            if foreign_keys:
                lines.append("  Foreign keys:")
                for foreign_key in foreign_keys:
                    referenced_table = foreign_key["referred_table"]
                    referred_columns = foreign_key.get("referred_columns") or []
                    for column, referred_column in zip(
                        foreign_key["constrained_columns"] or [],
                        referred_columns,
                    ):
                        lines.append(
                            f"    - {column} -> "
                            f"{DATABASE_SCHEMA}.{referenced_table}.{referred_column}"
                        )

        views = sorted(inspector.get_view_names(schema=DATABASE_SCHEMA))
        if views:
            lines.append("\nViews:")
            for view in views:
                columns = inspector.get_columns(view, schema=DATABASE_SCHEMA)
                lines.append(f"\nView: {view}")
                lines.append("  Columns:")
                for column in columns:
                    lines.append(f"    - {column['name']}: {column['type']}")

        return "\n".join(lines)
    
   