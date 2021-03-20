from django.shortcuts import get_object_or_404
from django.http.request import HttpHeaders
from django.http.response import HttpResponseRedirect
from django.http import Http404, HttpResponse
from django.urls import reverse
from django.views.decorators.csrf import csrf_exempt

from .models import Order, OrderType


@csrf_exempt
def order_index(request):
    if (request.method == 'GET'):
        orders = Order.objects.all()
        response = ''
        for order in orders:
            if response:
                response += '@'
            response += f'{order.ticket_no}:{order.symbol}:{order.order_type}'
        return HttpResponse(response)

    Order.objects.all().delete()
    for item in request.body.decode().split('@'):
        ticket_no, symbol, order_type = item.split(':')
        Order(
            ticket_no=ticket_no,
            symbol=symbol,
            order_type=OrderType[order_type],
        ).save()
    return HttpResponse(status=200)
