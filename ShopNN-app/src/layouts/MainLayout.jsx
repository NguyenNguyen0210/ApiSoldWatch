import { useEffect } from "react";
import { Layout, Menu, Badge, Button, Avatar, Dropdown, Typography } from "antd";
import {
  ShoppingCartOutlined,
  UserOutlined,
  LogoutOutlined,
  DashboardOutlined,
  ClockCircleOutlined,
} from "@ant-design/icons";
import { useNavigate, useLocation, Outlet, Link } from "react-router-dom";
import useAuthStore from "../store/authStore";
import useCartStore from "../store/cartStore";
import ThemeSelector from "../components/ThemeSelector";
import "./MainLayout.css";

const { Header, Content, Footer } = Layout;
const { Text } = Typography;

export default function MainLayout() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, accessToken, logout, isAdmin } = useAuthStore();
  const { fetchCart, cartCount } = useCartStore();

  useEffect(() => {
    if (accessToken) {
      if (isAdmin()) {
        navigate("/admin", { replace: true });
      } else {
        fetchCart();
      }
    }
  }, [accessToken, user]);

  const handleLogout = async () => {
    await logout();
    navigate("/login");
  };

  const userMenuItems = [
    {
      key: "orders",
      icon: <ClockCircleOutlined />,
      label: "Đơn hàng của tôi",
      onClick: () => navigate("/orders"),
    },
    { type: "divider" },
    {
      key: "logout",
      icon: <LogoutOutlined />,
      label: "Đăng xuất",
      danger: true,
      onClick: handleLogout,
    },
  ];

  const navItems = [
    { key: "/", label: <Link to="/">Trang chủ</Link> },
    { key: "/products", label: <Link to="/products">Sản phẩm</Link> }
  ];

  return (
    <Layout className="main-layout">
      {/* ===== NAVBAR ===== */}
      <Header className="main-header">
        {/* Logo */}
        <Link to="/" className="header-logo">
          <ClockCircleOutlined className="header-logo-icon" />
          <span className="header-logo-text">
            SHOP<span className="header-logo-accent">NN</span>
          </span>
        </Link>

        {/* Nav Menu */}
        <Menu
          mode="horizontal"
          selectedKeys={[location.pathname]}
          items={navItems}
          className="header-nav"
        />

        {/* Right side */}
        <div className="header-actions">
          <ThemeSelector />

          {accessToken ? (
            <>
              {/* Cart */}
              <Badge count={cartCount()} size="small" overflowCount={10} color="var(--primary-color)">
                <Button
                  type="text"
                  icon={<ShoppingCartOutlined className="header-cart-icon" />}
                  onClick={() => navigate("/cart")}
                  className="header-cart-btn"
                />
              </Badge>

              {/* User dropdown */}
              <Dropdown menu={{ items: userMenuItems }} placement="bottomRight">
                <div className="header-user-trigger">
                  <Avatar
                    className="header-avatar"
                    icon={<UserOutlined />}
                  />
                  <Text className="header-username">
                    {user?.userName || "User"}
                  </Text>
                </div>
              </Dropdown>
            </>
          ) : (
            <div className="header-auth-buttons">
              <Button onClick={() => navigate("/login")} className="header-login-btn">
                Đăng nhập
              </Button>
              <Button
                type="primary"
                onClick={() => navigate("/register")}
                className="header-register-btn"
              >
                Đăng ký
              </Button>
            </div>
          )}
        </div>
      </Header>

      {/* ===== CONTENT ===== */}
      <Content className="main-content">
        <Outlet />
      </Content>

      {/* ===== FOOTER ===== */}
      <Footer className="main-footer">
        © {new Date().getFullYear()} <span className="footer-brand">ShopNN</span> — Cửa hàng đồng hồ cao cấp
      </Footer>
    </Layout>
  );
}