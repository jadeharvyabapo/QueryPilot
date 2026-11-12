import os

import httpx
from fastapi import FastAPI, HTTPException
from fastapi.responses import PlainTextResponse
from pydantic import BaseModel, Field

from Services.Database.schema_inspector import SchemaInspector
from Services.Sql.ValidateSql import ValidateSql
from Prompt.SqlPrompt import prompt as Prompt


##put in config settings
OLLAMA_URL = os.getenv("OLLAMA_URL", "http://localhost:11434")
OLLAMA_MODEL = os.getenv("OLLAMA_MODEL", "qwen2.5-coder:7b")
app = FastAPI(title="QueryPilot")



class SQLRequest(BaseModel):
    question: str


@app.post("/generate-sql", response_class=PlainTextResponse)
async def generate_sql(request: SQLRequest):
    
    question = request.question
    # Get the database schema
    schema = SchemaInspector.get_database_schema()

    # Build the complete prompt
    original_prompt = Prompt.format(
        schema=schema,
        question=question
    )

    prompt = original_prompt

    max_attempts = 10

    for attempt in range(max_attempts):
        # Send the prompt to Ollama
        try:
            async with httpx.AsyncClient(timeout=120) as client:
                response = await client.post(
                    f"{OLLAMA_URL.rstrip('/')}/api/generate",
                    json={
                        "model": "llama3.2:3b",
                        "prompt": prompt,
                        "stream": False,
                        "options": {
                            "temperature": 0
                        }
                    }
                )

                response.raise_for_status()

        except httpx.TimeoutException:
            raise HTTPException(
                status_code=504,
                detail="Ollama request timed out."
            )

        except httpx.RequestError:
            raise HTTPException(
                status_code=503,
                detail="Unable to connect to Ollama."
            )

        # Extract generated SQL
        result = response.json()

        generated_sql = ValidateSql.extract_sql(result["response"])

        # Validate SQL
        is_valid, message = ValidateSql.validate(generated_sql)

        print(f"Attempt {attempt + 1}: {is_valid}")
        print(message)

        if  is_valid:
            break

        prompt = f"""
           {original_prompt}

           PREVIOUS GENERATED SQL:
           {generated_sql}

           POSTGRESQL ERROR:
           {message}

           CORRECTION INSTRUCTIONS:
           - Read the PostgreSQL error carefully.
           - If PostgreSQL provides a HINT, follow that hint.

           """
    else:
        raise HTTPException(
            status_code=422,
            detail=f"Unable to generate valid SQL after {max_attempts} attempts. Last error: {message}"
        )


    response.raise_for_status()

    result = response.json()
    generated_sql = ValidateSql.extract_sql(result["response"])

    # Validate corrected SQL
    is_valid, message = ValidateSql.validate(generated_sql)

    print(is_valid, message)



    return PlainTextResponse(
        content=generated_sql.strip()
    )
