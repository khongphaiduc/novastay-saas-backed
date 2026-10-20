#!/bin/bash
# ╔══════════════════════════════════════════════════════════════╗
# ║  NovaStay — VPS Setup & Deployment Script                   ║
# ║  Target: Fresh Ubuntu 22.04+ VPS with 8GB RAM               ║
# ║  Usage: source deploy.sh && phase_a_prepare                 ║
# ╚══════════════════════════════════════════════════════════════╝

set -e

echo "======================================"
echo "  NovaStay VPS Deployment Script"
echo "======================================"

# ──────────────────────────────────────────
# PHASE A: System Preparation
# ──────────────────────────────────────────
phase_a_prepare() {
    echo ">>> [Phase A] Preparing VPS..."

    # Step 1: Update system
    echo ">>> Step 1: Updating system..."
    sudo apt update && sudo apt upgrade -y
    sudo apt install -y curl wget git unzip software-properties-common apt-transport-https ca-certificates gnupg lsb-release

    # Step 2: Install Docker
    echo ">>> Step 2: Installing Docker..."
    if ! command -v docker &> /dev/null; then
        curl -fsSL https://get.docker.com | sh
        sudo usermod -aG docker $USER
        echo ">>> Docker installed. Run: newgrp docker (or logout/login)"
    else
        echo ">>> Docker already installed: $(docker --version)"
    fi

    # Step 3: Install Nginx
    echo ">>> Step 3: Installing Nginx..."
    sudo apt install -y nginx
    sudo systemctl enable nginx
    sudo systemctl start nginx

    # Step 4: Install Node.js 20
    echo ">>> Step 4: Installing Node.js 20..."
    if ! command -v node &> /dev/null; then
        curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
        sudo apt install -y nodejs
    fi
    echo ">>> Node.js: $(node --version), npm: $(npm --version)"

    # Step 5: Install Certbot
    echo ">>> Step 5: Installing Certbot..."
    sudo apt install -y certbot python3-certbot-nginx

    # Step 6: Create directories
    echo ">>> Step 6: Creating directories..."
    sudo mkdir -p /opt/novastay/{backend,frontend,data/{sqlserver,redis,rabbitmq,minio,jenkins},env,deploy/nginx}
    sudo chown -R $USER:$USER /opt/novastay

    echo ">>> [Phase A] DONE! Now copy deploy files to /opt/novastay/"
}

# ──────────────────────────────────────────
# PHASE C: Start Infrastructure Services
# ──────────────────────────────────────────
phase_c_infrastructure() {
    echo ">>> [Phase C] Starting infrastructure services..."
    cd /opt/novastay/deploy

    echo ">>> Starting SQL Server, Redis, RabbitMQ, MinIO..."
    docker compose --env-file /opt/novastay/env/.env up -d novastay-db novastay-redis novastay-rabbit novastay-minio

    echo ">>> Waiting for services to be healthy (~30-60s)..."
    sleep 30
    docker compose ps

    echo ">>> Creating NovaStayDatabase..."
    docker exec novastay-db /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$(grep SA_PASSWORD /opt/novastay/env/.env | cut -d= -f2)" -C \
        -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'NovaStayDatabase') CREATE DATABASE NovaStayDatabase;"

    echo ">>> [Phase C] DONE!"
}

# ──────────────────────────────────────────
# PHASE D: Deploy Backend
# ──────────────────────────────────────────
phase_d_backend() {
    echo ">>> [Phase D] Deploying backend..."
    cd /opt/novastay/deploy

    echo ">>> Starting backend API container..."
    docker compose --env-file /opt/novastay/env/.env up -d novastay-api

    sleep 15
    docker logs --tail 20 novastay-api

    echo ">>> Testing backend..."
    curl -s -o /dev/null -w "HTTP Status: %{http_code}\n" http://localhost:8080/api/rooms || true

    echo ">>> [Phase D] DONE!"
}

# ──────────────────────────────────────────
# PHASE E: Deploy Frontend
# ──────────────────────────────────────────
phase_e_frontend() {
    echo ">>> [Phase E] Deploying frontend..."
    cd /opt/novastay/frontend/NovaStay

    echo ">>> Building frontend..."
    npm install
    npm run build

    echo ">>> Copying to /var/www/novastay/..."
    sudo mkdir -p /var/www/novastay
    sudo rm -rf /var/www/novastay/*
    sudo cp -r dist/* /var/www/novastay/
    sudo chown -R www-data:www-data /var/www/novastay

    echo ">>> [Phase E] DONE!"
}

# ──────────────────────────────────────────
# PHASE F: Configure Nginx + SSL
# ──────────────────────────────────────────
phase_f_nginx() {
    echo ">>> [Phase F] Configuring Nginx..."

    sudo cp /opt/novastay/deploy/nginx/novastay-frontend.conf /etc/nginx/sites-available/
    sudo cp /opt/novastay/deploy/nginx/novastay-api.conf /etc/nginx/sites-available/
    sudo ln -sf /etc/nginx/sites-available/novastay-frontend.conf /etc/nginx/sites-enabled/
    sudo ln -sf /etc/nginx/sites-available/novastay-api.conf /etc/nginx/sites-enabled/
    sudo rm -f /etc/nginx/sites-enabled/default

    sudo nginx -t && sudo systemctl reload nginx
    echo ">>> [Phase F] Nginx configured (HTTP). Run phase_f_ssl after DNS setup."
}

phase_f_ssl() {
    echo ">>> Installing SSL certificates..."
    read -p "DNS already pointing to this VPS? (y/n): " dns_ready
    [ "$dns_ready" != "y" ] && echo "Point DNS first." && return 1

    sudo certbot --nginx \
        -d nestone.io.vn \
        -d www.nestone.io.vn \
        -d api.nestone.io.vn \
        --non-interactive --agree-tos \
        -m hotelluxurytrungduc@gmail.com

    sudo certbot renew --dry-run
    echo ">>> SSL installed!"
}

# ──────────────────────────────────────────
# PHASE G: Firewall + Jenkins + Testing
# ──────────────────────────────────────────
phase_g_firewall() {
    sudo ufw default deny incoming
    sudo ufw default allow outgoing
    sudo ufw allow 22/tcp
    sudo ufw allow 80/tcp
    sudo ufw allow 443/tcp
    sudo ufw --force enable
    sudo ufw status verbose
    echo ">>> Firewall configured!"
}

phase_g_jenkins() {
    cd /opt/novastay/deploy
    docker compose --env-file /opt/novastay/env/.env up -d novastay-jenkins
    sleep 15
    echo ">>> Jenkins initial password:"
    docker exec novastay-jenkins cat /var/jenkins_home/secrets/initialAdminPassword 2>/dev/null || echo "Still starting..."
    echo ">>> Access: ssh -L 8081:localhost:8081 user@vps-ip then http://localhost:8081"
}

phase_g_test() {
    echo ">>> Running tests..."
    echo ""
    for svc in novastay-db novastay-redis novastay-rabbit novastay-minio novastay-api; do
        STATUS=$(docker inspect --format='{{.State.Status}}' $svc 2>/dev/null || echo "not found")
        [ "$STATUS" = "running" ] && echo "  ✅ $svc: running" || echo "  ❌ $svc: $STATUS"
    done
    echo ""
    curl -s -o /dev/null -w "  API: HTTP %{http_code}\n" http://localhost:8080/api/rooms 2>/dev/null || echo "  ❌ API not responding"
    curl -s -o /dev/null -w "  Frontend: HTTP %{http_code}\n" http://localhost/ 2>/dev/null || echo "  ❌ Frontend not responding"
    echo ""
    echo ">>> Tests complete!"
}

echo ""
echo "Functions available:"
echo "  phase_a_prepare        - Install Docker, Nginx, Node.js, Certbot"
echo "  phase_c_infrastructure - Start DB, Redis, RabbitMQ, MinIO"
echo "  phase_d_backend        - Deploy backend API"
echo "  phase_e_frontend       - Build and deploy frontend"
echo "  phase_f_nginx          - Configure Nginx (HTTP)"
echo "  phase_f_ssl            - Install SSL (after DNS)"
echo "  phase_g_firewall       - Configure UFW firewall"
echo "  phase_g_jenkins        - Start Jenkins"
echo "  phase_g_test           - Run tests"
echo ""
echo "Usage: source deploy.sh && phase_a_prepare"
