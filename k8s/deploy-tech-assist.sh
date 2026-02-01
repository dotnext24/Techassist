#!/bin/bash

# Exit on error
set -e

echo "🚀 Starting TechAssistPro deployment to Kubernetes..."

# Change to the project root directory
cd ..

# 1. Build Docker images
echo "🐳 Building Docker images..."
docker build -t techassistpro/ticketing:latest -f src/TechAssistPro.Ticketing/Dockerfile .
docker build -t techassistpro/scheduling:latest -f src/TechAssistPro.Scheduling/Dockerfile .
docker build -t techassistpro/customer:latest -f src/TechAssistPro.CustomerManagement/Dockerfile .
docker build -t techassistpro/gateway:latest -f src/TechAssistPro.Gateway/Dockerfile .

# 2. Apply Kubernetes manifests
echo "☸️ Applying Kubernetes manifests..."
kubectl apply -f k8s/deploy/

# 3. Wait for pods to be ready
echo "⏳ Waiting for pods to be ready..."
kubectl wait --for=condition=ready pod --all --timeout=300s

# 5. Get NodePort details
GATEWAY_SERVICE=$(kubectl get svc gateway -o jsonpath='{.spec.ports[0].nodePort}')
NODE_IP=$(hostname -I | awk '{print $1}')

echo "✅ TechAssistPro deployed successfully!"
echo "🌐 Access the Gateway at: http://$NODE_IP:$GATEWAY_SERVICE"
echo ""
echo "📋 Useful commands:"
echo "   kubectl get pods -A          # List all pods"
echo "   kubectl get svc -A           # List all services"
echo "   kubectl logs <pod-name>      # View pod logs"
echo "   kubectl describe pod <pod>   # Debug a pod"