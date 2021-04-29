from locust import HttpUser, task, constant_pacing
import urllib3
from urllib3.exceptions import InsecureRequestWarning
urllib3.disable_warnings(InsecureRequestWarning)
import uuid
import requests

class Admin:
    def __init__(self, host: str) -> None:
        self.host = host
        self.client = requests.Session()
        self.client.verify = False
    
    def login(self, username: str, password: str):
        self.client.get(f"{self.host}/login")
        return self.client.post(f"{self.host}/login", data={'Username': username, 'Password': password})

    def createKey(self, key: str, key_type: str):
        return self.client.post(f"{self.host}/admin/keys/new", data={'Key': key, 'KeyType': key_type})

    def activateKey(self, key: str):
        return self.client.post(f"{self.host}/admin/keys/{key}/approve")

    def deactivateKey(self, key: str):
        return self.client.post(f"{self.host}/admin/keys/{key}/revoke")

    def deleteKey(self, key: str):
        return self.client.post(f"{self.host}/admin/keys/{key}/delete")

admin = Admin("https://localhost:5001")
res = admin.login("admin", "P@ssw0rd")
if res.status_code != 200:
    raise ValueError(f"login HTTP Error! Status code: {res.status_code}")

class Subscriber(HttpUser):
    wait_time = constant_pacing(0.1)

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.secretKey = str(uuid.uuid4())
    
    def on_start(self):
        admin.createKey(self.secretKey, "3")
        admin.activateKey(self.secretKey)
        self.client.verify = False
        self.client.headers['Content-Type'] = 'text/plain'
        res = self.client.post("/api/token", data=self.secretKey)
        if res.status_code != 200:
            raise ValueError(f"HTTP Error! Status code: {res.status_code}")
        self.token = res.text

        self.client.headers = {}
        self.client.headers['Content-Type'] = 'text/plain'
        self.client.headers['Version'] = '20210331'
        self.client.headers['X-Secret-Token'] = self.token
    
    def on_stop(self):
        admin.deactivateKey(self.secretKey)
        admin.deleteKey(self.secretKey)

    @task
    def subscribe(self):
        res = self.client.get("/api/orders")
        if res.status_code != 200:
            raise ValueError(f"HTTP Error! Status code: {res.status_code}")
