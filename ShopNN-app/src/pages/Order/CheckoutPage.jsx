import { useEffect, useState } from "react";
import {
  Typography, Button, Card, Spin, Breadcrumb, message, Result, Form, Input,
} from "antd";
import {
  ArrowLeftOutlined, CreditCardOutlined,
  DollarOutlined, CheckCircleOutlined,
} from "@ant-design/icons";
import { useNavigate, Link } from "react-router-dom";
import useCartStore from "../../store/cartStore";
import orderApi from "../../api/orderApi";
import "./Order.css";

const { Title, Text } = Typography;

const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(price);

const PAYMENT_METHODS = [
  {
    key: "COD",
    name: "Thanh toán khi nhận hàng (COD)",
    desc: "Bạn sẽ thanh toán bằng tiền mặt khi nhận được hàng",
    icon: <DollarOutlined />,
  },
  {
    key: "VnPay",
    name: "Thanh toán qua VnPay",
    desc: "Chuyển hướng đến VnPay để thanh toán online an toàn",
    icon: <CreditCardOutlined />,
  },
];

export default function CheckoutPage() {
  const navigate = useNavigate();
  const [form] = Form.useForm();
  const { cart, loading: cartLoading, fetchCart } = useCartStore();
  const [paymentMethod, setPaymentMethod] = useState("COD");
  const [submitting, setSubmitting] = useState(false);
  const [orderSuccess, setOrderSuccess] = useState(null);

  useEffect(() => {
    fetchCart();
  }, []);

  const items = cart?.items || [];
  const totalAmount = items.reduce(
    (sum, item) => sum + item.productPrice * item.quantity,
    0
  );

  const handleCheckout = async (values) => {
    if (items.length === 0) {
      message.warning("Giỏ hàng trống!");
      return;
    }

    try {
      setSubmitting(true);
      const res = await orderApi.checkout({
        paymentMethod,
        receiverName: values.receiverName,
        phoneNumber: values.phoneNumber,
        shippingAddress: values.shippingAddress,
      });
      const order = res.data.data;

      // If VnPay, redirect to payment URL
      if (paymentMethod === "VnPay" && order.paymentUrl) {
        window.location.href = order.paymentUrl;
        return;
      }

      // COD success
      setOrderSuccess(order);
      // Refresh cart (should be empty now)
      await fetchCart();
    } catch (err) {
      const errMsg =
        err.response?.data?.message ||
        err.response?.data?.errors?.[0] ||
        "Đặt hàng thất bại! Vui lòng thử lại.";
      message.error(errMsg);
    } finally {
      setSubmitting(false);
    }
  };

  // Loading state
  if (cartLoading) {
    return (
      <div className="checkout-loading">
        <Spin size="large" />
      </div>
    );
  }

  // Success state
  if (orderSuccess) {
    return (
      <div className="checkout-page">
        <Result
          status="success"
          icon={<CheckCircleOutlined style={{ color: "#c9a84c" }} />}
          title="Đặt hàng thành công!"
          subTitle={`Mã đơn hàng: #${orderSuccess.id?.substring(0, 8).toUpperCase()}`}
          extra={[
            <Button
              key="orders"
              type="primary"
              onClick={() => navigate("/orders")}
              style={{
                background: "#0a0a0a",
                borderColor: "#0a0a0a",
                borderRadius: 8,
                height: 46,
              }}
            >
              Xem đơn hàng
            </Button>,
            <Button
              key="home"
              onClick={() => navigate("/")}
              style={{ borderRadius: 8, height: 46 }}
            >
              Về trang chủ
            </Button>,
          ]}
        />
      </div>
    );
  }

  // Empty cart
  if (items.length === 0) {
    return (
      <div className="checkout-page">
        <Result
          title="Giỏ hàng trống"
          subTitle="Vui lòng thêm sản phẩm vào giỏ trước khi thanh toán"
          extra={
            <Button
              type="primary"
              onClick={() => navigate("/products")}
              style={{
                background: "#c9a84c",
                borderColor: "#c9a84c",
                color: "#000",
                borderRadius: 8,
                height: 46,
              }}
            >
              Mua sắm ngay
            </Button>
          }
        />
      </div>
    );
  }

  return (
    <div className="checkout-page">
      <Breadcrumb
        items={[
          { title: <Link to="/">Trang chủ</Link> },
          { title: <Link to="/cart">Giỏ hàng</Link> },
          { title: "Thanh toán" },
        ]}
      />

      <Title level={2} className="checkout-title">
        Thanh toán đơn hàng
      </Title>

      <Form
        form={form}
        layout="vertical"
        onFinish={handleCheckout}
        requiredMark="optional"
      >
        {/* Order Summary */}
        <Card title="Sản phẩm đặt mua" className="checkout-summary-card">
          <div className="checkout-items">
            {items.map((item) => (
              <div key={item.id} className="checkout-item">
                <span className="checkout-item-name">{item.productName}</span>
                <span className="checkout-item-qty">x{item.quantity}</span>
                <span className="checkout-item-price">
                  {formatPrice(item.productPrice * item.quantity)}
                </span>
              </div>
            ))}
          </div>
          <div className="checkout-total-row">
            <span className="checkout-total-label">Tổng cộng</span>
            <span className="checkout-total-value">{formatPrice(totalAmount)}</span>
          </div>
        </Card>

        {/* Shipping Information */}
        <Card title="Thông tin giao hàng" className="checkout-summary-card">
          <Form.Item
            label="Họ và tên người nhận"
            name="receiverName"
            rules={[{ required: true, message: "Vui lòng nhập họ tên người nhận!" }]}
          >
            <Input placeholder="Ví dụ: Nguyễn Văn A" size="large" style={{ borderRadius: 8 }} />
          </Form.Item>

          <Form.Item
            label="Số điện thoại nhận hàng"
            name="phoneNumber"
            rules={[
              { required: true, message: "Vui lòng nhập số điện thoại!" },
              { pattern: /^[0-9]{10,11}$/, message: "Số điện thoại nhận hàng không hợp lệ (10-11 chữ số)!" }
            ]}
          >
            <Input placeholder="Ví dụ: 0987654321" size="large" style={{ borderRadius: 8 }} />
          </Form.Item>

          <Form.Item
            label="Địa chỉ giao hàng"
            name="shippingAddress"
            rules={[{ required: true, message: "Vui lòng nhập địa chỉ giao hàng!" }]}
          >
            <Input.TextArea
              placeholder="Nhập số nhà, tên đường, phường/xã, quận/huyện, tỉnh/thành phố"
              rows={3}
              size="large"
              style={{ borderRadius: 8 }}
            />
          </Form.Item>
        </Card>

        {/* Payment Method */}
        <Card className="checkout-payment-card">
          <Title level={4} className="checkout-payment-title">
            Phương thức thanh toán
          </Title>

          {PAYMENT_METHODS.map((method) => (
            <div
              key={method.key}
              className={`payment-option ${paymentMethod === method.key ? "selected" : ""}`}
              onClick={() => setPaymentMethod(method.key)}
            >
              <div className="payment-option-header">
                <span className="payment-option-icon">{method.icon}</span>
                <span className="payment-option-name">{method.name}</span>
              </div>
              <div className="payment-option-desc">{method.desc}</div>
            </div>
          ))}
        </Card>

        {/* Actions */}
        <div className="checkout-actions">
          <Button
            icon={<ArrowLeftOutlined />}
            className="checkout-back-btn"
            onClick={() => navigate("/cart")}
          >
            Quay lại giỏ hàng
          </Button>
          <Button
            type="primary"
            htmlType="submit"
            className="checkout-submit-btn"
            loading={submitting}
          >
            {paymentMethod === "VnPay"
              ? `Thanh toán ${formatPrice(totalAmount)}`
              : `Đặt hàng ${formatPrice(totalAmount)}`}
          </Button>
        </div>
      </Form>
    </div>
  );
}
