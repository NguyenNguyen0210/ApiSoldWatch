import { useEffect, useState } from "react";
import {
  Typography, Button, Card, Tag, Spin, Breadcrumb, message,
} from "antd";
import {
  ShoppingOutlined, ClockCircleOutlined,
} from "@ant-design/icons";
import { useNavigate, Link, useSearchParams } from "react-router-dom";
import orderApi from "../../api/orderApi";
import useCartStore from "../../store/cartStore";
import "./Order.css";

const { Title, Text } = Typography;

const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(price);

const formatDate = (dateStr) => {
  const date = new Date(dateStr);
  return date.toLocaleDateString("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
};

const STATUS_COLOR = {
  Pending: "orange",
  Processing: "blue",
  Shipped: "cyan",
  Delivered: "green",
  Cancelled: "red",
};

const STATUS_LABEL = {
  Pending: "Chờ xử lý",
  Processing: "Đang xử lý",
  Shipped: "Đang giao",
  Delivered: "Đã giao",
  Cancelled: "Đã hủy",
};

const PAYMENT_STATUS_COLOR = {
  Unpaid: "default",
  Paid: "success",
  Failed: "error",
  Refunded: "warning",
};

const PAYMENT_STATUS_LABEL = {
  Unpaid: "Chưa thanh toán",
  Paid: "Đã thanh toán",
  Failed: "Thanh toán lỗi",
  Refunded: "Đã hoàn tiền",
};

export default function OrderHistoryPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { fetchCart } = useCartStore();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const payment = searchParams.get("payment");
    const orderId = searchParams.get("orderId");
    if (payment === "success") {
      message.success(`Thanh toán đơn hàng #${orderId?.substring(0, 8).toUpperCase()} thành công qua VNPay!`);
      fetchCart(); // Clear cart badge immediately
      setSearchParams({}, { replace: true });
    } else if (payment === "fail") {
      message.error(`Thanh toán đơn hàng #${orderId?.substring(0, 8).toUpperCase()} thất bại!`);
      setSearchParams({}, { replace: true });
    }
  }, [searchParams]);

  useEffect(() => {
    const fetchOrders = async () => {
      try {
        const res = await orderApi.getMyOrders();
        setOrders(res.data.data || []);
      } catch {
        setOrders([]);
      } finally {
        setLoading(false);
      }
    };
    fetchOrders();
  }, []);

  if (loading) {
    return (
      <div className="orders-loading">
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div className="orders-page">
      <Breadcrumb
        items={[
          { title: <Link to="/">Trang chủ</Link> },
          { title: "Đơn hàng" },
        ]}
      />

      <Title level={2} className="orders-title">
        <ClockCircleOutlined /> Đơn hàng của tôi
      </Title>

      {orders.length === 0 ? (
        <div className="orders-empty">
          <ShoppingOutlined className="orders-empty-icon" />
          <Title level={3} style={{ color: "#666" }}>
            Chưa có đơn hàng nào
          </Title>
          <Text type="secondary">
            Bạn chưa đặt đơn hàng nào
          </Text>
          <br /><br />
          <Button
            type="primary"
            onClick={() => navigate("/products")}
            style={{
              background: "#c9a84c",
              borderColor: "#c9a84c",
              color: "#000",
              fontWeight: 600,
              height: 46,
              borderRadius: 8,
            }}
          >
            Mua sắm ngay
          </Button>
        </div>
      ) : (
        orders.map((order) => (
          <Card key={order.id} className="order-card">
            {/* Header */}
            <div className="order-card-header">
              <div>
                <span className="order-id">
                  Đơn #{order.id?.substring(0, 8).toUpperCase()}
                </span>
                <span className="order-date"> — {formatDate(order.createdAt)}</span>
              </div>
              <div>
                <Tag color={STATUS_COLOR[order.status] || "default"}>
                  {STATUS_LABEL[order.status] || order.status}
                </Tag>
              </div>
            </div>

            {/* Items */}
            <div className="order-items-list">
              {order.items?.map((item, i) => (
                <div key={i} className="order-item-row">
                  <span className="order-item-name">{item.productName || `SP #${item.productId}`}</span>
                  <span className="order-item-qty">x{item.quantity}</span>
                  <span className="order-item-price">
                    {formatPrice(item.unitPrice * item.quantity)}
                  </span>
                </div>
              ))}
            </div>

            {/* Footer */}
            <div className="order-card-footer">
              <div className="order-payment-info">
                <Tag>{order.paymentMethod === "VnPay" ? "VnPay" : "COD"}</Tag>
                <Tag color={PAYMENT_STATUS_COLOR[order.paymentStatus] || "default"}>
                  {PAYMENT_STATUS_LABEL[order.paymentStatus] || order.paymentStatus}
                </Tag>
              </div>
              <span className="order-total">{formatPrice(order.totalAmount)}</span>
            </div>
          </Card>
        ))
      )}
    </div>
  );
}
