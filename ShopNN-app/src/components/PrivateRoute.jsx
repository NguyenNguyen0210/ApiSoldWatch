import { Navigate } from "react-router-dom";
import useAuthStore from "../store/authStore";

// Bảo vệ route cần đăng nhập
export function PrivateRoute({ children }) {
  const accessToken = useAuthStore((s) => s.accessToken);
  return accessToken ? children : <Navigate to="/login" replace />;
}

// Bảo vệ route chỉ dành cho Admin
export function AdminRoute({ children }) {
  const { accessToken, isAdmin } = useAuthStore();
  if (!accessToken) return <Navigate to="/login" replace />;
  if (!isAdmin()) return <Navigate to="/" replace />;
  return children;
}

// Chỉ dành cho khách chưa đăng nhập (ngăn truy cập login/register khi đã đăng nhập)
export function GuestRoute({ children }) {
  const { accessToken, isAdmin } = useAuthStore();
  if (accessToken) {
    return <Navigate to={isAdmin() ? "/admin" : "/"} replace />;
  }
  return children;
}