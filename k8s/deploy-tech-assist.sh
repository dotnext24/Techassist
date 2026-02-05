#!/usr/bin/env bash
set -euo pipefail

echo "🚀 Starting TechAssistPro deployment to Kubernetes..."

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

NAMESPACE="techassistpro"
IMAGE_TAG="$(date +%Y%m%d%H%M%S)"

echo "📦 Using image tag: $IMAGE_TAG"

# --- Namespace (SAFE) ---
kubectl get namespace "$NAMESPACE" >/dev/null 2>&1 || \
  kubectl apply -f "$ROOT_DIR/k8s/deploy/namespace.yaml"

# --- Build images ---
echo "🐳 Building Docker images..."

docker build -t techassistpro/ticketing:$IMAGE_TAG \
  -f "$ROOT_DIR/src/TechAssistPro.Ticketing/Dockerfile" "$ROOT_DIR"

docker build -t techassistpro/scheduling:$IMAGE_TAG \
  -f "$ROOT_DIR/src/TechAssistPro.Scheduling/Dockerfile" "$ROOT_DIR"

docker build -t techassistpro/customer:$IMAGE_TAG \
  -f "$ROOT_DIR/src/TechAssistPro.CustomerManagement/Dockerfile" "$ROOT_DIR"

docker build -t techassistpro/gateway:$IMAGE_TAG \
  -f "$ROOT_DIR/src/TechAssistPro.Gateway/Dockerfile" "$ROOT_DIR"

# --- Load images into containerd ---
for svc in ticketing scheduling customer gateway; do
  docker save techassistpro/$svc:$IMAGE_TAG -o "/tmp/$svc.tar"
  ctr -n k8s.io images import "/tmp/$svc.tar"
done

# --- Update manifests ---
echo "📝 Updating Kubernetes manifests..."
for svc in ticketing scheduling customer gateway; do
  sed -i \
    "s|image: techassistpro/$svc:.*|image: techassistpro/$svc:$IMAGE_TAG|g" \
    "$ROOT_DIR/k8s/deploy/${svc}-deployment.yaml"
done

# --- Apply manifests ---
echo "☸️ Applying Kubernetes manifests..."
kubectl apply -n "$NAMESPACE" -f "$ROOT_DIR/k8s/deploy/"

# --- Wait for readiness ---
echo "⏳ Waiting for pods..."
kubectl wait -n "$NAMESPACE" --for=condition=ready pod --all --timeout=300s

# --- Access info ---
GATEWAY_PORT=$(kubectl get svc gateway -n "$NAMESPACE" -o jsonpath='{.spec.ports[0].nodePort}')
NODE_IP=$(hostname -I | awk '{print $1}')

echo "✅ Deployment successful!"
echo "🌐 Gateway: http://$NODE_IP:$GATEWAY_PORT"
