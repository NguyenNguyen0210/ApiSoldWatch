import { Layout, Button, Avatar, Dropdown, Typography } from "antd";
import {
  LogoutOutlined,
  DashboardOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { useNavigate, Outlet, Link } from "react-router-dom";
import useAuthStore from "../store/authStore";
import ThemeSelector from "../components/ThemeSelector";
import "./AdminLayout.css";

const { Header, Content } = Layout;
const { Text } = Typography;

export default function AdminLayout() {
  const navigate = useNavigate();
  const { user, logout } = useAuthStore();

  const handleLogout = async () => {
    await logout();
    navigate("/login");
  };

  const userMenuItems = [
    {
      key: "logout",
      icon: <LogoutOutlined />,
      label: "Đăng xuất",
      danger: true,
      onClick: handleLogout,
    },
  ];

  return (
    <Layout className="admin-layout-container">
      {/* ===== ADMIN HEADER ===== */}
      <Header className="admin-header-bar">
        {/* Logo */}
        <Link to="/admin" className="admin-header-logo">
          <DashboardOutlined className="admin-logo-icon" />
          <span className="admin-logo-text">
            SHOP<span className="admin-logo-accent">NN</span> <span className="admin-logo-badge">ADMIN</span>
          </span>
        </Link>

        {/* Right side actions */}
        <div className="admin-header-actions">
          <ThemeSelector />

          {/* User dropdown */}
          <Dropdown menu={{ items: userMenuItems }} placement="bottomRight">
            <div className="admin-user-trigger">
              <Avatar
                className="admin-avatar"
                src = "https://img.magnific.com/vecteurs-libre/homme-affaires-caractere-avatar-isole_24877-60111.jpg?w=800"
                // icon={<UserOutlined />}
              />
              <Text className="admin-username">
                {user?.userName || "Admin"}
              </Text>
            </div>
          </Dropdown>
        </div>
      </Header>

      {/* ===== ADMIN CONTENT ===== */}
      <Content className="admin-layout-content">
        <Outlet />
      </Content>
    </Layout>
  );
}
