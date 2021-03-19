from django.test import TestCase
from django.urls import reverse

from .models import Account


class AccountIndexTests(TestCase):
    def test_get_account_should_return_not_found(self):
        """
        /api/accountsにgetすると404になる
        """
        response = self.client.get(reverse('api:account_index'))
        self.assertEqual(response.status_code, 404)

    def test_post_account_should_create_new_account(self):
        """
        アカウントが存在しない場合に/api/accountsにpostするとアカウントが作成される
        """
        response = self.client.post(
            reverse('api:account_index'),
            {
                'accountcompany': 'Account Company',
                'accountname': 'Taro Test',
                'accountnumber': '11111111',
                'email': 'email@example.com',
                'isdemo': '1'
            },
        )
        self.assertEqual(response.status_code, 201)
        result = Account.objects.filter(account_number='11111111').first()
        self.assertIsNotNone(result)

    def test_post_account_given_account_exists_should_update_account(self):
        """
        アカウントが存在する場合に/api/accountsにpostするとアカウントが更新される
        """
        account = Account(
            account_company='Account Company',
            account_name='Taro Test',
            account_number='11111111',
            email='email@example.com',
            is_demo=True
        )
        account.save()
        response = self.client.post(
            reverse('api:account_index'),
            {
                'accountcompany': 'Account Company2',
                'accountname': 'Taro Test2',
                'accountnumber': '11111111',
                'email': 'email2@example.com',
                'isdemo': '0'
            },
        )
        self.assertEqual(response.status_code, 204)
        result = Account.objects.filter(account_number='11111111').first()
        self.assertIsNotNone(result)
