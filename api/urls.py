from django.urls import path
from django.views.decorators.csrf import csrf_exempt

from . import views

app_name = 'api'
urlpatterns = [
    path('accounts', csrf_exempt(views.account_index)),
    path('trades', csrf_exempt(views.trade_index)),
]
