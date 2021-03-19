from django.shortcuts import render
from django.http import HttpResponse


def account_index(request):
    print("post_account")
    return HttpResponse("index")

def trade_index(request):
    print("post_trade")
    return HttpResponse("index")
