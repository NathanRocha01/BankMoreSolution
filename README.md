# Teste Técnico – Desenvolvedor C# API

## 📌 Visão Geral

Este projeto foi desenvolvido como parte do processo seletivo. O objetivo é criar uma API para gerenciamento de contas correntes, autenticação de usuários, movimentações financeiras e transferências entre contas da mesma instituição.

A arquitetura adota boas práticas como autenticação via JWT, persistência com **Dapper**, uso de **cache com Redis** e configuração de containers via **Docker** (com `docker-compose`).

---

## ✅ Funcionalidades Implementadas

* **Cadastro de Conta Corrente**

  * Criação de contas com CPF e senha.
  * Validação de CPF.
  * Retorno do número da conta.

* **Autenticação/Login**

  * Login por CPF ou número da conta.
  * Geração de token JWT válido.

* **Movimentações (Depósito e Saque)**

  * Registro de créditos e débitos.
  * Validações de tipo e valor.
  * Persistência em banco SQLite.

* **Consulta de Saldo**

  * Retorno de saldo atual da conta.
  * Validações de conta ativa/inexistente.
  * Uso de **cache Redis** para otimizar consultas.

* **Inativação de Conta Corrente**

  * Atualização do status da conta para inativo.

---

## ⚠️ Funcionalidades Não Implementadas

* **Transferência entre contas (Testes de Integração)**

  * Embora o conceito e endpoints tenham sido estruturados, os **testes de integração entre APIs** não foram concluídos devido ao tempo limitado.

* **API de Tarifas com Kafka (Opcional no teste)**

  * Apesar do entendimento conceitual sobre mensageria e Kafka, optei por não implementar para priorizar a entrega no prazo, visto que é uma tecnologia que ainda não utilizei em produção.

---

## 🛠️ Tecnologias Utilizadas

* **.NET 8**
* **Dapper** – mapeamento objeto-relacional leve
* **SQLite** – banco de dados relacional
* **Redis** – cache distribuído
* **JWT** – autenticação via token
* **Docker** + **Docker Compose** – orquestração e execução dos containers

---

## 🚀 Como Executar o Projeto

1. Clone este repositório:

   ```bash
   git clone <url-do-repositorio>
   ```

2. Acesse a pasta do projeto:

   ```bash
   cd <nome-do-projeto>
   ```

3. Execute os containers (a aplicação + banco de dados + Redis):

   ```bash
   docker compose up --build
   ```

   > Obs.: O arquivo está no formato `.yml` em vez de `.yaml`, mas isso não afeta o funcionamento.

4. Acesse a documentação da API via **Swagger**:

   ```
   http://localhost:<porta>/swagger
   ```

---

## 📂 Estrutura dos Containers

* **API** – aplicação principal
* **Banco de Dados** – SQLite
* **Redis** – cache em memória
* **(Opcional para futura implementação)** – Kafka, consumidores e produtores de mensagens

---

## 🔮 Melhorias Futuras

* Implementar testes de integração para transferência entre contas.
* Adicionar microsserviço de **Tarifas** integrado ao Kafka.
* Expandir uso de cache Redis para outras consultas críticas.
* Implementar resiliência de comunicação entre APIs.
* Ajustar deploy para ambientes Kubernetes (conforme especificação informativa do teste).
