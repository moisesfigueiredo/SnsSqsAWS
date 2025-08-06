#!/bin/bash

# --- Variáveis de configuração ---
ACCOUNT_ID="000000000000"
QUEUE_NAME="minha-fila-de-mensagens"
TOPIC_NAME="meu-topico-sns"
REGION="us-east-1"

# Constroi os URLs e ARNs com base nas variáveis
QUEUE_URL="http://localstack:4566/${ACCOUNT_ID}/${QUEUE_NAME}"
QUEUE_ARN="arn:aws:sqs:${REGION}:${ACCOUNT_ID}:${QUEUE_NAME}"
TOPIC_ARN="arn:aws:sns:${REGION}:${ACCOUNT_ID}:${TOPIC_NAME}"

# --- Criação e Configuração ---

echo "📬 Criando fila SQS '${QUEUE_NAME}' na região '${REGION}'..."
awslocal sqs create-queue --queue-name "$QUEUE_NAME"

echo "🔧 Criando tópico SNS '${TOPIC_NAME}' na região '${REGION}'..."
awslocal sns create-topic --name "$TOPIC_NAME"

echo "🔗 Assinando fila ao tópico..."
awslocal sns subscribe \
    --topic-arn "$TOPIC_ARN" \
    --protocol sqs \
    --notification-endpoint "$QUEUE_ARN"

echo "🔐 Aplicando política à fila..."
awslocal sqs set-queue-attributes \
    --queue-url "$QUEUE_URL" \
    --attributes "{\"Policy\": \"{\\\"Version\\\":\\\"2012-10-17\\\",\\\"Statement\\\":[{\\\"Effect\\\":\\\"Allow\\\",\\\"Principal\\\":\\\"*\\\",\\\"Action\\\":\\\"sqs:SendMessage\\\",\\\"Resource\\\":\\\"${QUEUE_ARN}\\\",\\\"Condition\\\":{\\\"ArnEquals\\\":{\\\"aws:SourceArn\\\":\\\"${TOPIC_ARN}\\\"}}}]}\"}"

echo "✅ Setup concluído!"