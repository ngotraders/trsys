from django.urls import path

from . import views

app_name = 'api'
urlpatterns = [
    path('accounts', views.account_index, name="account_index"),
    path('accounts/<int:pk>/trades', views.trade_index, name="trade_index"),
]
