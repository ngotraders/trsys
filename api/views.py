from django.http.request import HttpHeaders
from django.http.response import HttpResponseRedirect
from django.http import Http404, HttpResponse
from django.urls import reverse
from django.views.decorators.csrf import csrf_exempt

from .models import Account


@csrf_exempt
def account_index(request):
    if (request.method == 'GET'):
        raise Http404()
    account = Account.objects.filter(
        account_number=request.POST['accountnumber']
    ).first()
    if (account):
        account.account_company = request.POST['accountcompany'],
        account.account_name = request.POST['accountname'],
        account.email = request.POST['email'],
        account.is_demo = (request.POST['isdemo'] == '1')
        account.save()
        return HttpResponse(status=204)
    else:
        account = Account(
            account_company=request.POST['accountcompany'],
            account_name=request.POST['accountname'],
            account_number=request.POST['accountnumber'],
            email=request.POST['email'],
            is_demo=(request.POST['isdemo'] == '1')
        )
        account.save()
        createdLocation = reverse(
            "api:account_detail", kwargs=dict(pk=account.id))
        response = HttpResponse(status=201)
        response['Location'] = createdLocation
        return response


@csrf_exempt
def account_detail(request):
    print("account_detail")
    return HttpResponse("detail")


@csrf_exempt
def trade_index(request):
    print("post_trade")
    return HttpResponse("index")
