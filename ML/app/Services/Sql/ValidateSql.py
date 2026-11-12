from Services.Database.connection import engine
import re

class ValidateSql:
    @staticmethod
    def extract_sql(response: str) -> str:

        match = re.search(
            r"```(?:sql)?\s*(.*?)```",
            response,
            re.DOTALL | re.IGNORECASE
        )

        if match:
            return match.group(1).strip()

        match = re.search(
            r"\b(SELECT|WITH)\b.*",
            response,
            re.DOTALL | re.IGNORECASE
        )

        if match:
            return match.group(0).strip()

        return ""


    @staticmethod
    def validate(response: str) -> tuple[bool, str]:
        if not response or not response.strip():
            return False, "No SQL response was received."

        try:
            with engine.connect() as connection:
                with connection.begin():
                    connection.exec_driver_sql(
                        "SET TRANSACTION READ ONLY"
                    )

                    connection.exec_driver_sql(
                        "SET LOCAL statement_timeout = '5000ms'"
                    )

                    connection.exec_driver_sql(
                        "EXPLAIN " + response
                    )

                return True, "SQL is valid."

        except Exception as e:
            return False, str(e)
