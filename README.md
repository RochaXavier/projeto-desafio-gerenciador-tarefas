# Projeto Gerencimanento de tarefas

Este é o projeto/desafio para estudo das tecnologias abaixo e suas integrações:
- .NET 8
- RabbitMQ
- MongoDB
- MediatR
- Docker Composer
- E outras bibliotecas como: FluentValidation, Swashbuckle (Swagger), Polly

A aplicação tem como objetivo simular a criação, consultas e processamentos de tarefas de maneira assincronas com processamentos de fila no RabbitMQ e controle de dados via MongoDB

Para executar essa aplicação **é preciso ter o docker instalado**.

Execute o comando abaixo na raiz do projeto para iniciar a aplicação:
```
docker-compose up -d
```

Para finalizar a aplicação execute o comando abaixo:
```
docker-compose down
```

Para acompanhar a fila é possivel acessar o painel admintrativo do RabbitMQ: [http://localhost:15672](http://localhost:15672)

Documentação da API: [https://localhost:8091/swagger/index.html](https://localhost:8091/swagger/index.html)

Para acessar a base de dados pode ser utilizada a connection string configurada no arquivo **.ENV**.

Observação: _O arquivo .ENV na raiz do projeto possui dados sensiveis e não devem estar no repositório normalmente, deve ser configurado na medida do nescessário
em cada projeto e só foi adicionado aqui para auxiliar o entendimento do projeto_

