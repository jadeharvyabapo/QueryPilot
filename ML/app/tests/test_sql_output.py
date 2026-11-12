import unittest
from unittest.mock import MagicMock

from Services.LLM.sql_output import SQLValidationError, validate_generated_sql


class GeneratedSQLTests(unittest.TestCase):
    def setUp(self) -> None:
        self.engine = MagicMock()
        connection = self.engine.connect.return_value.__enter__.return_value
        connection.begin.return_value.__enter__.return_value = MagicMock()
        self.allowed_relations = {"orders", "customers"}

    def validate(self, output: str) -> str:
        return validate_generated_sql(
            output,
            allowed_relations=self.allowed_relations,
            engine=self.engine,
        )

    def test_extracts_markdown_and_assistant_wrapper(self) -> None:
        output = """`sql
SELECT shipped_date FROM northwind.orders;
`

Assistant: ```sql
SELECT shipped_date FROM northwind.orders;
```"""

        sql = self.validate(output)

        self.assertIn("northwind.orders", sql)

    def test_rejects_different_queries_in_multiple_fences(self) -> None:
        with self.assertRaises(SQLValidationError):
            self.validate(
                "```sql SELECT 1 ```\nAssistant: ```sql SELECT 2 ```"
            )
        self.engine.connect.assert_not_called()

    def test_rejects_non_select_and_multiple_statements(self) -> None:
        for output in (
            "DELETE FROM northwind.orders",
            "SELECT 1; SELECT 2",
        ):
            with self.subTest(output=output), self.assertRaises(SQLValidationError):
                self.validate(output)
        self.engine.connect.assert_not_called()

    def test_rejects_unknown_or_unqualified_tables(self) -> None:
        for output in (
            "SELECT id FROM orders",
            "SELECT id FROM northwind.unknown_table",
            "SELECT id FROM other_schema.orders",
        ):
            with self.subTest(output=output), self.assertRaises(SQLValidationError):
                self.validate(output)
        self.engine.connect.assert_not_called()

    def test_accepts_cte_and_plans_query_read_only(self) -> None:
        sql = self.validate(
            "WITH annual AS ("
            "SELECT shipped_date FROM northwind.orders"
            ") SELECT shipped_date FROM annual"
        )

        self.assertIn("WITH annual", sql)
        connection = self.engine.connect.return_value.__enter__.return_value
        connection.exec_driver_sql.assert_any_call("SET TRANSACTION READ ONLY")
        connection.exec_driver_sql.assert_any_call(
            "SET LOCAL statement_timeout = '3s'"
        )
        connection.execute.assert_called_once()


if __name__ == "__main__":
    unittest.main()
