import { useState, useEffect } from "react";
import {
  Row, Col, Button, InputNumber, Tag, Typography, Divider,
  Breadcrumb, Spin, message, Card,
} from "antd";
import {
  ShoppingCartOutlined, ArrowLeftOutlined,
  CheckCircleOutlined, CloseCircleOutlined, ClockCircleOutlined,
} from "@ant-design/icons";
import { useParams, useNavigate, Link } from "react-router-dom";
import productApi from "../../api/productApi";
import useCartStore from "../../store/cartStore";
import useAuthStore from "../../store/authStore";
import "./Product.css";

const { Title, Text, Paragraph } = Typography;

export default function ProductDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { addItem } = useCartStore();
  const { accessToken } = useAuthStore();

  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);
  const [adding, setAdding] = useState(false);

  useEffect(() => {
    const fetchProduct = async () => {
      try {
        setLoading(true);
        const res = await productApi.getById(id);
        setProduct(res.data.data);
      } catch {
        message.error("Không tìm thấy sản phẩm!");
        navigate("/products");
      } finally {
        setLoading(false);
      }
    };
    fetchProduct();
  }, [id]);

  const handleAddToCart = async () => {
    if (!accessToken) {
      message.warning("Vui lòng đăng nhập để thêm vào giỏ hàng!");
      navigate("/login");
      return;
    }
    try {
      setAdding(true);
      await addItem(product.id, quantity);
      message.success(`Đã thêm ${quantity} "${product.name}" vào giỏ hàng!`);
    } catch (err) {
      message.error(err.response?.data?.message || "Thêm vào giỏ thất bại!");
    } finally {
      setAdding(false);
    }
  };

  const formatPrice = (price) =>
    new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(price);

  if (loading) {
    return (
      <div className="product-detail-loading">
        <Spin size="large" />
      </div>
    );
  }

  if (!product) return null;

  return (
    <div className="product-detail-page">
      {/* Breadcrumb */}
      <Breadcrumb
        items={[
          { title: <Link to="/">Trang chủ</Link> },
          { title: <Link to="/products">Sản phẩm</Link> },
          { title: product.name },
        ]}
      />

      <Button
        icon={<ArrowLeftOutlined />}
        onClick={() => navigate("/products")}
        className="product-detail-back"
      >
        Quay lại
      </Button>

      <Row gutter={[48, 32]}>
        {/* ===== ẢNH SẢN PHẨM ===== */}
        <Col xs={24} md={12}>
          <div className="product-detail-image">
            {product.imageUrl ? (
              <img
                src={product.imageUrl}
                alt={product.name}
                onError={(e) => { e.target.src = "https://placehold.co/600x600?text=No+Image"; }}
              />
            ) : (
              <div className="product-detail-no-image">
                <ClockCircleOutlined />
                <br />
                <Text type="secondary">Chưa có hình ảnh</Text>
              </div>
            )}
          </div>
        </Col>

        {/* ===== THÔNG TIN SẢN PHẨM ===== */}
        <Col xs={24} md={12}>
          {/* Danh mục */}
          {product.categoryName && (
            <Tag color="#c9a84c" className="product-detail-category">
              {product.categoryName}
            </Tag>
          )}

          {/* Tên sản phẩm */}
          <Title level={2} className="product-detail-name">
            {product.name}
          </Title>

          {/* Giá */}
          <Title level={3} className="product-detail-price">
            {formatPrice(product.price)}
          </Title>

          {/* Tình trạng */}
          <div className="product-detail-stock">
            {product.stock > 0 ? (
              <Tag icon={<CheckCircleOutlined />} color="success">
                Còn hàng ({product.stock} chiếc)
              </Tag>
            ) : (
              <Tag icon={<CloseCircleOutlined />} color="error">
                Hết hàng
              </Tag>
            )}
          </div>

          <Divider />

          {/* Mô tả */}
          <div className="product-detail-desc">
            <Text strong className="product-detail-desc-label">Mô tả sản phẩm</Text>
            <Paragraph className="product-detail-desc-text">
              {product.description}
            </Paragraph>
          </div>

          <Divider />

          {/* Số lượng + Thêm giỏ hàng */}
          {product.stock > 0 && (
            <div>
              <div className="product-detail-quantity">
                <Text strong>Số lượng:</Text>
                <InputNumber
                  min={1}
                  max={product.stock}
                  value={quantity}
                  onChange={(v) => setQuantity(v)}
                  size="large"
                  style={{ width: 100 }}
                />
                <Text type="secondary">/ {product.stock} còn lại</Text>
              </div>

              <Button
                type="primary"
                size="large"
                icon={<ShoppingCartOutlined />}
                loading={adding}
                onClick={handleAddToCart}
                className="product-detail-add-btn"
              >
                Thêm vào giỏ hàng
              </Button>
            </div>
          )}

          {/* Thông tin thêm */}
          <Card className="product-detail-meta" size="small">
            <Row gutter={16}>
              <Col span={12}>
                <Text type="secondary" className="product-detail-meta-label">Mã sản phẩm</Text>
                <br />
                <Text strong>#{product.id}</Text>
              </Col>
              <Col span={12}>
                <Text type="secondary" className="product-detail-meta-label">Danh mục</Text>
                <br />
                <Text strong>{product.categoryName || "—"}</Text>
              </Col>
            </Row>
          </Card>
        </Col>
      </Row>
    </div>
  );
}