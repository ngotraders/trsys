from django.test import TestCase
from django.urls import reverse

from .models import Order


class OrderIndexTests(TestCase):
    def test_get_order_should_return_empty_response_given_no_orders(self):
        """
        /api/ordersにgetするとオーダーのリストが取得できる。オーダーが存在しない場合
        """
        response = self.client.get(reverse('api:order_index'))
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.content, b'')

    def test_post_order_should_create_new_order(self):
        """
        /api/ordersにpostするとオーダーのリストを登録できる。オーダーが存在しない場合
        """
        response = self.client.post(
            reverse('api:order_index'),
            '123456:USDJPY:BUY',
            content_type='text/plain',
        )
        self.assertEqual(response.status_code, 200)
        result = Order.objects.filter(ticket_no='123456').first()
        self.assertIsNotNone(result)
