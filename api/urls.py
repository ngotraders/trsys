from django.urls import path

from . import views

app_name = 'api'
urlpatterns = [
    path('orders', views.order_index, name="order_index"),
]
