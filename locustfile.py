from locust import HttpUser, task, constant_pacing

class Publisher(HttpUser):
    secretKey = "d94b64d6-62d5-4063-bdb7-adc30f9abbc7"
    wait_time = constant_pacing(60)
    
    def on_start(self):
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

    @task
    def publish(self):
        res = self.client.post("/api/orders", data="".encode())
        if res.status_code != 200:
            raise ValueError(f"HTTP Error! Status code: {res.status_code}")
        

class Subscriber(HttpUser):
    secretKey = "b5b8e437-7963-4e82-862b-d185feb39fd4"
    wait_time = constant_pacing(0.1)
    
    def on_start(self):
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

    @task
    def publish(self):
        res = self.client.get("/api/orders")
        if res.status_code != 200:
            raise ValueError(f"HTTP Error! Status code: {res.status_code}")
