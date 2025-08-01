#!/bin/bash

# Criar a fila SQS
awslocal sqs create-queue --queue-name minha-fila-de-mensagens

# Criar o tópico SNS
awslocal sns create-topic --name meu-topico-sns

# Subscrever a fila SQS ao tópico SNS
awslocal sns subscribe \
    --topic-arn arn:aws:sns:us-east-1:000000000000:meu-topico-sns \
    --protocol sqs \
    --notification-endpoint arn:aws:sqs:us-east-1:000000000000:minha-fila-de-mensagens