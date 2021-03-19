from django.db import models


class Account(models.Model):
    account_company = models.TextField()
    account_name = models.TextField()
    account_number = models.TextField()
    email = models.TextField()
    is_demo = models.BooleanField()


class Position(models.Model):
    ticket_no = models.TextField()
    symbol = models.TextField()
    order_type = models.TextChoices('OP_BUY', 'OP_SELL')
