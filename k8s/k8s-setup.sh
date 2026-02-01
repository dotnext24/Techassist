#!/bin/bash
set -e

echo "🚀 Kubernetes setup for Ubuntu 24.04 starting..."

# -----------------------------------
# 1. System prep
# -----------------------------------
sudo apt update && sudo apt upgrade -y

echo "🔧 Disabling swap"
sudo swapoff -a
sudo sed -i '/ swap / s/^/#/' /etc/fstab

# -----------------------------------
# 2. Kernel modules & sysctl
# -----------------------------------
echo "🔧 Configuring kernel modules"
sudo modprobe overlay
sudo modprobe br_netfilter

cat <<EOF | sudo tee /etc/modules-load.d/k8s.conf
overlay
br_netfilter
EOF

cat <<EOF | sudo tee /etc/sysctl.d/k8s.conf
net.bridge.bridge-nf-call-iptables = 1
net.bridge.bridge-nf-call-ip6tables = 1
net.ipv4.ip_forward = 1
EOF

sudo sysctl --system

# -----------------------------------
# 3. Install containerd
# -----------------------------------
echo "🐳 Installing containerd"
sudo apt install -y containerd

sudo mkdir -p /etc/containerd
containerd config default | sudo tee /etc/containerd/config.toml > /dev/null

# IMPORTANT: systemd cgroup
sudo sed -i 's/SystemdCgroup = false/SystemdCgroup = true/' \
  /etc/containerd/config.toml

sudo systemctl restart containerd
sudo systemctl enable containerd

# -----------------------------------
# 4. Install Kubernetes packages
# -----------------------------------
echo "☸️ Installing kubeadm, kubelet, kubectl"

sudo apt install -y apt-transport-https ca-certificates curl gpg
sudo mkdir -p /etc/apt/keyrings

curl -fsSL https://pkgs.k8s.io/core:/stable:/v1.35/deb/Release.key \
  | sudo gpg --dearmor -o /etc/apt/keyrings/kubernetes-apt-keyring.gpg

echo "deb [signed-by=/etc/apt/keyrings/kubernetes-apt-keyring.gpg] \
https://pkgs.k8s.io/core:/stable:/v1.35/deb/ /" | \
sudo tee /etc/apt/sources.list.d/kubernetes.list

sudo apt update
sudo apt install -y kubelet kubeadm kubectl
sudo apt-mark hold kubelet kubeadm kubectl

sudo systemctl enable kubelet

echo "✅ Base Kubernetes setup complete"
echo "👉 Reboot before kubeadm init"

sudo reboot

# 5. Initialize Kubernetes cluster
echo "🛠️ Initializing Kubernetes cluster..."
sudo kubeadm init --pod-network-cidr=10.244.0.0/16

# 6. Set up kubectl for current user
echo "👤 Setting up kubectl for current user..."
mkdir -p $HOME/.kube
sudo cp /etc/kubernetes/admin.conf $HOME/.kube/config
sudo chown $(id -u):$(id -g) $HOME/.kube/config


# 7. Install Flannel CNI
echo "🌐 Installing Flannel CNI..."
kubectl apply -f https://raw.githubusercontent.com/coreos/flannel/master/Documentation/kube-flannel.yml


# 8. Verify cluster status
echo "⏳ Waiting for cluster to be ready..."
sleep 30  # Give some time for the cluster to initialize
kubectl get nodes

echo "✅ Kubernetes setup completed successfully!"
echo "📌 To use kubectl as a non-root user, run:"
echo "   mkdir -p \$HOME/.kube"
echo "   sudo cp -i /etc/kubernetes/admin.conf \$HOME/.kube/config"
echo "   sudo chown \$(id -u):\$(id -g) \$HOME/.kube/config"