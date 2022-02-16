# ApiTODOSwagger
 
Alterar ConnectionStrings em appsettings.json
 
Cria o arquivo de migração:
add-migration Inicial

Executa a migração: 
update-database

Para refazer:

Drop nas tabelas criadas:
Update-Database -Migration 0

Deleta o arquivo de migração criado
Remove-Migration
