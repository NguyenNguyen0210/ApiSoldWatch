import { useState } from "react";
import { Form, Input, Button, Card, Typography, message, Divider } from "antd";
import { UserOutlined, LockOutlined, MailOutlined } from "@ant-design/icons";
import { useNavigate, Link } from "react-router-dom";
import authApi from "../../api/authApi";
import "./Auth.css";

const { Title, Text } = Typography;

export default function RegisterPage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  const onFinish = async (values) => {
    try {
      setLoading(true);

      // Gọi API signup
      // Body: { username, password, email }
      // Response: { success: true, message: "Sign up success", data: null }
      const res = await authApi.register({
        username: values.username,
        email: values.email,
        password: values.password,
      });

      message.success(res.data.message || "Đăng ký thành công!");
      navigate("/login");
    } catch (err) {
      const errData = err.response?.data;
      // Backend có thể trả nhiều lỗi trong errors[]
      if (errData?.errors?.length > 0) {
        errData.errors.forEach((e) => message.error(e));
      } else {
        message.error(errData?.message || "Đăng ký thất bại! Vui lòng thử lại.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <Card className="auth-card">
        <div className="auth-header">
          <Title level={2}>🕐 ShopNN</Title>
          <Text type="secondary">Tạo tài khoản mới miễn phí</Text>
        </div>

        <Form layout="vertical" onFinish={onFinish} size="large">
          <Form.Item
            label="Tên đăng nhập"
            name="username"
            rules={[
              { required: true, message: "Vui lòng nhập tên đăng nhập!" },
              { min: 3, message: "Tên đăng nhập tối thiểu 3 ký tự!" },
            ]}
          >
            <Input
              prefix={<UserOutlined className="auth-icon-prefix" />}
              placeholder="Nhập tên đăng nhập"
            />
          </Form.Item>

          <Form.Item
            label="Email"
            name="email"
            rules={[
              { required: true, message: "Vui lòng nhập email!" },
              { type: "email", message: "Email không hợp lệ!" },
            ]}
          >
            <Input
              prefix={<MailOutlined className="auth-icon-prefix" />}
              placeholder="Nhập email"
            />
          </Form.Item>

          <Form.Item
            label="Mật khẩu"
            name="password"
            rules={[
              { required: true, message: "Vui lòng nhập mật khẩu!" },
              { min: 6, message: "Mật khẩu tối thiểu 6 ký tự!" },
              {
                // Password policy: chữ hoa + chữ thường + số + ký tự đặc biệt
                pattern: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$/,
                message:
                  "Mật khẩu phải có chữ hoa, chữ thường, số và ký tự đặc biệt!",
              },
            ]}
          >
            <Input.Password
              prefix={<LockOutlined className="auth-icon-prefix" />}
              placeholder="Ví dụ: Admin@123"
            />
          </Form.Item>

          <Form.Item
            label="Xác nhận mật khẩu"
            name="confirmPassword"
            dependencies={["password"]}
            rules={[
              { required: true, message: "Vui lòng xác nhận mật khẩu!" },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue("password") === value)
                    return Promise.resolve();
                  return Promise.reject(new Error("Mật khẩu không khớp!"));
                },
              }),
            ]}
          >
            <Input.Password
              prefix={<LockOutlined className="auth-icon-prefix" />}
              placeholder="Nhập lại mật khẩu"
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
              Đăng ký
            </Button>
          </Form.Item>
        </Form>

        <Divider />

        <div className="auth-footer">
          <Text type="secondary">Đã có tài khoản? </Text>
          <Link to="/login">Đăng nhập</Link>
        </div>
      </Card>
    </div>
  );
}