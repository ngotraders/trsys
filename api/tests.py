from django.test import TestCase
from django.urls import reverse
from django.core.cache import cache

from .models import Order


class OrderIndexTests(TestCase):
    def setUp(self):
        cache.delete('orders')

    def test_get_order_should_return_empty_response_given_no_orders(self):
        """
        /api/ordersにgetするとオーダーのリストが取得できる。オーダーが存在しない場合
        """
        response = self.client.get(reverse('api:order_index'))
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.content, b'')

    def test_get_order_should_return_empty_response_given_single_order(self):
        """
        /api/ordersにgetするとオーダーのリストが取得できる。オーダーが1件の場合
        """
        Order(ticket_no="1", symbol="USDJPY", order_type="0").save()
        response = self.client.get(reverse('api:order_index'))
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.content, b'1:USDJPY:0')

    def test_get_order_should_return_empty_response_given_multiple_orders(self):
        """
        /api/ordersにgetするとオーダーのリストが取得できる。オーダーが2件以上の場合
        """
        Order(ticket_no="1", symbol="USDJPY", order_type="0").save()
        Order(ticket_no="2", symbol="EURUSD", order_type="1").save()
        response = self.client.get(reverse('api:order_index'))
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.content, b'1:USDJPY:0@2:EURUSD:1')

    def test_post_order_should_create_new_order(self):
        """
        /api/ordersにpostするとオーダーを登録できる。
        """
        response = self.client.post(
            reverse('api:order_index'),
            '123456:USDJPY:0',
            content_type='text/plain',
        )
        self.assertEqual(response.status_code, 200)
        result = Order.objects.filter(ticket_no='123456').first()
        self.assertIsNotNone(result)

    def test_post_order_should_create_new_multiple_orders(self):
        """
        /api/ordersにpostするとオーダーを登録できる。オーダーが複数存在する場合
        """
        response = self.client.post(
            reverse('api:order_index'),
            '123456:USDJPY:0@123457:USDJPY:0',
            content_type='text/plain',
        )
        self.assertEqual(response.status_code, 200)
        self.assertIsNotNone(Order.objects.filter(ticket_no='123456').first())
        self.assertIsNotNone(Order.objects.filter(ticket_no='123457').first())

    def test_post_order_should_update_multiple_orders(self):
        """
        /api/ordersにpostするとオーダーを更新できる。
        """
        Order(ticket_no="1", symbol="USDJPY", order_type="0").save()
        Order(ticket_no="2", symbol="USDJPY", order_type="0").save()
        response = self.client.post(
            reverse('api:order_index'),
            '3:USDJPY:1@4:USDJPY:1',
            content_type='text/plain',
        )
        self.assertEqual(response.status_code, 200)
        self.assertIsNone(Order.objects.filter(ticket_no='1').first())
        self.assertIsNone(Order.objects.filter(ticket_no='2').first())
        self.assertIsNotNone(Order.objects.filter(ticket_no='3').first())
        self.assertIsNotNone(Order.objects.filter(ticket_no='4').first())
