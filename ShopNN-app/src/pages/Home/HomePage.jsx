import { useEffect, useState } from "react";
import { Button, Row, Col, Typography, Spin } from "antd";
import {
  ArrowRightOutlined,
  ClockCircleOutlined,
  SafetyCertificateOutlined,
  CarOutlined,
  CustomerServiceOutlined,
} from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import productApi from "../../api/productApi";
import categoryApi from "../../api/categoryApi";
import "./HomePage.css";

const { Title } = Typography;

const formatPrice = (price) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(price);

const WHY_US = [
  { icon: <SafetyCertificateOutlined />, title: "Chính hãng 100%", desc: "Tất cả sản phẩm đều có chứng nhận xuất xứ và bảo hành chính hãng từ nhà sản xuất." },
  { icon: <CarOutlined />, title: "Giao hàng toàn quốc", desc: "Miễn phí vận chuyển cho đơn từ 5 triệu. Đóng gói cao cấp, bảo mật tuyệt đối." },
  { icon: <CustomerServiceOutlined />, title: "Hỗ trợ 24/7", desc: "Đội ngũ tư vấn chuyên nghiệp sẵn sàng hỗ trợ bạn mọi lúc, mọi nơi." },
  { icon: <ClockCircleOutlined />, title: "Bảo hành 2 năm", desc: "Cam kết bảo hành chính hãng 2 năm cho tất cả sản phẩm tại ShopNN." },
];

const FEATURED_LIMIT = 8;

export default function HomePage() {
  const navigate = useNavigate();
  const [featuredProducts, setFeaturedProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [totalProducts, setTotalProducts] = useState(0);
  const [categoryMeta, setCategoryMeta] = useState({});
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [prodRes, allProdRes, catRes] = await Promise.all([
          productApi.search({ Page: 1, PageSize: FEATURED_LIMIT, SortBy: "name", SortOrder: "asc" }),
          productApi.getAll(),
          categoryApi.getAll(),
        ]);
        const pagedData = prodRes.data.data;
        setFeaturedProducts(pagedData?.items || []);
        setTotalProducts(pagedData?.totalCount || 0);
        setCategories(catRes.data.data || []);

        // Build category metadata: first product image + product count per category
        const allProducts = allProdRes.data.data || [];
        const meta = {};
        allProducts.forEach((p) => {
          if (p.categoryId != null) {
            if (!meta[p.categoryId]) {
              meta[p.categoryId] = { imageUrl: p.imageUrl || null, count: 0 };
            }
            meta[p.categoryId].count += 1;
            // Use the first product that has an image
            if (!meta[p.categoryId].imageUrl && p.imageUrl) {
              meta[p.categoryId].imageUrl = p.imageUrl;
            }
          }
        });
        setCategoryMeta(meta);
      } catch { }
      finally { setLoading(false); }
    };
    fetchData();
  }, []);

  const currentYear = new Date().getFullYear();

  return (
    <div className="home-page">
      {/* ===== HERO ===== */}
      <section className="hero-section">
        <div className="hero-clock-bg">
          <ClockCircleOutlined className="hero-clock-icon" />
        </div>

        <div className="hero-content">
          <div className="hero-label fade-up">Bộ sưu tập {currentYear}</div>

          <Title className="hero-title fade-up fade-up-delay-1">
            Thời gian<br />là <em>nghệ thuật</em>
          </Title>

          <p className="hero-subtitle fade-up fade-up-delay-2">
            Khám phá những chiếc đồng hồ được chế tác tỉ mỉ từ các thương hiệu hàng đầu thế giới.
            Mỗi chiếc đồng hồ là một tuyên ngôn về phong cách sống.
          </p>

          <div className="hero-cta fade-up fade-up-delay-3">
            <Button
              className="btn-primary-gold"
              onClick={() => navigate("/products")}
              icon={<ArrowRightOutlined />}
            >
              Khám phá ngay
            </Button>
            <Button
              className="btn-ghost-white"
              onClick={() => navigate("/products")}
            >
              Xem bộ sưu tập
            </Button>
          </div>

          <div className="hero-stats fade-up fade-up-delay-3">
            <div className="stat-item">
              <div className="stat-num">
                {loading ? "—" : `${totalProducts}+`}
              </div>
              <div className="stat-label">Sản phẩm</div>
            </div>
            <div className="stat-item">
              <div className="stat-num">
                {loading ? "—" : categories.length}
              </div>
              <div className="stat-label">Danh mục</div>
            </div>
            <div className="stat-item">
              <div className="stat-num">100%</div>
              <div className="stat-label">Chính hãng</div>
            </div>
          </div>
        </div>
      </section>

      {/* ===== CATEGORIES ===== */}
      <section className="categories-section">
        <div className="section-header">
          <div className="section-eyebrow">Danh mục</div>
          <Title className="section-title">Chọn theo phong cách</Title>
        </div>

        <Row gutter={[24, 24]}>
          {loading
            ? [1, 2, 3, 4].map((i) => (
              <Col key={i} xs={12} md={6}>
                <div className="category-skeleton" />
              </Col>
            ))
            : categories.map((cat, i) => {
              const bgColors = ["#0d0d0d", "#111108", "#080d0d", "#0d0808"];
              const meta = categoryMeta[cat.id];
              const catImage = cat.imageUrl || meta?.imageUrl;
              const catCount = meta?.count || 0;
              return (
                <Col key={cat.id} xs={12} md={6}>
                  <div
                    className="category-card"
                    style={{ background: bgColors[i % bgColors.length] }}
                    onClick={() => navigate(`/products?CategoryId=${cat.id}`)}
                  >
                    {catImage ? (
                      <img
                        src={catImage}
                        alt={cat.name}
                        className="category-card-img"
                        onError={(e) => { e.target.style.display = "none"; }}
                      />
                    ) : (
                      <ClockCircleOutlined className="category-card-icon" />
                    )}
                    <div className="category-card-content">
                      <div className="category-name">{cat.name}</div>
                      <div className="category-count">
                        {catCount > 0 ? `${catCount} sản phẩm` : "Khám phá"} →
                      </div>
                    </div>
                  </div>
                </Col>
              );
            })}
        </Row>
      </section>

      {/* ===== FEATURED PRODUCTS ===== */}
      <section className="featured-section">
        <div className="featured-header">
          <div>
            <div className="section-eyebrow">Nổi bật</div>
            <Title className="section-title">Sản phẩm tiêu biểu</Title>
          </div>
          <Button
            className="btn-primary-gold"
            onClick={() => navigate("/products")}
            icon={<ArrowRightOutlined />}
          >
            Xem tất cả
          </Button>
        </div>

        {loading ? (
          <div className="featured-loading"><Spin size="large" /></div>
        ) : (
          <Row gutter={[24, 24]}>
            {featuredProducts.map((product) => (
              <Col key={product.id} xs={12} md={6}>
                <div
                  className="product-card-luxury"
                  onClick={() => navigate(`/products/${product.id}`)}
                >
                  <div className="product-img-wrap">
                    {product.imageUrl ? (
                      <img
                        src={product.imageUrl}
                        alt={product.name}
                        onError={(e) => { e.target.src = "https://placehold.co/400x300?text=ShopNN"; }}
                      />
                    ) : (
                      <ClockCircleOutlined className="product-img-placeholder" />
                    )}
                  </div>
                  <div className="product-info">
                    {product.categoryName && (
                      <div className="product-category-tag">{product.categoryName}</div>
                    )}
                    <div className="product-name">{product.name}</div>
                    <div className="product-price">{formatPrice(product.price)}</div>
                  </div>
                </div>
              </Col>
            ))}
          </Row>
        )}
      </section>

      {/* ===== WHY US ===== */}
      <section className="why-section">
        <div className="why-header">
          <div className="section-eyebrow">Cam kết</div>
          <Title className="section-title">Tại sao chọn ShopNN?</Title>
        </div>
        <Row gutter={[24, 24]}>
          {WHY_US.map((item, i) => (
            <Col key={i} xs={12} md={6}>
              <div className="why-card">
                <span className="why-icon">{item.icon}</span>
                <div className="why-title">{item.title}</div>
                <div className="why-desc">{item.desc}</div>
              </div>
            </Col>
          ))}
        </Row>
      </section>

      {/* ===== CTA BANNER ===== */}
      <section className="cta-section">
        <div>
          <div className="cta-eyebrow">Ưu đãi đặc biệt</div>
          <div className="cta-title">
            Miễn phí vận chuyển<br />cho đơn từ 5 triệu
          </div>
        </div>
        <Button
          className="btn-cta-dark"
          onClick={() => navigate("/products")}
        >
          Mua ngay →
        </Button>
      </section>
    </div>
  );
}