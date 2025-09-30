Teste Técnico – Desenvolvedor C# API
📌 Visão Geral

Este projeto foi desenvolvido como parte do processo seletivo para a vaga de Desenvolvedor C#.
O sistema implementa funcionalidades de conta corrente, autenticação de usuários, movimentações financeiras e consulta de saldo.

A solução foi construída em .NET 8, utiliza Dapper para persistência de dados em SQLite, Redis para cache, autenticação via JWT e conteinerização com Docker.

✅ Funcionalidades Implementadas

Cadastro de Conta Corrente (com validação de CPF)

Autenticação/Login (com geração de token JWT)

Movimentações (depósitos e saques, com validações de tipo e valor)

Consulta de Saldo (com otimização via Redis)

Inativação de Conta Corrente

⚠️ Funcionalidades Não Implementadas

Testes de integração da Transferência entre contas (não finalizados devido à comunicação entre APIs).

API de Tarifas com Kafka (opcional no teste; não implementada para priorizar prazo).

🛠️ Tecnologias Utilizadas

.NET 8

Dapper – ORM leve

SQLite – Banco de dados relacional

Redis – Cache distribuído

JWT – Autenticação via token

Docker + Docker Compose – Orquestração de containers

🚀 Como Executar o Projeto (Arquivo ZIP)

Extraia o arquivo .zip em uma pasta local.

Certifique-se de ter instalado:

Docker Desktop

.NET 8 SDK

No diretório do projeto, execute:

docker compose up --build


Obs.: O arquivo está no formato .yml, mas funciona normalmente.

Após a inicialização, acesse a documentação da API via Swagger:

http://localhost:<porta>/swagger

📂 Estrutura dos Containers

API – aplicação principal

Banco de Dados (SQLite) – armazenamento de dados

Redis – cache em memória

(Opcional para futuro) – Kafka (não implementado nesta entrega)

🔮 Melhorias Futuras

Finalizar testes de integração para transferência entre contas.

Implementar microsserviço de Tarifas com mensageria via Kafka.

Ampliar uso de cache Redis em consultas críticas.

Implementar padrões de resiliência e idempotência.

Preparar deploy para ambientes Kubernetes.