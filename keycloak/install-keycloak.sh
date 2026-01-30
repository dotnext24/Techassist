#!/bin/bash
# Keycloak Installation for Ubuntu 24.04

# Update system
sudo apt update && sudo apt upgrade -y

# Install Java 17
sudo apt install -y openjdk-17-jdk wget unzip


# Download Keycloak
cd /opt
sudo wget https://github.com/keycloak/keycloak/releases/download/23.0.4/keycloak-23.0.4.zip
sudo unzip keycloak-23.0.4.zip
sudo mv keycloak-23.0.4 keycloak

# Create Keycloak user
sudo useradd -r -s /bin/false keycloak
sudo chown -R keycloak:keycloak /opt/keycloak

# Configure Keycloak
sudo tee /opt/keycloak/conf/keycloak.conf > /dev/null <<EOF
db=postgres
db-url=jdbc:postgresql://localhost:5432/keycloak
db-username=keycloak
db-password=Coder##12
hostname=172.232.102.50
hostname-strict=false
hostname-stict-https=false
http-enabled=true
http-port=8000
proxy-headers=xforwarded
EOF

# Build Keycloak
cd /opt/keycloak
sudo -u keycloak ./bin/kc.sh build

# Create systemd service
sudo tee /etc/systemd/system/keycloak.service > /dev/null <<EOF
[Unit]
Description=Keycloak
After=postgresql.service

[Service]
Type=idle
User=keycloak
Group=keycloak
Environment="KEYCLOAK_ADMIN=dotnext24"
Environment="KEYCLOAK_ADMIN_PASSWORD=Coder##12"
ExecStart=/opt/keycloak/bin/kc.sh start-dev
Restart=on-failure

[Install]
WantedBy=multi-user.target
EOF

# Start Keycloak
sudo systemctl daemon-reload
sudo systemctl start keycloak
sudo systemctl enable keycloak

echo "Keycloak installed! Access at: http://localhost:8000"