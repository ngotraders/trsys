from django.http import HttpResponse
from django.http.response import HttpResponseBadRequest
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
        print(response)
        return HttpResponse(response)
    elif request.method == 'POST':
        requestString = request.body.decode().rstrip('\x00')
        Order.objects.all().delete()
        for item in requestString.split('@'):
            splitted = item.split(':')
            if len(splitted) != 3:
                return HttpResponseBadRequest()
            ticket_no, symbol, order_type = splitted
            if (not ticket_no or not symbol or not order_type):
                return HttpResponseBadRequest()
            order = Order(
                ticket_no=ticket_no,
                symbol=symbol,
                order_type=OrderType(order_type),
            )
            order.save()
        return HttpResponse(status=200)
