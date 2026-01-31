#!/bin/bash

# Exit on error
set -e

# Check if running as root
if [ "$(id -u)" -ne 0 ]; then
    echo "Please run as root (sudo)." >&2
    exit 1
fi

echo "🚀 Starting Kubernetes setup on Ubuntu 24.04..."

# 1. Disable swap
echo "🔧 Disabling swap..."
sudo swapoff -a
sed -i '/ swap / s/^\(.*\)$/#\1/g' /etc/fstab

# 2. Install dependencies
echo "📦 Installing dependencies..."
apt update
apt install -y apt-transport-https ca-certificates curl gnupg lsb-release

# 3. Add Kubernetes repository
echo "🔑 Adding Kubernetes repository..."
mkdir -p /etc/apt/keyrings
curl -fsSL https://pkgs.k8s.io/core:/stable:/v1.28/deb/Release.key | gpg --dearmor -o /etc/apt/keyrings/kubernetes-apt-keyring.gpg
echo 'deb [signed-by=/etc/apt/keyrings/kubernetes-apt-keyring.gpg] https://pkgs.k8s.io/core:/stable:/v1.28/deb/ /' | tee /etc/apt/sources.list.d/kubernetes.list

# 4. Install Kubernetes tools
echo "📥 Installing kubeadm, kubelet, and kubectl..."
apt update
apt install -y kubelet kubeadm kubectl
apt-mark hold kubelet kubeadm kubectl

# 5. Initialize Kubernetes cluster
echo "🛠️ Initializing Kubernetes cluster..."
kubeadm init --pod-network-cidr=10.244.0.0/16

# 6. Set up kubectl for current user
echo "👤 Setting up kubectl for current user..."
mkdir -p $HOME/.kube
cp -i /etc/kubernetes/admin.conf $HOME/.kube/config
chown $(id -u):$(id -g) $HOME/.kube/config

# 7. Install Flannel CNI
echo "🌐 Installing Flannel CNI..."
kubectl apply -f https://github.com/flannel-io/flannel/releases/latest/download/kube-flannel.yml

# 8. Verify cluster status
echo "⏳ Waiting for cluster to be ready..."
sleep 30  # Give some time for the cluster to initialize
kubectl get nodes

echo "✅ Kubernetes setup completed successfully!"
echo "📌 To use kubectl as a non-root user, run:"
echo "   mkdir -p \$HOME/.kube"
echo "   sudo cp -i /etc/kubernetes/admin.conf \$HOME/.kube/config"
echo "   sudo chown \$(id -u):\$(id -g) \$HOME/.kube/config"