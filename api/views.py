import hashlib

from django.http import HttpResponse
from django.http.response import HttpResponseBadRequest
from django.core.cache import cache
from django.views.decorators.csrf import csrf_exempt

from .models import Order, OrderType

CACHE_KEY_HASH = 'ORDERS_RESPONSE_HSAH'


def _createHash(value: str):
    """
    calculate hash of string
    """
    hash = hashlib.sha1()
    hash.update(value.encode())
    return hash.hexdigest()[:-10]


@csrf_exempt
def order_index(request):
    if (request.method == 'GET'):
        hash = cache.get(CACHE_KEY_HASH)
        etag = request.headers.get('If-None-Match')
        if etag == f'"{hash}"':
            return HttpResponse(status=304)

        orders = Order.objects.all()
        responseText = ''
        for order in orders:
            if responseText:
                responseText += '@'
            responseText += f'{order.ticket_no}:{order.symbol}:{order.order_type}'
        hash = _createHash(responseText)
        cache.set(CACHE_KEY_HASH, hash)
        response = HttpResponse(responseText)
        response["ETag"] = f'{hash}'
        return response
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
        cache.delete(CACHE_KEY_HASH)
        return HttpResponse(status=200)
