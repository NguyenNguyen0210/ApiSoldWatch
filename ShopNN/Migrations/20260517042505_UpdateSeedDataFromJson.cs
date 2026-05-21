using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShopNN.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedDataFromJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bc04aa19-f509-4a2d-8572-368965c22854", "AQAAAAIAAYagAAAAEGGCrAwKrG0lDWM2hMLUUKMg4asI2fNBSnd+CjDJFoBqVkfS7VIvIwO5+jZ9q0w2BQ==", "STATIC-SECURITY-STAMP-FOR-SEED-DATA" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 5, "Fashion Watches" },
                    { 6, "Dive Watches" },
                    { 7, "Pilot Watches" }
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "Description", "ImageUrl", "Name", "Price" },
                values: new object[] { "Đồng hồ lặn huyền thoại với vỏ Oystersteel 41mm, mặt số đen Cerachrom, chống nước 300m. Bộ máy Calibre 3235 tự động, dự trữ năng lượng 70 giờ. Dây đeo Oyster với khóa Oysterlock an toàn và hệ thống điều chỉnh Glidelock.", "https://images.unsplash.com/photo-1622434641406-a158123450f9?w=800", "Rolex Submariner Date 126610LN", 245000000m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "Description", "ImageUrl", "Name", "Price" },
                values: new object[] { "Biểu tượng của tốc độ và đẳng cấp. Vỏ Oystersteel 40mm, bezel Cerachrom đen, mặt số Panda trắng. Bộ máy Calibre 4131 chronograph, dự trữ năng lượng 72 giờ. Chống nước 100m.", "https://images.unsplash.com/photo-1547996160-81dfa63595aa?w=800", "Rolex Daytona 126500LN", 385000000m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { "Chiếc đồng hồ thể thao sang trọng kinh điển. Vỏ thép không gỉ 40mm, mặt số xanh gradient, dây thép tích hợp. Bộ máy Calibre 26-330 S C tự động, chống nước 120m. Thiết kế bởi Gerald Genta.", "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?w=800", "Patek Philippe Nautilus 5711/1A-010", 2850000000m, 1 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 1, "Vỏ thép không gỉ 41mm, mặt số xanh Petite Tapisserie. Bộ máy Calibre 4302 tự động, dự trữ năng lượng 70 giờ. Thiết kế octagon với 8 ốc vít hexagonal đặc trưng. Chống nước 50m.", "https://images.unsplash.com/photo-1614164185128-e4ec99c436d7?w=800", "Audemars Piguet Royal Oak 15500ST", 1250000000m, 2 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 1, "Chiếc đồng hồ đầu tiên lên Mặt Trăng. Vỏ thép 42mm, mặt số đen, kính Hesalite. Bộ máy Calibre 3861 METAS lên dây cót thủ công, dự trữ 50 giờ. Chronograph với tachymeter bezel.", "https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=800", "Omega Speedmaster Moonwatch Professional", 152000000m, 5 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 6, "Vỏ thép không gỉ 42mm, mặt số ceramic xanh biển. Bộ máy Master Chronometer 8800, chống từ 15,000 gauss. Chống nước 300m, van thoát khí helium. Dây cao su hoặc thép.", "https://images.unsplash.com/photo-1612817159949-195b6eb9e31a?w=800", "Omega Seamaster Diver 300M", 125000000m, 7 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price" },
                values: new object[] { 2, "Cấu trúc Carbon Core Guard siêu bền. Chống bùn, chống va đập, chống rung. Triple Sensor (la bàn, đo cao, khí áp/nhiệt độ). Năng lượng mặt trời, đồng bộ sóng radio Multiband 6.", "https://images.unsplash.com/photo-1522312346375-d1a52e2b99b3?w=800", "Casio G-Shock Mudmaster GWG-2000", 18500000m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 108,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 2, "Thiết kế bát giác mỏng nhất dòng G-Shock chỉ 11.8mm. Vỏ nhựa carbon, chống va đập. Hiển thị analog-digital kết hợp. Pin CR2016 thời lượng 3 năm. Chống nước 200m.", "https://images.unsplash.com/photo-1508685096489-77a46807e624?w=800", "Casio G-Shock GA-2100-1A1 'CasiOak'", 3200000m, 50 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 109,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 6, "Tái bản mẫu đồng hồ lặn đầu tiên của Seiko. Vỏ thép 40.5mm, mặt số xanh gradient. Bộ máy 6R35 tự động, dự trữ 70 giờ. Kính sapphire, chống nước 200m. Dây silicone.", "https://images.unsplash.com/photo-1509048191080-d2984bad6ad5?w=800", "Seiko Prospex SPB143 '62MAS Reissue'", 28500000m, 12 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 110,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 2, "Đồng hồ thể thao GPS cao cấp nhất của Garmin. Màn hình MIP 1.4 inch, sạc năng lượng mặt trời. Bản đồ TopoActive, đèn LED tích hợp. Pin lên đến 37 ngày (GPS), cảm biến SpO2, nhịp tim quang học.", "https://images.unsplash.com/photo-1517502884422-41eaead166d4?w=800", "Garmin Fenix 7X Pro Solar", 22900000m, 15 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 111,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 3, "Vỏ titanium 49mm, màn hình OLED 2000 nits. Chip S9, Double Tap, GPS L1+L5 chính xác cao. Chống nước 100m, đạt chuẩn EN13319 cho lặn. Pin 36 giờ sử dụng bình thường, 72 giờ tiết kiệm pin.", "https://images.unsplash.com/photo-1434493907317-a46b5bc78344?w=800", "Apple Watch Ultra 2", 21990000m, 30 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { 112, 3, "Vòng bezel xoay cổ điển, màn hình Super AMOLED 1.5 inch. Chip Exynos W930, Wear OS, theo dõi giấc ngủ nâng cao. Cảm biến BioActive, đo nhiệt độ da, ECG, huyết áp. Pin 425mAh.", "https://images.unsplash.com/photo-1579586337278-3befd40fd17a?w=800", "Samsung Galaxy Watch 6 Classic 47mm", 9490000m, 35 },
                    { 113, 3, "Màn hình AMOLED 1.4 inch sáng rõ. Theo dõi sức khỏe toàn diện: Body Battery, Sleep Coach, HRV, Nap Detection. Hơn 30 ứng dụng thể thao tích hợp. Pin lên đến 14 ngày. Hỗ trợ nghe gọi Bluetooth.", "https://images.unsplash.com/photo-1544117519-31731f4fcdde?w=800", "Garmin Venu 3", 11990000m, 20 },
                    { 114, 4, "Thiết kế retro-chic lấy cảm hứng từ thập niên 70. Vỏ thép 40mm, mặt số xanh lá waffle. Bộ máy Powermatic 80 tự động, dự trữ 80 giờ. Kính sapphire chống xước. Chống nước 100m.", "https://images.unsplash.com/photo-1533139502658-0198f920d8e8?w=800", "Tissot PRX Powermatic 80", 16200000m, 18 },
                    { 115, 4, "Đồng hồ quân đội kinh điển. Vỏ thép 38mm, mặt số đen matte. Bộ máy H-50 lên dây cót thủ công, dự trữ 80 giờ. Dây vải NATO olive. Kính sapphire, chống nước 50m.", "https://images.unsplash.com/photo-1548171915-e79a380a2a4b?w=800", "Hamilton Khaki Field Mechanical", 12500000m, 14 },
                    { 116, 4, "Vỏ thép 40mm, mặt số trắng kem với chỉ báo trăng tròn lúc 6h. Bộ máy L899 tự động (base ETA), dự trữ 72 giờ. Kim xanh thép, index La Mã. Dây da cá sấu đen.", "https://images.unsplash.com/photo-1542496658-e33a6d0d50f6?w=800", "Longines Master Collection Moonphase", 58000000m, 6 },
                    { 123, 4, "Dress watch giá tốt nhất phân khúc. Vỏ thép 40.5mm, mặt số kem vintage, kim blued hands. Bộ máy F6722 tự động, dự trữ 40 giờ. Kính mineral dome. Dây da nâu.", "https://images.unsplash.com/photo-1455397792935-60e8e4d4c0a5?w=800", "Orient Bambino Version 2 FAC00005W0", 4800000m, 20 },
                    { 124, 3, "Màn hình AMOLED 1.43 inch, vỏ bezel có thể thay thế. HyperOS, hỗ trợ eSIM, GPS độc lập. Theo dõi 150+ bài tập, SpO2 24/7. Pin 486mAh, sử dụng 15 ngày. Chống nước 5ATM.", "https://images.unsplash.com/photo-1461141346587-763ab02bced9?w=800", "Xiaomi Watch S3", 3290000m, 45 },
                    { 125, 2, "Dòng đồng hồ tự động huyền thoại của Seiko. Vỏ thép 42.5mm, mặt số đen sunburst. Bộ máy 4R36 tự động, dự trữ 41 giờ, hacking & hand-winding. Chống nước 100m. Day-Date hiển thị.", "https://images.unsplash.com/photo-1539874754764-5a96559165b0?w=800", "Seiko 5 Sports SRPD55K1", 6800000m, 30 },
                    { 126, 2, "Biểu tượng đua xe thể thao. Vỏ thép 44mm, mặt số đen với subdials bạc. Bộ máy Heuer 02 COSC in-house chronograph, dự trữ 80 giờ. Kính sapphire, caseback transparent.", "https://images.unsplash.com/photo-1600721391776-b5cd0e0048f2?w=800", "TAG Heuer Carrera Chronograph 44mm", 128000000m, 5 },
                    { 127, 1, "Chiếc đồng hồ đeo tay đầu tiên trong lịch sử (1904). Vỏ thép 35.1mm, mặt số bạc guilloché, số La Mã xanh. Bộ máy 1847 MC tự động. Hệ thống QuickSwitch dây da/thép.", "https://images.unsplash.com/photo-1526045431048-f857369baa09?w=800", "Cartier Santos de Cartier Medium", 168000000m, 4 },
                    { 130, 2, "Chronograph thể thao kết nối Bluetooth. Vỏ thép carbon 48mm, mặt số đen đa lớp. Năng lượng mặt trời Tough Solar. World time 300 thành phố, đồng bộ thời gian qua smartphone.", "https://images.unsplash.com/photo-1495856458515-0637185db551?w=800", "Casio Edifice ECB-2000DC-1A", 8500000m, 18 },
                    { 117, 5, "Thiết kế minimalist thanh lịch cho phái nữ. Vỏ thép mạ vàng hồng 32mm, mặt số trắng tinh khiết. Bộ máy Miyota quartz Nhật Bản. Dây da đen Italian. Chống nước 30m.", "https://images.unsplash.com/photo-1524805444758-089113d48a6d?w=800", "Daniel Wellington Petite Sheffield 32mm", 4590000m, 40 },
                    { 118, 5, "Kết hợp kim analog truyền thống và màn hình E-Ink. Theo dõi nhịp tim, SpO2, giấc ngủ. Pin 2 tuần. Kết nối Bluetooth, thông báo điện thoại. Vỏ thép 44mm, chống nước 50m.", "https://images.unsplash.com/photo-1434056886845-dbe89f0b9571?w=800", "Fossil Gen 6 Hybrid Wellness Edition", 5990000m, 22 },
                    { 119, 7, "Vỏ thép 40mm, mặt số đen matte với triangle index lúc 12h. Bộ máy 32111 tự động (base Sellita), dự trữ 120 giờ. Kính sapphire chống phản chiếu, hệ thống bảo vệ từ trường.", "https://images.unsplash.com/photo-1587925358603-c2eea5305bbc?w=800", "IWC Pilot's Watch Mark XX", 115000000m, 4 },
                    { 120, 7, "Biểu tượng hàng không với slide rule bezel. Vỏ thép 43mm, bộ máy in-house B01 chronograph, dự trữ 70 giờ. Mặt số đen với 3 subdial. Chống nước 30m. COSC Chronometer certified.", "https://images.unsplash.com/photo-1509941943102-10c232535736?w=800", "Breitling Navitimer B01 Chronograph 43", 198000000m, 3 },
                    { 121, 6, "Vỏ thép 39mm kích thước vintage hoàn hảo. Mặt số đen gilt, bezel đen nhôm. Bộ máy MT5402 COSC tự động, dự trữ 70 giờ. Chống nước 200m. Dây thép riveted style.", "https://images.unsplash.com/photo-1548169874-53e85f753f1e?w=800", "Tudor Black Bay 58", 89000000m, 8 },
                    { 122, 6, "Đồng hồ lặn năng lượng ánh sáng, không cần thay pin. Vỏ thép 44mm, mặt số xanh biển. Chống nước 200m ISO 6425. Dạ quang Lumi-Brite mạnh mẽ. Dây polyurethane bền bỉ.", "https://images.unsplash.com/photo-1506193095-80a5798431c7?w=800", "Citizen Promaster Eco-Drive BN0151-09L", 7200000m, 28 },
                    { 128, 5, "Thiết kế Bauhaus tinh khiết bởi nghệ sĩ Max Bill. Vỏ thép 38mm, mặt số trắng với font số đặc trưng. Bộ máy J800.1 tự động. Kính Plexiglas dome, dây da đen. Chống nước 30m.", "https://images.unsplash.com/photo-1533139143976-30918502365b?w=800", "Junghans Max Bill Automatic", 28500000m, 10 },
                    { 129, 7, "Đồng hồ lặn thiết kế vuông độc đáo lấy cảm hứng từ buồng lái máy bay. Vỏ thép 42mm, mặt số đen matte. Bộ máy BR-CAL.302 tự động. Chống nước 300m, bezel xoay một chiều.", "https://images.unsplash.com/photo-1585123334904-845d60e97b29?w=800", "Bell & Ross BR 03-92 Diver", 98000000m, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 120);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 125);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 126);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 127);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 128);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 129);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 130);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "eb0eea4a-5312-45ff-af62-5ceb568afdd9", "AQAAAAIAAYagAAAAEGYHx8/9xyW7ZIwMfvTO0jPDLL3KZCCmuOQyZcd5cY/7df0uFtnxPnzwIrj2NCmmEQ==", "b1b464a1-eb20-4653-a76b-23475271c401" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "Description", "ImageUrl", "Name", "Price" },
                values: new object[] { "18ct yellow gold, President bracelet", "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?q=80&w=1000&auto=format&fit=crop", "Rolex Day-Date 40", 38000m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "Description", "ImageUrl", "Name", "Price" },
                values: new object[] { "Steel blue dial, luxury sports watch", "https://images.unsplash.com/photo-1547996160-81dfa63595aa?q=80&w=1000&auto=format&fit=crop", "Patek Philippe Nautilus", 120000m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { "Selfwinding 'Jumbo' Extra-thin", "https://images.unsplash.com/photo-1614164185128-e4ec99c436d7?q=80&w=1000&auto=format&fit=crop", "Audemars Piguet Royal Oak", 75000m, 2 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 2, "Carbon Core Guard, Triple Sensor", "https://images.unsplash.com/photo-1522312346375-d1a52e2b99b3?q=80&w=1000&auto=format&fit=crop", "Casio G-Shock Mudmaster", 850m, 20 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 2, "Automatic diver's watch 200m", "https://images.unsplash.com/photo-1612817159949-195b6eb9e31a?q=80&w=1000&auto=format&fit=crop", "Seiko Prospex 'Turtle'", 550m, 15 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 2, "Solar powered multisport GPS watch", "https://images.unsplash.com/photo-1517502884422-41eaead166d4?q=80&w=1000&auto=format&fit=crop", "Garmin Fenix 7X", 999m, 10 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price" },
                values: new object[] { 3, "Rugged and capable, with GPS + Cellular", "https://images.unsplash.com/photo-1434493907317-a46b5bc78344?q=80&w=1000&auto=format&fit=crop", "Apple Watch Ultra 2", 799m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 108,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 3, "Advanced sleep tracking and wellness", "https://images.unsplash.com/photo-1508685096489-77a46807e624?q=80&w=1000&auto=format&fit=crop", "Samsung Galaxy Watch 6", 350m, 30 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 109,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 4, "Elegant moonphase automatic watch", "https://images.unsplash.com/photo-1524592094714-0f0654e20314?q=80&w=1000&auto=format&fit=crop", "Longines Master Collection", 2500m, 8 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 110,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 4, "Traditional swiss automatic watch", "https://images.unsplash.com/photo-1533139502658-0198f920d8e8?q=80&w=1000&auto=format&fit=crop", "Tissot Le Locle", 650m, 12 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 111,
                columns: new[] { "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[] { 4, "Open heart dial, stainless steel", "https://images.unsplash.com/photo-1509048191080-d2984bad6ad5?q=80&w=1000&auto=format&fit=crop", "Hamilton Jazzmaster", 950m, 7 });
        }
    }
}
