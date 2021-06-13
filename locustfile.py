from locust import HttpUser, task, constant_pacing
import urllib3
from urllib3.exceptions import InsecureRequestWarning

urllib3.disable_warnings(InsecureRequestWarning)
import uuid
import time


class Admin:
    def __init__(self, client) -> None:
        self.client = client
        self.client.verify = False

    def login(self, username: str, password: str):
        self.client.cookies.clear()
        self.client.headers = {}
        self.client.get("/login")
        return self.client.post("/login",
                                data={
                                    'Username': username,
                                    'Password': password
                                })

    def createKey(self, key: str):
        return self.client.post("/api/keys", json={'Key': key, 'KeyType': 3})

    def activateKey(self, key: str):
        return self.client.put(f"/api/keys/{key}",
                               name='/api/keys/[key]',
                               json={
                                   'KeyType': 3,
                                   'IsApproved': True,
                               })

    def deactivateKey(self, key: str):
        return self.client.put(f"/api/keys/{key}",
                               name='/api/keys/[key]',
                               json={
                                   'KeyType': 3,
                                   'IsApproved': False,
                               })

    def deleteKey(self, key: str):
        return self.client.delete(f"/api/keys/{key}", name="/api/keys/[key]")


class Subscriber(HttpUser):
    wait_time = constant_pacing(0.1)

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.client.verify = False
        self.token = ''
        self.etag = ''

    def fetch_secret_key(self):
        self.client.headers = {}
        self.client.headers['Content-Type'] = 'text/plain'
        self.client.headers['X-Ea-Id'] = self.secretKey
        self.client.headers['X-Ea-Type'] = 'Subscriber'
        self.client.headers['X-Ea-Version'] = '20210609:Locust'
        res = self.client.post("/api/token", data=self.secretKey)
        if res.status_code == 200:
            self.token = res.text

    def on_start(self):
        self.secretKey = str(uuid.uuid4())
        admin = Admin(self.client)
        admin.login("admin", "P@ssw0rd")
        admin.createKey(self.secretKey)
        admin.activateKey(self.secretKey)

    def on_stop(self):
        admin = Admin(self.client)
        admin.login("admin", "P@ssw0rd")
        admin.deactivateKey(self.secretKey)
        admin.deleteKey(self.secretKey)

    @task
    def subscribe(self):
        if self.token == '':
            self.fetch_secret_key()
        if self.token == '':
            return
        else:
            self.client.cookies.clear()
            self.client.headers = {}
            self.client.headers['Content-Type'] = 'text/plain'
            self.client.headers['X-Ea-Id'] = self.secretKey
            self.client.headers['X-Ea-Type'] = 'Subscriber'
            self.client.headers['X-Ea-Version'] = '20210609:Locust'
            self.client.headers['X-Secret-Token'] = self.token

        if self.etag != '':
            self.client.headers['If-None-Match'] = self.etag
        res = self.client.get("/api/orders")
        if res.status_code == 200:
            self.etag = res.headers['ETag']
            self.client.post(
                '/api/logs',
                data=
                f'{int(time.time())}:DEBUG:{self.secretKey}/{self.token}/{res.content}'
            )
        elif res.status_code == 401:
            self.token = ''
        elif res.status_code != 304:
            raise ValueError(f"HTTP Error! Status code: {res.status_code}")
