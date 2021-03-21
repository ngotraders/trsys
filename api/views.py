from django.http import HttpResponse
from django.views.decorators.csrf import csrf_exempt
from django.core.cache import cache

from .models import Order, OrderType


@csrf_exempt
def order_index(request):
    if (request.method == 'GET'):
        orders = cache.get('orders')
        if not orders:
            orders = Order.objects.all()
            cache.set('orders', orders)

        response = ''
        for order in orders:
            if response:
                response += '@'
            response += f'{order.ticket_no}:{order.symbol}:{order.order_type}'
        return HttpResponse(response)
    elif request.method == 'POST':
        Order.objects.all().delete()
        for item in request.body.decode().split('@'):
            ticket_no, symbol, order_type = item.split(':')
            order = Order(
                ticket_no=ticket_no,
                symbol=symbol,
                order_type=OrderType[order_type],
            )
            order.save()
        cache.delete('orders')
        return HttpResponse(status=200)
