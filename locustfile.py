from locust import HttpUser, task, constant_pacing
import urllib3
from urllib3.exceptions import InsecureRequestWarning

urllib3.disable_warnings(InsecureRequestWarning)
import uuid


class Admin:
    def __init__(self, client) -> None:
        self.client = client
        self.client.verify = False

    def login(self, username: str, password: str):
        self.client.get("/login")
        return self.client.post("/login",
                                data={
                                    'Username': username,
                                    'Password': password
                                })

    def createKey(self, key: str, key_type: str):
        return self.client.post("/admin/keys/new",
                                data={
                                    'Key': key,
                                    'KeyType': key_type
                                })

    def activateKey(self, key: str):
        return self.client.post(f"/admin/keys/{key}/approve",
                                name="/admin/keys/[key]/approve")

    def deactivateKey(self, key: str):
        return self.client.post(f"/admin/keys/{key}/revoke",
                                name="/admin/keys/[key]/revoke")

    def deleteKey(self, key: str):
        return self.client.post(f"/admin/keys/{key}/delete",
                                name="/admin/keys/[key]/delete")


class Subscriber(HttpUser):
    wait_time = constant_pacing(0.1)

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.secretKey = ''
        self.token = ''
        self.etag = ''

    def fetch_secret_key(self):
        self.client.verify = False
        self.client.headers['Content-Type'] = 'text/plain'
        res = self.client.post("/api/token", data=self.secretKey)
        if res.status_code == 200:
            self.token = res.text
            self.client.headers = {}
            self.client.headers['Content-Type'] = 'text/plain'
            self.client.headers['Version'] = '20210331'
            self.client.headers['X-Secret-Token'] = self.token

    def on_start(self):
        self.admin = Admin(self.client)
        self.admin.login("admin", "P@ssw0rd")
        self.secretKey = str(uuid.uuid4())
        self.admin.createKey(self.secretKey, "3")
        self.admin.activateKey(self.secretKey)

    def on_stop(self):
        self.admin.deactivateKey(self.secretKey)
        self.admin.deleteKey(self.secretKey)

    @task
    def subscribe(self):
        if self.token == '':
            self.fetch_secret_key()
        if self.token == '':
            return

        if self.etag != '':
            self.client.headers['If-None-Match'] = self.etag
        res = self.client.get("/api/orders")
        if res.status_code == 200:
            self.etag = res.headers['ETag']
        elif res.status_code == 401:
            self.token = ''
        elif res.status_code != 304:
            raise ValueError(f"HTTP Error! Status code: {res.status_code}")
