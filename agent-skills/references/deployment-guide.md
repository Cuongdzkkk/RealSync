# 🚀 Deployment Guide — RealSync

> Hướng dẫn deploy hệ thống RealSync lên các môi trường.

---

## 1. Environments

| Environment | Branch | URL | Purpose |
|-------------|--------|-----|---------|
| Development | `feature/*` | `localhost` | Local development |
| Staging | `develop` | `staging.realsync.vn` | QA testing |
| Production | `main` | `realsync.vn` | Live system |

---

## 2. Docker Setup

### 2.1 Backend Dockerfile
```dockerfile
# backend/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/RealSync.Api/RealSync.Api.csproj", "RealSync.Api/"]
COPY ["src/RealSync.Core/RealSync.Core.csproj", "RealSync.Core/"]
COPY ["src/RealSync.Services/RealSync.Services.csproj", "RealSync.Services/"]
COPY ["src/RealSync.Data/RealSync.Data.csproj", "RealSync.Data/"]
COPY ["src/RealSync.Shared/RealSync.Shared.csproj", "RealSync.Shared/"]
RUN dotnet restore "RealSync.Api/RealSync.Api.csproj"

COPY src/ .
WORKDIR "/src/RealSync.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=3s CMD curl -f http://localhost:5000/health || exit 1
ENTRYPOINT ["dotnet", "RealSync.Api.dll"]
```

### 2.2 Frontend Dockerfile
```dockerfile
# frontend/Dockerfile
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:alpine AS production
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=3s CMD curl -f http://localhost/ || exit 1
CMD ["nginx", "-g", "daemon off;"]
```

### 2.3 Docker Compose
```yaml
# docker-compose.yml
version: '3.8'

services:
  # SQL Server
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: realsync-db
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=${DB_PASSWORD}
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - realsync-net
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$$MSSQL_SA_PASSWORD" -C -Q "SELECT 1"
      interval: 10s
      timeout: 3s
      retries: 10

  # Redis Cache
  redis:
    image: redis:7-alpine
    container_name: realsync-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - realsync-net
    healthcheck:
      test: redis-cli ping
      interval: 10s
      timeout: 3s

  # Backend API
  api:
    build:
      context: ./backend
      dockerfile: Dockerfile
    container_name: realsync-api
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNET_ENV:-Development}
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - Redis__ConnectionString=redis:6379
      - Jwt__Secret=${JWT_SECRET}
    ports:
      - "5000:5000"
    depends_on:
      sqlserver:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - realsync-net

  # Frontend
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    container_name: realsync-frontend
    ports:
      - "3000:80"
    depends_on:
      - api
    networks:
      - realsync-net

  # Hangfire Worker (optional, có thể chạy cùng API)
  # hangfire-worker:
  #   build:
  #     context: ./backend
  #     dockerfile: Dockerfile.worker
  #   container_name: realsync-worker
  #   depends_on:
  #     - sqlserver
  #     - redis
  #   networks:
  #     - realsync-net

volumes:
  sqlserver-data:
  redis-data:

networks:
  realsync-net:
    driver: bridge
```

---

## 3. Environment Variables

### .env Template
```env
# Database
DB_PASSWORD=YourStr0ngP@ssword!
DB_CONNECTION_STRING=Server=sqlserver;Database=RealSyncDb;User=sa;Password=YourStr0ngP@ssword!;TrustServerCertificate=true

# JWT
JWT_SECRET=your-super-secret-key-at-least-32-chars
JWT_ISSUER=RealSync
JWT_AUDIENCE=RealSyncClient

# Redis
REDIS_CONNECTION=redis:6379

# ASP.NET
ASPNET_ENV=Development

# Frontend
VITE_API_BASE_URL=http://localhost:5000/api/v1

# AI Service
AI_SERVICE_URL=http://localhost:8000
AI_API_KEY=your-ai-api-key

# OpenAI (if using)
OPENAI_API_KEY=sk-...
OPENAI_MODEL=gpt-4o-mini
```

> ⚠ **KHÔNG BAO GIỜ** commit file `.env` vào Git!

---

## 4. Nginx Configuration

```nginx
# devops/nginx/realsync.conf
server {
    listen 80;
    server_name realsync.vn www.realsync.vn;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name realsync.vn www.realsync.vn;

    ssl_certificate /etc/letsencrypt/live/realsync.vn/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/realsync.vn/privkey.pem;

    # Frontend (Vue SPA)
    location / {
        root /var/www/realsync/frontend;
        try_files $uri $uri/ /index.html;
        
        # Cache static assets
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff2?)$ {
            expires 30d;
            add_header Cache-Control "public, immutable";
        }
    }

    # Backend API
    location /api/ {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # SignalR Hub
    location /hubs/ {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_read_timeout 86400;
    }

    # Hangfire Dashboard (admin only)
    location /hangfire {
        proxy_pass http://localhost:5000;
        # Restrict access
        allow 10.0.0.0/8;
        deny all;
    }
}
```

---

## 5. CI/CD Pipeline (GitHub Actions)

### .github/workflows/deploy.yml
```yaml
name: Deploy RealSync

on:
  push:
    branches: [main, develop]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Backend Tests
        run: |
          cd backend
          dotnet restore
          dotnet test --no-restore --verbosity normal
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
      
      - name: Frontend Lint & Build
        run: |
          cd frontend
          npm ci
          npm run lint
          npm run build

  deploy-staging:
    needs: test
    if: github.ref == 'refs/heads/develop'
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to Staging
        uses: appleboy/ssh-action@v1
        with:
          host: ${{ secrets.STAGING_HOST }}
          username: ${{ secrets.SSH_USER }}
          key: ${{ secrets.SSH_KEY }}
          script: |
            cd /opt/realsync
            git pull origin develop
            docker-compose -f docker-compose.staging.yml up -d --build
            echo "Staging deployed successfully"

  deploy-production:
    needs: test
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    environment: production
    steps:
      - name: Deploy to Production
        uses: appleboy/ssh-action@v1
        with:
          host: ${{ secrets.PROD_HOST }}
          username: ${{ secrets.SSH_USER }}
          key: ${{ secrets.SSH_KEY }}
          script: |
            cd /opt/realsync
            git pull origin main
            docker-compose -f docker-compose.prod.yml up -d --build
            echo "Production deployed successfully"
```

---

## 6. Database Migration (Production)

```bash
# 1. Generate migration script
dotnet ef migrations script \
  --idempotent \
  -p backend/src/RealSync.Data \
  -s backend/src/RealSync.Api \
  -o migration.sql

# 2. Review script trước khi chạy

# 3. Backup database
sqlcmd -S server -U sa -P password -Q "BACKUP DATABASE RealSyncDb TO DISK='/backup/RealSyncDb_$(date +%Y%m%d).bak'"

# 4. Apply migration
sqlcmd -S server -U sa -P password -d RealSyncDb -i migration.sql
```

---

## 7. Monitoring & Health Checks

### Health Check Endpoint
```
GET /health          → Basic health
GET /health/ready    → Readiness (DB, Redis connected)
GET /health/live     → Liveness
```

### Logging
```
Backend logs → Serilog → Seq (http://localhost:5341)
Nginx logs   → /var/log/nginx/
Docker logs  → docker logs <container>
```

### Backup Schedule
```
Daily:   Database backup (giữ 7 ngày)
Weekly:  Full backup (giữ 4 tuần)
Monthly: Archive backup (giữ 12 tháng)
```

---

## 8. Deployment Checklist

- [ ] `.env` file configured cho target environment
- [ ] Database migration script reviewed
- [ ] Database backed up
- [ ] All tests passing
- [ ] Docker images built successfully
- [ ] SSL certificate valid
- [ ] Nginx config tested (`nginx -t`)
- [ ] Health checks passing
- [ ] Monitoring alerts configured
- [ ] Rollback plan prepared

---

> Xem thêm: `architecture-guide.md`
