import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { ConfigProvider, theme } from "antd";
import { useEffect } from "react";
import useThemeStore from "./store/themeStore";
import MainLayout from "./layouts/MainLayout";
import AdminLayout from "./layouts/AdminLayout";
import LoginPage from "./pages/Auth/LoginPage";
import RegisterPage from "./pages/Auth/RegisterPage";
import HomePage from "./pages/Home/HomePage";
import ProductListPage from "./pages/Product/ProductListPage";
import ProductDetailPage from "./pages/Product/ProductDetailPage";
import CartPage from "./pages/Cart/CartPage";
import CheckoutPage from "./pages/Order/CheckoutPage";
import OrderHistoryPage from "./pages/Order/OrderHistoryPage";
import AdminDashboard from "./pages/Admin/AdminDashboard";
import { PrivateRoute, AdminRoute, GuestRoute } from "./components/PrivateRoute";

const getAntTheme = (activeTheme) => {
  const isLight = activeTheme === "light";
  
  const tokens = {
    colorPrimary: isLight ? "#b3923b" : "#c9a84c",
    borderRadius: 8,
    fontFamily: "'Jost', sans-serif",
  };

  if (isLight) {
    return {
      algorithm: theme.defaultAlgorithm,
      token: {
        ...tokens,
        colorBgContainer: "#ffffff",
        colorBgLayout: "#fdfcf7",
        colorText: "#1a1a1a",
      },
    };
  }

  // Dark theme
  return {
    algorithm: theme.darkAlgorithm,
    token: {
      ...tokens,
      colorBgContainer: "#1c1c1c",
      colorBgLayout: "#121212",
      colorText: "#ffffff",
    },
  };
};

export default function App() {
  const { activeTheme, initTheme } = useThemeStore();

  useEffect(() => {
    initTheme();
  }, [activeTheme, initTheme]);

  return (
    <ConfigProvider theme={getAntTheme(activeTheme)}>
      <BrowserRouter>
      <Routes>
        {/* ===== Auth (không có layout) ===== */}
        <Route
          path="/login"
          element={
            <GuestRoute>
              <LoginPage />
            </GuestRoute>
          }
        />
        <Route
          path="/register"
          element={
            <GuestRoute>
              <RegisterPage />
            </GuestRoute>
          }
        />

        {/* ===== Routes có MainLayout (Navbar + Footer) ===== */}
        <Route element={<MainLayout />}>
          {/* Public */}
          <Route path="/" element={<HomePage />} />
          <Route path="/products" element={<ProductListPage />} />
          <Route path="/products/:id" element={<ProductDetailPage />} />

          {/* User (cần đăng nhập) */}
          <Route
            path="/cart"
            element={
              <PrivateRoute>
                <CartPage />
              </PrivateRoute>
            }
          />
          <Route
            path="/checkout"
            element={
              <PrivateRoute>
                <CheckoutPage />
              </PrivateRoute>
            }
          />
          <Route
            path="/orders"
            element={
              <PrivateRoute>
                <OrderHistoryPage />
              </PrivateRoute>
            }
          />
        </Route>

        {/* ===== Routes có AdminLayout (Quản trị hệ thống) ===== */}
        <Route element={<AdminLayout />}>
          <Route
            path="/admin"
            element={
              <AdminRoute>
                <AdminDashboard />
              </AdminRoute>
            }
          />
        </Route>

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
      </BrowserRouter>
    </ConfigProvider>
  );
}