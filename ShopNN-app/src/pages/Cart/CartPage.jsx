import { useEffect, useState } from "react";
import {
  Typography, Button, Spin, Breadcrumb, Card, message, Popconfirm,
} from "antd";
import {
  ShoppingCartOutlined, DeleteOutlined, MinusOutlined, PlusOutlined,
  ArrowLeftOutlined, ArrowRightOutlined, ClockCircleOutlined,
  ShoppingOutlined,
} from "@ant-design/icons";
import { useNavigate, Link } from "react-router-dom";
import useCartStore from "../../store/cartStore";
import "./Cart.css";

const { Title, Text } = Typography;

const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(price);

export default function CartPage() {
  const navigate = useNavigate();
  const { cart, loading, fetchCart, updateItem, removeItem, clearCart } = useCartStore();
  const [updatingId, setUpdatingId] = useState(null);
  const [removingId, setRemovingId] = useState(null);
  const [clearing, setClearing] = useState(false);

  useEffect(() => {
    fetchCart();
  }, []);

  const items = cart?.items || [];

  const totalAmount = items.reduce(
    (sum, item) => sum + item.productPrice * item.quantity,
    0
  );

  const totalItems = items.reduce((sum, item) => sum + item.quantity, 0);

  const handleUpdateQuantity = async (itemId, newQuantity) => {
    if (newQuantity < 1) return;
    try {
      setUpdatingId(itemId);
      await updateItem(itemId, newQuantity);
    } catch (err) {
      message.error(err.response?.data?.message || "Không thể cập nhật số lượng!");
    } finally {
      setUpdatingId(null);
    }
  };

  const handleRemoveItem = async (itemId, productName) => {
    try {
      setRemovingId(itemId);
      await removeItem(itemId);
      message.success(`Đã xóa "${productName}" khỏi giỏ hàng`);
    } catch (err) {
      message.error(err.response?.data?.message || "Không thể xóa sản phẩm!");
    } finally {
      setRemovingId(null);
    }
  };

  const handleClearCart = async () => {
    try {
      setClearing(true);
      await clearCart();
      message.success("Đã xóa toàn bộ giỏ hàng");
    } catch (err) {
      message.error(err.response?.data?.message || "Không thể xóa giỏ hàng!");
    } finally {
      setClearing(false);
    }
  };

  if (loading) {
    return (
      <div className="cart-loading">
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div className="cart-page">
      <Breadcrumb
        items={[
          { title: <Link to="/">Trang chủ</Link> },
          { title: "Giỏ hàng" },
        ]}
      />

      <Title level={2} className="cart-title">
        <ShoppingCartOutlined /> Giỏ hàng của bạn
      </Title>

      {/* Empty Cart */}
      {items.length === 0 ? (
        <div className="cart-empty">
          <ShoppingCartOutlined className="cart-empty-icon" />
          <Title level={3} className="cart-empty-title">
            Giỏ hàng trống
          </Title>
          <Text type="secondary">
            Bạn chưa thêm sản phẩm nào vào giỏ hàng
          </Text>
          <br />
          <Button
            className="cart-empty-btn"
            icon={<ShoppingOutlined />}
            onClick={() => navigate("/products")}
          >
            Tiếp tục mua sắm
          </Button>
        </div>
      ) : (
        <>
          {/* Cart Header */}
          <div className="cart-header-row">
            <Text className="cart-item-count">
              {totalItems} sản phẩm trong giỏ
            </Text>
            <Popconfirm
              title="Xóa toàn bộ giỏ hàng?"
              description="Bạn có chắc muốn xóa tất cả sản phẩm?"
              onConfirm={handleClearCart}
              okText="Xóa hết"
              cancelText="Hủy"
              okButtonProps={{ danger: true }}
            >
              <Button
                type="text"
                icon={<DeleteOutlined />}
                loading={clearing}
                className="cart-clear-btn"
              >
                Xóa tất cả
              </Button>
            </Popconfirm>
          </div>

          {/* Cart Items */}
          <div className="cart-items">
            {items.map((item) => (
              <div key={item.id} className="cart-item">
                {/* Image */}
                <div
                  className="cart-item-image"
                  onClick={() => navigate(`/products/${item.productId}`)}
                >
                  {item.productImageUrl ? (
                    <img
                      src={item.productImageUrl}
                      alt={item.productName}
                      onError={(e) => { e.target.src = "https://placehold.co/100x100?text=ShopNN"; }}
                    />
                  ) : (
                    <ClockCircleOutlined className="cart-item-image-placeholder" />
                  )}
                </div>

                {/* Info */}
                <div className="cart-item-info">
                  <div
                    className="cart-item-name"
                    onClick={() => navigate(`/products/${item.productId}`)}
                  >
                    {item.productName}
                  </div>
                  <div className="cart-item-unit-price">
                    {formatPrice(item.productPrice)}
                  </div>
                </div>

                {/* Quantity Controls */}
                <div className="cart-item-quantity">
                  <Button
                    className="cart-item-qty-btn"
                    icon={<MinusOutlined />}
                    disabled={item.quantity <= 1 || updatingId === item.id}
                    onClick={() => handleUpdateQuantity(item.id, item.quantity - 1)}
                  />
                  <span className="cart-item-qty-value">
                    {updatingId === item.id ? <Spin size="small" /> : item.quantity}
                  </span>
                  <Button
                    className="cart-item-qty-btn"
                    icon={<PlusOutlined />}
                    disabled={updatingId === item.id}
                    onClick={() => handleUpdateQuantity(item.id, item.quantity + 1)}
                  />
                </div>

                {/* Subtotal */}
                <div className="cart-item-subtotal">
                  {formatPrice(item.productPrice * item.quantity)}
                </div>

                {/* Remove */}
                <Button
                  type="text"
                  icon={<DeleteOutlined />}
                  className="cart-item-remove"
                  loading={removingId === item.id}
                  onClick={() => handleRemoveItem(item.id, item.productName)}
                />
              </div>
            ))}
          </div>

          {/* Cart Summary */}
          <Card className="cart-summary">
            <div className="cart-summary-row">
              <span className="cart-summary-label">Tạm tính ({totalItems} sản phẩm)</span>
              <span className="cart-summary-value">{formatPrice(totalAmount)}</span>
            </div>
            <div className="cart-summary-row">
              <span className="cart-summary-label">Phí vận chuyển</span>
              <span className="cart-summary-value">
                {totalAmount >= 5000000 ? "Miễn phí" : "Tính khi thanh toán"}
              </span>
            </div>
            <div className="cart-summary-row total">
              <span className="cart-summary-total-label">Tổng cộng</span>
              <span className="cart-summary-total-value">{formatPrice(totalAmount)}</span>
            </div>
          </Card>

          {/* Actions */}
          <div className="cart-actions">
            <Button
              icon={<ArrowLeftOutlined />}
              className="cart-continue-btn"
              onClick={() => navigate("/products")}
            >
              Tiếp tục mua sắm
            </Button>
            <Button
              className="cart-checkout-btn"
              icon={<ArrowRightOutlined />}
              onClick={() => navigate("/checkout")}
            >
              Tiến hành thanh toán
            </Button>
          </div>
        </>
      )}
    </div>
  );
}
