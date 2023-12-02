コピートレードシステム (Backend)
==============================

[![Deploy To Azure](https://raw.githubusercontent.com/Azure/azure-quickstart-templates/master/1-CONTRIBUTION-GUIDE/images/deploytoazure.svg?sanitize=true)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fchameleonhead%2Ftrsys%2Fdev%2Fcsharp%2Fazuredeploy.json)


```powershell
$rg = 'trsys'
New-AzResourceGroup -Name $rg -Location japaneast -Force
New-AzResourceGroupDeployment -Name 'trsys-backend' -ResourceGroupName $rg -TemplateFile 'azuredeploy.json' -dbAdminName 'trsys' -dbAdminPassword 'P@ssw0rd'
```