import unittest
from types import SimpleNamespace
from unittest.mock import MagicMock, patch

from fastapi import HTTPException

from main import generate_response
from Services.LLM.sql_output import SQLValidationError


class GenerateResponseTests(unittest.TestCase):
    def setUp(self) -> None:
        self.engine = MagicMock()
        connection = self.engine.connect.return_value.__enter__.return_value
        connection.begin.return_value.__enter__.return_value = MagicMock()
        self.sql_chain = MagicMock()
        self.request = SimpleNamespace(
            app=SimpleNamespace(
                state=SimpleNamespace(
                    sql_chain=self.sql_chain,
                    database_engine=self.engine,
                )
            )
        )

    @patch("main.SchemaInspector.get_database_relations", return_value=("customers",))
    @patch("main.SchemaInspector.get_database_schema", return_value="Table: customers")
    def test_returns_a_validated_query(
        self, _schema: MagicMock, _relations: MagicMock
    ) -> None:
        self.sql_chain.invoke.return_value = (
            "SELECT count(*) FROM northwind.customers"
        )

        response = generate_response(self.request, question="How many customers?")

        self.assertEqual(
            response.body.decode(),
            "SELECT COUNT(*) FROM northwind.customers",
        )
        self.sql_chain.invoke.assert_called_once()
        self.engine.connect.assert_called_once()

    @patch("main.SchemaInspector.get_database_relations", return_value=("customers",))
    @patch("main.SchemaInspector.get_database_schema", return_value="Table: customers")
    def test_reports_invalid_sql_without_retrying(
        self, _schema: MagicMock, _relations: MagicMock
    ) -> None:
        self.sql_chain.invoke.return_value = "SELECT id FROM northwind.unknown_table"

        with self.assertRaises(SQLValidationError):
            generate_response(self.request, question="How many customers?")

        self.assertEqual(self.sql_chain.invoke.call_count, 1)

    @patch("main.SchemaInspector.get_database_relations", return_value=("customers",))
    @patch("main.SchemaInspector.get_database_schema", return_value="Table: customers")
    def test_returns_unsupported_response_as_http_error(
        self, _schema: MagicMock, _relations: MagicMock
    ) -> None:
        self.sql_chain.invoke.return_value = "UNSUPPORTED: metric unavailable"

        with self.assertRaises(HTTPException) as error:
            generate_response(self.request, question="Unknown metric?")

        self.assertEqual(error.exception.status_code, 422)


if __name__ == "__main__":
    unittest.main()
