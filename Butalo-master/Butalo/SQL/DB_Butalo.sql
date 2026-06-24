-- ==============================================================
CREATE DATABASE QL_CanHo;
GO

USE QL_CanHo;
GO
-- ==============================================================

-- Bảng Tài Khoản
CREATE TABLE tblTaiKhoan (
    PK_MaTaiKhoan NVARCHAR(10) PRIMARY KEY,
    sMatKhau NVARCHAR(12) NOT NULL,
    sSDT NVARCHAR(10) NOT NULL,
    sHoTen NVARCHAR(30) NOT NULL,
    sVaiTro NVARCHAR(30) NOT NULL
);

-- Bảng Loại Căn Hộ
CREATE TABLE tblLoaiCanHo (
    PK_MaLoaiCanHo NVARCHAR(10) PRIMARY KEY,
    sTenLoaiCanHo NVARCHAR(50) NOT NULL
);

-- Bảng Kiểm Duyệt (Dựa vào Sơ đồ quan hệ [23])
CREATE TABLE tblKiemDuyet (
    PK_MaKiemDuyet NVARCHAR(15) PRIMARY KEY,
    sTrangThaiDuyet NVARCHAR(50)
);

-- Bảng Dịch Vụ
CREATE TABLE tblDichVu (
    PK_MaDichVu NVARCHAR(10) PRIMARY KEY, 
    sTenDichVu NVARCHAR(15) NOT NULL
);

-- Bảng Tỉnh Thành Phố
CREATE TABLE tblTinhThanhPho (
    PK_MaTinhThanhPho NVARCHAR(10) PRIMARY KEY,
    sTenTinhThanhPho NVARCHAR(30) NOT NULL
);


-- ==============================================================
-- 2. TẠO CÁC BẢNG PHỤ THUỘC (CÓ CHỨA KHÓA NGOẠI)
-- ==============================================================

-- Bảng Quận Huyện
CREATE TABLE tblQuanHuyen (
    PK_MaQuanHuyen NVARCHAR(10) PRIMARY KEY,
    sTenQuanHuyen NVARCHAR(50) NOT NULL, 
    FK_MaTinhThanhPho NVARCHAR(10) NOT NULL, -- Đã đồng bộ với PK_MaTinhThanhPho
    FOREIGN KEY (FK_MaTinhThanhPho) REFERENCES tblTinhThanhPho(PK_MaTinhThanhPho)
);

-- Bảng Xã Phường Thị Trấn
CREATE TABLE tblXaPhuongThiTran (
    PK_MaXaPhuongThiTran NVARCHAR(10) PRIMARY KEY,
    sTenXaPhuongThiTran NVARCHAR(50) NOT NULL,
    FK_MaQuanHuyen NVARCHAR(10) NOT NULL, -- Đã đồng bộ với PK_MaQuanHuyen
    FOREIGN KEY (FK_MaQuanHuyen) REFERENCES tblQuanHuyen(PK_MaQuanHuyen)
);

-- Bảng Căn Hộ (Bảng trung tâm)
CREATE TABLE tblCanHo (
    PK_MaCanHo NVARCHAR(10) PRIMARY KEY,
    FK_MaLoaiCanHo NVARCHAR(10) NOT NULL,
    FK_MaTaiKhoan NVARCHAR(10) NOT NULL, -- Đã đồng bộ với PK_MaTaiKhoan (10 thay vì 15)
    FK_MaKiemDuyet NVARCHAR(15) NOT NULL,
    sTenCanHo NVARCHAR(100), -- Kiểu dữ liệu giả định
    fGiaCanHo FLOAT,            -- Kiểu dữ liệu giả định
    fGiaDien FLOAT,             -- Kiểu dữ liệu giả định
    fGiaNuoc FLOAT,             -- Kiểu dữ liệu giả định
    dNgayDang DATE,             -- Kiểu dữ liệu giả định
    sSDT NVARCHAR(15),          -- Kiểu dữ liệu giả định
    fDienTich FLOAT,            -- Kiểu dữ liệu giả định
    bTrangThai BIT,             -- Kiểu dữ liệu giả định (True/False)
    
    FOREIGN KEY (FK_MaLoaiCanHo) REFERENCES tblLoaiCanHo(PK_MaLoaiCanHo),
    FOREIGN KEY (FK_MaTaiKhoan) REFERENCES tblTaiKhoan(PK_MaTaiKhoan),
    FOREIGN KEY (FK_MaKiemDuyet) REFERENCES tblKiemDuyet(PK_MaKiemDuyet)
);

-- Bảng tblCanHo_DichVu (Bảng trung gian phân giải quan hệ n-n từ Sơ đồ [23])
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
    FK_MaCanHo NVARCHAR(10) NOT NULL, -- Đã đồng bộ với PK_MaCanHo
    FK_MaTaiKhoan NVARCHAR(10) NOT NULL,
    sDuongDan NVARCHAR(255) NOT NULL, -- Khuyến nghị mở rộng từ 30 lên 255 để lưu đủ URL
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
    sDiaChiChiTiet NVARCHAR(100), -- Mở rộng từ 30 để ghi chi tiết số nhà
    
    FOREIGN KEY (FK_MaCanHo) REFERENCES tblCanHo(PK_MaCanHo),
    FOREIGN KEY (FK_MaQuanHuyen) REFERENCES tblQuanHuyen(PK_MaQuanHuyen),
    FOREIGN KEY (FK_MaTinhThanhPho) REFERENCES tblTinhThanhPho(PK_MaTinhThanhPho),
    FOREIGN KEY (FK_MaXaPhuongThiTran) REFERENCES tblXaPhuongThiTran(PK_MaXaPhuongThiTran)
);
