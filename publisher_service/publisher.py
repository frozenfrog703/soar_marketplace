from __future__ import annotations
from typing import Generator

from google.cloud import pubsub
from google.oauth2 import service_account
import os
import time
import random
import json

ROOT = os.path.dirname(os.path.dirname(__file__))
TOPIC = "projects/tip-demo-448009/topics/my-topic"
SA_PATH = os.path.join(ROOT, "service_account.json")
SA_ENV_VAR = "TIP_SERVICE_ACCOUNT"

BYTE_OCTATE = 256
TIMER_INTERVAL_SECONDS = 10


class TipDemoError(Exception):
    """ Base Exception """


def load_credentials() -> service_account.Credentials:
    try:
        if os.path.exists(SA_PATH):
            with open(SA_PATH) as f:
                sa_json = json.load(f)
        else:
            sa_json = json.loads(os.environ[SA_ENV_VAR])
        return service_account.Credentials.from_service_account_info(sa_json)
    except Exception as e:
        raise TipDemoError(f"Service Account Json File could not be found or invalid! Inner error: {e}")


def publish_msg(msg: str, credentials, topic_name) -> str:
    with pubsub.PublisherClient(credentials=credentials) as publisher:
        future = publisher.publish(topic_name, msg.encode())
        return future.result()


def generate_iocs(limit: int) -> Generator[str] :
    for _ in range(limit):
        yield "\n".join(
            ".".join(
                str(random.randint(0, 255)) for _ in range(4)
            ) for _ in range(random.randrange(1, 10))
        )
        time.sleep(TIMER_INTERVAL_SECONDS)


def main():
    try:
        creds = load_credentials()
        for ioc in generate_iocs(random.randint(1, 5)):
            msg_id = publish_msg(ioc, creds, TOPIC)
            print(f"Message with id {msg_id} sent to pub/sub topic {TOPIC}")
    except TipDemoError as e:
        print(f"Publisher Service Exited with error: {e}")


if __name__ == "__main__":
    main()