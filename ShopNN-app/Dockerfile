# Stage 1: Build ứng dụng React với Node.js
FROM node:20-alpine AS builder
WORKDIR /app

# Sao chép file package.json và package-lock.json để tận dụng cache của Docker
COPY package*.json ./
RUN npm install

# Sao chép toàn bộ mã nguồn và thực hiện build ứng dụng React
COPY . .
RUN npm run build

# Stage 2: Chạy ứng dụng đã build bằng Web Server Nginx siêu nhẹ
FROM nginx:alpine

# Copy file cấu hình Nginx tối ưu cho React Router (SPA)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy thư mục build 'dist' từ Stage 1 vào thư mục phục vụ web của Nginx
COPY --from=builder /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
