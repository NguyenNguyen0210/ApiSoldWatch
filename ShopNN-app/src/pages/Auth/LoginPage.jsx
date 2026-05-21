import { useState } from "react";
import { Form, Input, Button, Card, Typography, message, Divider } from "antd";
import { UserOutlined, LockOutlined } from "@ant-design/icons";
import { useNavigate, Link } from "react-router-dom";
import authApi from "../../api/authApi";
import useAuthStore from "../../store/authStore";
import "./Auth.css";

const { Title, Text } = Typography;

export default function LoginPage() {
  const navigate = useNavigate();
  const { setAuth, setUser } = useAuthStore();
  const [loading, setLoading] = useState(false);

  const onFinish = async (values) => {
    try {
      setLoading(true);

      // Gọi API signin
      // Response: { success: true, message: "...", data: { accessToken, refreshToken } }
      const res = await authApi.login({
        username: values.username,
        password: values.password,
      });

      const { accessToken, refreshToken } = res.data.data;
      setAuth(accessToken, refreshToken);

      // Lấy thêm thông tin profile
      // Response: { success: true, data: { id, userName, email, roles: [...] } }
      const profileRes = await authApi.getProfile();
      setUser(profileRes.data.data);

      message.success(res.data.message || "Đăng nhập thành công!");
      navigate("/");
    } catch (err) {
      // Backend trả về { success: false, message: "...", errors: [...] }
      const errMsg =
        err.response?.data?.message ||
        err.response?.data?.errors?.[0] ||
        "Sai tên đăng nhập hoặc mật khẩu!";
      message.error(errMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <Card className="auth-card">
        <div className="auth-header">
          <Title level={2}>🕐 ShopNN</Title>
          <Text type="secondary">Đăng nhập để tiếp tục mua sắm</Text>
        </div>

        <Form layout="vertical" onFinish={onFinish} size="large">
          <Form.Item
            label="Username"
            name="username"
            rules={[{ required: true, message: "Vui lòng nhập tên đăng nhập!" }]}
          >
            <Input
              prefix={<UserOutlined className="auth-icon-prefix" />}
              placeholder="Nhập tên đăng nhập"
            />
          </Form.Item>

          <Form.Item
            label="Mật khẩu"
            name="password"
            rules={[{ required: true, message: "Vui lòng nhập mật khẩu!" }]}
          >
            <Input.Password
              prefix={<LockOutlined className="auth-icon-prefix" />}
              placeholder="Nhập mật khẩu"
            />
          </Form.Item>

          <Form.Item>
            <Button
              type="primary"
              htmlType="submit"
              block
              loading={loading}
              className="auth-submit-btn"
            >
              Đăng nhập
            </Button>
          </Form.Item>
        </Form>

        <Divider />

        <div className="auth-footer">
          <Text type="secondary">Chưa có tài khoản? </Text>
          <Link to="/register">Đăng ký ngay</Link>
        </div>
      </Card>
    </div>
  );
}