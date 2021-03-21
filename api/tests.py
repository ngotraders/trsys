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
        /api/ordersにpostするとオーダーを登録できる。
        """
        response = self.client.post(
            reverse('api:order_index'),
            '123456:USDJPY:BUY',
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
            '123456:USDJPY:BUY@123457:USDJPY:BUY',
            content_type='text/plain',
        )
        self.assertEqual(response.status_code, 200)
        self.assertIsNotNone(Order.objects.filter(ticket_no='123456').first())
        self.assertIsNotNone(Order.objects.filter(ticket_no='123457').first())

    def test_post_order_should_update_multiple_orders(self):
        """
        /api/ordersにpostするとオーダーを更新できる。
        """
        Order(ticket_no="1", symbol="USDJPY", order_type="BUY").save()
        Order(ticket_no="2", symbol="USDJPY", order_type="BUY").save()
        response = self.client.post(
            reverse('api:order_index'),
            '3:USDJPY:SELL@4:USDJPY:SELL',
            content_type='text/plain',
        )
        self.assertEqual(response.status_code, 200)
        self.assertIsNone(Order.objects.filter(ticket_no='1').first())
        self.assertIsNone(Order.objects.filter(ticket_no='2').first())
        self.assertIsNotNone(Order.objects.filter(ticket_no='3').first())
        self.assertIsNotNone(Order.objects.filter(ticket_no='4').first())
