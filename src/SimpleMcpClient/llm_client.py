import logging
import os
import time
import httpx
from dotenv import load_dotenv
from openai import AzureOpenAI


class LLMClient:
    """Manages communication with the LLM provider."""

    def get_response(self, messages: list[dict[str, str]]) -> str:

        load_dotenv()

        client = AzureOpenAI(
            api_key=os.getenv("AZURE_OPENAI_API_KEY"),
            api_version=os.getenv("AZURE_OPENAI_API_VERSION"),
            azure_endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        )

        try:
            logging.info(f"LLM first request...")
            start_time = time.time()
            completion = client.chat.completions.create(
                model=os.getenv("AZURE_OPENAI_API_DEPLOYMENTNAME"),
                messages=messages,
            )
            elapsed_time = time.time() - start_time
            logging.info(f"LLM first request spent {elapsed_time} second")
            return completion.choices[0].message.content
        except httpx.RequestError as e:
            error_message = f"Error getting LLM response: {str(e)}"
            logging.error(error_message)

            if isinstance(e, httpx.HTTPStatusError):
                status_code = e.response.status_code
                logging.error(f"Status code: {status_code}")
                logging.error(f"Response details: {e.response.text}")

            return (
                f"I encountered an error: {error_message}. "
                "Please try again or rephrase your request."
            )
