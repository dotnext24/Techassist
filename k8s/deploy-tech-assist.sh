#!/bin/bash

# Exit on error
set -e

echo "🚀 Starting TechAssistPro deployment to Kubernetes..."

# Change to the project root directory
cd ..

# Deletes everything inside the namespace
kubectl delete namespace techassistpro --wait=true

#Recreate namespace
kubectl apply -f k8s/deploy/namespace.yaml
kubectl config set-context --current --namespace=techassistpro

IMAGE_TAG=$(date +%Y%m%d%H%M%S)
echo "📋 Using Git commit SHA as image tag: $IMAGE_TAG"

# 1. Build Docker images
echo "🐳 Building Docker images..."
docker build -t techassistpro/ticketing:$IMAGE_TAG -f src/TechAssistPro.Ticketing/Dockerfile .
docker build -t techassistpro/scheduling:$IMAGE_TAG -f src/TechAssistPro.Scheduling/Dockerfile .
docker build -t techassistpro/customer:$IMAGE_TAG -f src/TechAssistPro.CustomerManagement/Dockerfile .
docker build -t techassistpro/gateway:$IMAGE_TAG -f src/TechAssistPro.Gateway/Dockerfile .

docker save techassistpro/ticketing:$IMAGE_TAG -o ticketing_$IMAGE_TAG.tar
docker save techassistpro/scheduling:$IMAGE_TAG -o scheduling_$IMAGE_TAG.tar
docker save techassistpro/customer:$IMAGE_TAG -o customer_$IMAGE_TAG.tar
docker save techassistpro/gateway:$IMAGE_TAG -o gateway_$IMAGE_TAG.tar

ctr -n k8s.io images import ticketing_$IMAGE_TAG.tar
ctr -n k8s.io images import scheduling_$IMAGE_TAG.tar
ctr -n k8s.io images import customer_$IMAGE_TAG.tar
ctr -n k8s.io images import gateway_$IMAGE_TAG.tar

# Update Kubernetes manifests with the new tags
echo "📝 Updating Kubernetes manifests..."
for service in ticketing scheduling customer gateway; do
    # Update deployment.yaml
    sed -i "s|image: techassistpro/$service:.*|image: techassistpro/$service:$IMAGE_TAG|g" k8s/deploy/${service}-deployment.yaml

done

# 2. Apply Kubernetes manifests
echo "☸️ Applying Kubernetes manifests..."
kubectl apply -n techassistpro -f k8s/deploy/

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