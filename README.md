# Teste Técnico – Desenvolvedor C# API

## 📌 Visão Geral

Este projeto foi desenvolvido como parte do processo seletivo. O objetivo é criar uma API para gerenciamento de contas correntes, autenticação de usuários, movimentações financeiras e transferências entre contas da mesma instituição.

A arquitetura adota boas práticas como autenticação via JWT, persistência com **Dapper**, uso de **cache com Redis**, mensageria com **Kafka** e configuração de containers via **Docker** (com `docker-compose`).

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

* **Mensageria com Kafka**

  * Implementação de produtores e consumidores com **KafkaFlow**.
  * Criação do **serviço de Tarifas**, que consome eventos de transferência e publica eventos de tarifação.
  * A **API de Conta Corrente** consome os eventos de tarifação e debita automaticamente a conta.

---

## ⚠️ Funcionalidades Parciais ou Não Implementadas

* **Testes de Integração para Transferência entre Contas**

  * Os testes de integração entre APIs não foram concluídos devido ao tempo limitado, mas a estrutura de endpoints e comunicação já está funcional.

---

## 🛠️ Tecnologias Utilizadas

* **.NET 8**
* **Dapper** – mapeamento objeto-relacional leve
* **SQLite** – banco de dados relacional
* **Redis** – cache distribuído
* **JWT** – autenticação via token
* **Apache Kafka** – mensageria assíncrona
* **KafkaFlow** – abstração para Kafka no .NET
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

3. Execute os containers (a aplicação + bancos + Redis + Kafka):

   ```bash
   docker compose up --build
   ```

4. Acesse a documentação da API via **Swagger**:

   ```
   http://localhost:<porta>/swagger
   ```

---

## 📂 Estrutura dos Containers

* **ContaCorrente.API** – aplicação principal
* **Tarifa.Worker** – worker de tarifação conectado ao Kafka
* **Kafka + Zookeeper** – mensageria
* **Banco de Dados** – SQLite
* **Redis** – cache em memória

---

## 🔮 Melhorias Futuras

* Implementar políticas de resiliência (retries, circuit breakers).
* Ajustar deploy para ambientes Kubernetes (infraestrutura já compatível).

