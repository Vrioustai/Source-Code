-- ==============================================================
CREATE DATABASE QL_Butalo;
GO

USE QL_Butalo;
GO
-- ==============================================================

-- Bảng Tài Khoản
CREATE TABLE tblTaiKhoan (
    PK_MaTaiKhoan NVARCHAR(10) PRIMARY KEY,
    sMatKhau NVARCHAR(255) NOT NULL, -- Tăng độ dài để lưu Hash BCrypt
    sSDT NVARCHAR(10) NOT NULL,
    sHoTen NVARCHAR(50) NOT NULL,
    sVaiTro NVARCHAR(30) NOT NULL,
    bTrangThai BIT NOT NULL CONSTRAINT DF_tblTaiKhoan_bTrangThai DEFAULT(1)
);

-- Bảng Loại Căn Hộ
CREATE TABLE tblLoaiCanHo (
    PK_MaLoaiCanHo NVARCHAR(10) PRIMARY KEY,
    sTenLoaiCanHo NVARCHAR(50) NOT NULL
);

-- Bảng Kiểm Duyệt
CREATE TABLE tblKiemDuyet (
    PK_MaKiemDuyet NVARCHAR(15) PRIMARY KEY,
    sTrangThaiDuyet NVARCHAR(50)
);

-- Bảng Dịch Vụ
CREATE TABLE tblDichVu (
    PK_MaDichVu NVARCHAR(10) PRIMARY KEY, 
    sTenDichVu NVARCHAR(50) NOT NULL
);

-- Bảng Tỉnh Thành Phố
CREATE TABLE tblTinhThanhPho (
    PK_MaTinhThanhPho NVARCHAR(10) PRIMARY KEY,
    sTenTinhThanhPho NVARCHAR(50) NOT NULL
);


-- ==============================================================
-- 2. TẠO CÁC BẢNG PHỤ THUỘC (CÓ CHỨA KHÓA NGOẠI)
-- ==============================================================

-- Bảng Quận Huyện
CREATE TABLE tblQuanHuyen (
    PK_MaQuanHuyen NVARCHAR(10) PRIMARY KEY,
    sTenQuanHuyen NVARCHAR(50) NOT NULL, 
    FK_MaTinhThanhPho NVARCHAR(10) NOT NULL,
    FOREIGN KEY (FK_MaTinhThanhPho) REFERENCES tblTinhThanhPho(PK_MaTinhThanhPho)
);

-- Bảng Xã Phường Thị Trấn
CREATE TABLE tblXaPhuongThiTran (
    PK_MaXaPhuongThiTran NVARCHAR(10) PRIMARY KEY,
    sTenXaPhuongThiTran NVARCHAR(50) NOT NULL,
    FK_MaQuanHuyen NVARCHAR(10) NOT NULL,
    FOREIGN KEY (FK_MaQuanHuyen) REFERENCES tblQuanHuyen(PK_MaQuanHuyen)
);

-- Bảng Căn Hộ (Bảng trung tâm)
CREATE TABLE tblCanHo (
    PK_MaCanHo NVARCHAR(10) PRIMARY KEY,
    FK_MaLoaiCanHo NVARCHAR(10) NOT NULL,
    FK_MaTaiKhoan NVARCHAR(10) NOT NULL,
    FK_MaKiemDuyet NVARCHAR(15) NOT NULL,
    sTenCanHo NVARCHAR(255),
    fGiaCanHo FLOAT,
    fGiaDien FLOAT,
    fGiaNuoc FLOAT,
    dNgayDang DATE,
    sSDT NVARCHAR(15),
    fDienTich FLOAT,
    bTrangThai BIT,
    
    FOREIGN KEY (FK_MaLoaiCanHo) REFERENCES tblLoaiCanHo(PK_MaLoaiCanHo),
    FOREIGN KEY (FK_MaTaiKhoan) REFERENCES tblTaiKhoan(PK_MaTaiKhoan),
    FOREIGN KEY (FK_MaKiemDuyet) REFERENCES tblKiemDuyet(PK_MaKiemDuyet)
);

-- Bảng tblCanHo_DichVu
CREATE TABLE tblCanHo_DichVu (
    PK_MaCanHo NVARCHAR(10) NOT NULL,
    PK_MaDichVu NVARCHAR(10) NOT NULL,
    PRIMARY KEY (PK_MaCanHo, PK_MaDichVu),
    FOREIGN KEY (PK_MaCanHo) REFERENCES tblCanHo(PK_MaCanHo),
    FOREIGN KEY (PK_MaDichVu) REFERENCES tblDichVu(PK_MaDichVu)
);

-- Bảng Ảnh
CREATE TABLE tblAnh (
    PK_MaAnh NVARCHAR(10) PRIMARY KEY,
    FK_MaCanHo NVARCHAR(10) NOT NULL,
    FK_MaTaiKhoan NVARCHAR(10) NOT NULL,
    sDuongDan NVARCHAR(255) NOT NULL,
    FOREIGN KEY (FK_MaCanHo) REFERENCES tblCanHo(PK_MaCanHo),
    FOREIGN KEY (FK_MaTaiKhoan) REFERENCES tblTaiKhoan(PK_MaTaiKhoan)
);

-- Bảng Địa Chỉ
CREATE TABLE tblDiaChi (
    PK_MaDiaChi NVARCHAR(10) PRIMARY KEY,
    FK_MaCanHo NVARCHAR(10) NOT NULL,
    FK_MaQuanHuyen NVARCHAR(10) NOT NULL,
    FK_MaTinhThanhPho NVARCHAR(10) NOT NULL,
    FK_MaXaPhuongThiTran NVARCHAR(10),
    sDiaChiChiTiet NVARCHAR(255),
    
    FOREIGN KEY (FK_MaCanHo) REFERENCES tblCanHo(PK_MaCanHo),
    FOREIGN KEY (FK_MaQuanHuyen) REFERENCES tblQuanHuyen(PK_MaQuanHuyen),
    FOREIGN KEY (FK_MaTinhThanhPho) REFERENCES tblTinhThanhPho(PK_MaTinhThanhPho),
    FOREIGN KEY (FK_MaXaPhuongThiTran) REFERENCES tblXaPhuongThiTran(PK_MaXaPhuongThiTran)
);

-- Bảng Tố cáo
CREATE TABLE tblToCao (
    PK_MaToCao NVARCHAR(12) PRIMARY KEY,
    FK_MaCanHo NVARCHAR(10) NOT NULL,
    FK_MaTaiKhoan NVARCHAR(10) NOT NULL,
    FK_MaKiemDuyet NVARCHAR(15) NOT NULL,
    sLoaiViPham NVARCHAR(255) NOT NULL,
    sNoiDung NVARCHAR(1000),
    dNgayTao DATE NOT NULL,
    FOREIGN KEY (FK_MaCanHo) REFERENCES tblCanHo(PK_MaCanHo),
    FOREIGN KEY (FK_MaTaiKhoan) REFERENCES tblTaiKhoan(PK_MaTaiKhoan),
    FOREIGN KEY (FK_MaKiemDuyet) REFERENCES tblKiemDuyet(PK_MaKiemDuyet)
);


-- ==============================================================
-- DỮ LIỆU MẪU CAO CẤP - QL_Butalo
-- ==============================================================

-- 1. LOẠI CĂN HỘ
INSERT INTO tblLoaiCanHo (PK_MaLoaiCanHo, sTenLoaiCanHo) VALUES
(N'LP001', N'Studio Cao Cấp'),
(N'LP002', N'Căn hộ 1 Phòng Ngủ'),
(N'LP003', N'Căn hộ 2 Phòng Ngủ'),
(N'LP004', N'Duplex'),
(N'LP005', N'Penthouse Mini');

-- 2. KIỂM DUYỆT
INSERT INTO tblKiemDuyet (PK_MaKiemDuyet, sTrangThaiDuyet) VALUES
(N'KD001', N'Đã duyệt'),
(N'KD002', N'Chờ duyệt'),
(N'KD003', N'Từ chối');

-- 3. DỊCH VỤ CAO CẤP
INSERT INTO tblDichVu (PK_MaDichVu, sTenDichVu) VALUES
(N'DV001', N'Smart Home (Điều khiển giọng nói)'),
(N'DV002', N'Hồ bơi vô cực'),
(N'DV003', N'Gym & Yoga Center'),
(N'DV004', N'Lễ tân & Bảo vệ 24/7'),
(N'DV005', N'An ninh FaceID/Vân tay'),
(N'DV006', N'Dọn dẹp phòng 2 lần/tuần'),
(N'DV007', N'Bãi đỗ xe ô tô thông minh'),
(N'DV008', N'Khu BBQ sân thượng');

-- 4. TỈNH THÀNH PHỐ
INSERT INTO tblTinhThanhPho (PK_MaTinhThanhPho, sTenTinhThanhPho) VALUES
(N'TP001', N'Hà Nội'),
(N'TP002', N'TP. Hồ Chí Minh'),
(N'TP003', N'Đà Nẵng');

-- 5. QUẬN HUYỆN
INSERT INTO tblQuanHuyen (PK_MaQuanHuyen, sTenQuanHuyen, FK_MaTinhThanhPho) VALUES
(N'QH001', N'Cầu Giấy',   N'TP001'),
(N'QH002', N'Tây Hồ',     N'TP001'),
(N'QH003', N'Nam Từ Liêm',N'TP001'),
(N'QH005', N'Quận 1',     N'TP002'),
(N'QH006', N'Quận 2 (Thủ Đức)', N'TP002'),
(N'QH007', N'Bình Thạnh', N'TP002'),
(N'QH009', N'Sơn Trà',    N'TP003');

-- 6. XÃ PHƯỜNG THỊ TRẤN
INSERT INTO tblXaPhuongThiTran (PK_MaXaPhuongThiTran, sTenXaPhuongThiTran, FK_MaQuanHuyen) VALUES
(N'PX001', N'Phường Dịch Vọng Hậu',  N'QH001'),
(N'PX002', N'Phường Quảng An',       N'QH002'),
(N'PX003', N'Phường Mễ Trì',         N'QH003'),
(N'PX008', N'Phường Bến Nghé',       N'QH005'),
(N'PX009', N'Phường Thảo Điền',      N'QH006'),
(N'PX010', N'Phường 22 (Vinhomes)',  N'QH007'),
(N'PX012', N'Phường Phước Mỹ',       N'QH009');

-- 7. TÀI KHOẢN (Mật khẩu được băm bằng BCrypt - Mặc định là '123456')
-- Hash: $2a$11$0aE6mG2qB0.oJ.J8S7sZ6O1x/1JkI5M.Xv8xQ5QzJzRzH/U.5a00C (tương đương 123456)
INSERT INTO tblTaiKhoan (PK_MaTaiKhoan, sMatKhau, sSDT, sHoTen, sVaiTro, bTrangThai) VALUES
(N'TK0001', N'$2a$11$0aE6mG2qB0.oJ.J8S7sZ6O1x/1JkI5M.Xv8xQ5QzJzRzH/U.5a00C', N'0901234567', N'Nguyễn Văn CEO',  N'ChuTro',     1),
(N'TK0002', N'$2a$11$0aE6mG2qB0.oJ.J8S7sZ6O1x/1JkI5M.Xv8xQ5QzJzRzH/U.5a00C', N'0912345678', N'Trần Thị Quản Lý',  N'ChuTro',     1),
(N'TK0003', N'$2a$11$0aE6mG2qB0.oJ.J8S7sZ6O1x/1JkI5M.Xv8xQ5QzJzRzH/U.5a00C', N'0923456789', N'Lê Khách VIP',   N'NguoiDung',     1),
(N'TK0006', N'$2a$11$0aE6mG2qB0.oJ.J8S7sZ6O1x/1JkI5M.Xv8xQ5QzJzRzH/U.5a00C', N'0909090909', N'Admin Butalo', N'QuanTri',    1);

-- 8. CĂN HỘ
INSERT INTO tblCanHo (PK_MaCanHo, FK_MaLoaiCanHo, FK_MaTaiKhoan, FK_MaKiemDuyet,
    sTenCanHo, fGiaCanHo, fGiaDien, fGiaNuoc, dNgayDang, sSDT, fDienTich, bTrangThai) VALUES
(N'CH0001', N'LP001', N'TK0001', N'KD001', N'Studio VIP View Hồ Tây - Smart Home',       12000000, 3800, 25000, '2025-12-01', N'0901234567', 45, 1),
(N'CH0002', N'LP002', N'TK0001', N'KD001', N'Căn Hộ 1PN Dịch Vọng Hậu Full Kính Đèn Led',15000000, 3800, 25000, '2025-12-10', N'0901234567', 60, 1),
(N'CH0003', N'LP004', N'TK0002', N'KD001', N'Duplex Thảo Điền Phong Cách Châu Âu',       25000000, 4000, 30000, '2026-01-05', N'0912345678', 120, 1),
(N'CH0004', N'LP005', N'TK0002', N'KD001', N'Penthouse Mini Landmark 81 View Đỉnh',      45000000, 4000, 30000, '2026-01-15', N'0912345678', 150, 1),
(N'CH0005', N'LP003', N'TK0001', N'KD001', N'Căn Hộ 2PN Mễ Trì Kế Bên The Manor',        18000000, 3800, 25000, '2026-02-01', N'0901234567', 85, 1),
(N'CH0006', N'LP001', N'TK0002', N'KD001', N'Studio Ban Công Biển Mỹ Khê Đà Nẵng',       10000000, 3500, 20000, '2026-02-10', N'0912345678', 40, 1);

-- 9. ĐỊA CHỈ
INSERT INTO tblDiaChi (PK_MaDiaChi, FK_MaCanHo, FK_MaQuanHuyen, FK_MaTinhThanhPho, FK_MaXaPhuongThiTran, sDiaChiChiTiet) VALUES
(N'DC001', N'CH0001', N'QH002', N'TP001', N'PX002', N'Tòa nhà Butalo Tower, Số 12 Xuân Diệu'),
(N'DC002', N'CH0002', N'QH001', N'TP001', N'PX001', N'Butalo Residence, Số 45 Trần Thái Tông'),
(N'DC003', N'CH0003', N'QH006', N'TP002', N'PX009', N'Butalo Thảo Điền Elite, Số 8 Nguyễn Văn Hưởng'),
(N'DC004', N'CH0004', N'QH007', N'TP002', N'PX010', N'Khu dân cư cao cấp Vinhomes Central Park'),
(N'DC005', N'CH0005', N'QH003', N'TP001', N'PX003', N'Số 5 Đường Mễ Trì Thượng'),
(N'DC006', N'CH0006', N'QH009', N'TP003', N'PX012', N'Butalo Ocean View, Số 22 Võ Nguyên Giáp');

-- 10. ẢNH (Ảnh placeholder phân giải cao)
INSERT INTO tblAnh (PK_MaAnh, FK_MaCanHo, FK_MaTaiKhoan, sDuongDan) VALUES
(N'ANH001', N'CH0001', N'TK0001', N'https://images.unsplash.com/photo-1502672260266-1c1de2d93688?w=800&q=80'),
(N'ANH002', N'CH0002', N'TK0001', N'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800&q=80'),
(N'ANH003', N'CH0003', N'TK0002', N'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=800&q=80'),
(N'ANH004', N'CH0004', N'TK0002', N'https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800&q=80'),
(N'ANH005', N'CH0005', N'TK0001', N'https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=800&q=80'),
(N'ANH006', N'CH0006', N'TK0002', N'https://images.unsplash.com/photo-1497366216548-37526070297c?w=800&q=80');

-- 11. DỊCH VỤ CĂN HỘ (Full dịch vụ)
INSERT INTO tblCanHo_DichVu (PK_MaCanHo, PK_MaDichVu) VALUES
(N'CH0001', N'DV001'), (N'CH0001', N'DV004'), (N'CH0001', N'DV005'), (N'CH0001', N'DV006'),
(N'CH0002', N'DV001'), (N'CH0002', N'DV004'), (N'CH0002', N'DV005'), (N'CH0002', N'DV007'),
(N'CH0003', N'DV001'), (N'CH0003', N'DV002'), (N'CH0003', N'DV003'), (N'CH0003', N'DV004'), (N'CH0003', N'DV005'), (N'CH0003', N'DV006'), (N'CH0003', N'DV007'), (N'CH0003', N'DV008'),
(N'CH0004', N'DV001'), (N'CH0004', N'DV002'), (N'CH0004', N'DV003'), (N'CH0004', N'DV004'), (N'CH0004', N'DV005'), (N'CH0004', N'DV006'), (N'CH0004', N'DV007'), (N'CH0004', N'DV008'),
(N'CH0005', N'DV001'), (N'CH0005', N'DV004'), (N'CH0005', N'DV005'), (N'CH0005', N'DV006'),
(N'CH0006', N'DV001'), (N'CH0006', N'DV002'), (N'CH0006', N'DV004'), (N'CH0006', N'DV006');