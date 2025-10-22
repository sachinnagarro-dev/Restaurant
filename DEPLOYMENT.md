# TableOrder Restaurant App - Production Deployment Guide

This guide covers deploying the TableOrder restaurant management system to production using Docker, Docker Compose, and Kubernetes.

## 🏗️ Architecture

- **Backend**: .NET 8 Web API with JWT authentication
- **Frontend**: React + TypeScript apps (Admin, Kitchen, Tablet)
- **Database**: PostgreSQL
- **Cache**: Redis
- **Reverse Proxy**: Nginx (for frontend)

## 📦 Docker Deployment

### Prerequisites

- Docker and Docker Compose installed
- Environment variables configured

### Quick Start

1. **Clone and configure**:
   ```bash
   git clone <repository-url>
   cd RestaurantApp
   cp backend/env.prod.example backend/.env
   # Edit backend/.env with your production values
   ```

2. **Deploy with Docker Compose**:
   ```bash
   cd backend
   docker-compose -f docker-compose.prod.yml up -d
   ```

3. **Access the applications**:
   - Admin UI: http://localhost:3002
   - Kitchen UI: http://localhost:3001
   - Tablet UI: http://localhost:3000
   - Backend API: http://localhost:5000

### Environment Variables

Key production environment variables:

```bash
# Database
POSTGRES_PASSWORD=your_secure_password

# JWT Security
JWT_SECRET_KEY=your_32_character_secret_key
ADMIN_KEY=your_admin_key
ADMIN_PASSWORD=your_admin_password
STAFF_PASSWORD=your_staff_password
```

## ☸️ Kubernetes Deployment

### Prerequisites

- Kubernetes cluster (1.20+)
- kubectl configured
- Ingress controller (nginx-ingress recommended)
- cert-manager (for SSL certificates)

### Deploy to Kubernetes

1. **Update image references** in `k8s/*.yaml`:
   ```yaml
   # Replace 'your-registry' with your container registry
   image: your-registry/tableorder-backend:latest
   ```

2. **Create secrets**:
   ```bash
   # Update base64 encoded values in k8s/postgres.yaml
   kubectl apply -f k8s/postgres.yaml
   ```

3. **Deploy applications**:
   ```bash
   kubectl apply -f k8s/backend.yaml
   kubectl apply -f k8s/frontend.yaml
   kubectl apply -f k8s/ingress.yaml
   ```

4. **Update DNS** to point to your ingress controller

### Kubernetes Resources

- **ConfigMap**: Non-sensitive configuration
- **Secret**: Sensitive data (passwords, keys)
- **PVC**: Persistent storage for PostgreSQL
- **Deployments**: Application replicas
- **Services**: Internal load balancing
- **Ingress**: External access with SSL

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

The `.github/workflows/ci-cd.yml` provides:

1. **Build & Test**:
   - Backend: .NET build, test, Docker image
   - Frontend: Node.js build, lint, Docker images

2. **Security Scanning**:
   - Trivy vulnerability scanning
   - SARIF results to GitHub Security tab

3. **Deployment**:
   - Staging: `develop` branch
   - Production: `main` branch

### Setup CI/CD

1. **Configure secrets** in GitHub repository:
   ```
   GITHUB_TOKEN (automatic)
   ```

2. **Configure environments**:
   - `staging`: For develop branch deployments
   - `production`: For main branch deployments

3. **Update deployment scripts** in workflow for your infrastructure

## 🔒 Security Considerations

### Production Security Checklist

- [ ] Change all default passwords
- [ ] Use strong JWT secret keys (32+ characters)
- [ ] Enable HTTPS with valid certificates
- [ ] Configure firewall rules
- [ ] Regular security updates
- [ ] Monitor logs and metrics
- [ ] Backup database regularly

### SSL/TLS Configuration

For production, replace self-signed certificates with:
- Let's Encrypt certificates (via cert-manager)
- Commercial SSL certificates
- Proper certificate chain

## 📊 Monitoring & Observability

### Recommended Tools

- **Logging**: Fluentd, ELK Stack
- **Metrics**: Prometheus + Grafana
- **Tracing**: Jaeger
- **APM**: Application Performance Monitoring

### Health Checks

All services include health check endpoints:
- Backend: `/health`
- Database: `pg_isready`
- Redis: `redis-cli ping`

## 🚀 Scaling Considerations

### Horizontal Scaling

- **Backend**: Scale deployment replicas
- **Frontend**: Scale nginx replicas
- **Database**: Consider read replicas
- **Cache**: Redis cluster mode

### Performance Optimization

- Enable gzip compression
- Configure CDN for static assets
- Database connection pooling
- Redis caching strategies

## 🛠️ Troubleshooting

### Common Issues

1. **Database Connection**:
   ```bash
   kubectl logs -f deployment/postgres
   ```

2. **Backend Startup**:
   ```bash
   kubectl logs -f deployment/tableorder-backend
   ```

3. **Frontend Issues**:
   ```bash
   kubectl logs -f deployment/tableorder-frontend
   ```

### Debug Commands

```bash
# Check pod status
kubectl get pods

# Check service endpoints
kubectl get svc

# Check ingress
kubectl get ingress

# Describe resources
kubectl describe pod <pod-name>
```

## 📝 Maintenance

### Updates

1. **Backend**: Update image tag in deployment
2. **Frontend**: Update image tag in deployment
3. **Database**: Use migration scripts
4. **Configuration**: Update ConfigMap/Secret

### Backups

- **Database**: Regular pg_dump backups
- **Configuration**: Version control
- **Secrets**: Secure secret management

## 🔗 External Integrations

### Payment Gateways

Configure production payment gateway credentials:
- Stripe
- PayPal
- Square

### Email Services

Configure SMTP for notifications:
- SendGrid
- AWS SES
- SMTP relay

---

For additional support, refer to the application documentation or contact the development team.
