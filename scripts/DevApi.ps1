# =============================================================================
# DevApi.ps1 — chamadas de teste em PowerShell
# Uso: .\scripts\DevApi.ps1
#      ou cole trechos no terminal com o Gateway e serviços já rodando.
# =============================================================================

$ErrorActionPreference = "Stop"

$Gateway = "http://localhost:5000"
$CustomerService = "http://localhost:5002"

Write-Host "== Customers: POST (gateway) ==" -ForegroundColor Cyan
$body = @{
    name           = "Cliente Teste"
    email          = "cliente@teste.com"
    phone          = "+5511999999999"
    documentNumber = "12345678901"
} | ConvertTo-Json

$created = Invoke-RestMethod -Method Post -Uri "$Gateway/api/customers" -Body $body -ContentType "application/json; charset=utf-8"
$created | ConvertTo-Json
$customerId = $created.id
Write-Host "customerId para PUT/DELETE: $customerId" -ForegroundColor Yellow

Write-Host "`n== Customers: GET todos (gateway) ==" -ForegroundColor Cyan
Invoke-RestMethod -Uri "$Gateway/api/customers" | ConvertTo-Json

Write-Host "`n== Customers: GET por e-mail (gateway) ==" -ForegroundColor Cyan
$enc = [uri]::EscapeDataString("cliente@teste.com")
Invoke-RestMethod -Uri "$Gateway/api/customers/email/$enc" | ConvertTo-Json

# --- Descomente para testar PUT / DELETE com o id criado acima ---
# Write-Host "`n== Customers: PUT ==" -ForegroundColor Cyan
# $put = @{ name = "Cliente Teste Atualizado"; email = "cliente@teste.com"; phone = "+5511888888888"; documentNumber = "12345678901" } | ConvertTo-Json
# Invoke-RestMethod -Method Put -Uri "$Gateway/api/customers/$customerId" -Body $put -ContentType "application/json; charset=utf-8" | ConvertTo-Json
#
# Write-Host "`n== Customers: DELETE ==" -ForegroundColor Cyan
# Invoke-RestMethod -Method Delete -Uri "$Gateway/api/customers/$customerId"

Write-Host "`n== Chatbot: GET customer ==" -ForegroundColor Cyan
Invoke-RestMethod -Uri "$Gateway/api/chatbot/customer?email=$enc" | ConvertTo-Json

Write-Host "`nPara novos POST/GET/PUT: duplique um bloco acima e mude Uri, Method e Body." -ForegroundColor DarkGray
Write-Host "Customer direto (sem gateway): $CustomerService/api/customers" -ForegroundColor DarkGray
