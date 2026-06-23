using Microsoft.Data.SqlClient;
using System.Text.Json;
using Butalo.Models;

namespace Butalo.Services;

public class ButaloRepository(IConfiguration configuration) : IButaloRepository
{
    private static readonly SemaphoreSlim ReportLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static string ReportStorePath => Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "reports.json");

    private readonly string _connectionString = configuration.GetConnectionString("ButaloDb")
        ?? throw new InvalidOperationException("Thiếu ConnectionStrings:ButaloDb trong appsettings.json");

    public async Task<List<CanHoItemViewModel>> LayDanhSachPhongAsync(int soLuong = 30)
    {
        const string sql = """
            SELECT TOP (@SoLuong)
                p.PK_MaCanHo,
                ISNULL(p.sTenCanHo, N'Căn hộ') AS sTenCanHo,
                ISNULL(p.fGiaCanHo, 0) AS fGiaCanHo,
                ISNULL(p.fDienTich, 0) AS fDienTich,
                ISNULL(dc.sDiaChiChiTiet, N'') + N' ' +
                ISNULL(xa.sTenXaPhuongThiTran, N'') + N' ' +
                ISNULL(qh.sTenQuanHuyen, N'') + N' ' +
                ISNULL(tp.sTenTinhThanhPho, N'') AS DiaChiDayDu,
                ISNULL(p.sSDT, N'') AS sSDT,
                (SELECT TOP 1 sDuongDan FROM tblAnh a WHERE a.FK_MaCanHo = p.PK_MaCanHo) AS sDuongDan
            FROM tblCanHo p
            LEFT JOIN tblDiaChi dc ON dc.FK_MaCanHo = p.PK_MaCanHo
            LEFT JOIN tblXaPhuongThiTran xa ON xa.PK_MaXaPhuongThiTran = dc.FK_MaXaPhuongThiTran
            LEFT JOIN tblQuanHuyen qh ON qh.PK_MaQuanHuyen = dc.FK_MaQuanHuyen
            LEFT JOIN tblTinhThanhPho tp ON tp.PK_MaTinhThanhPho = dc.FK_MaTinhThanhPho
            WHERE p.FK_MaKiemDuyet = N'KD001' AND ISNULL(p.bTrangThai, 0) = 1
            ORDER BY p.dNgayDang DESC, p.PK_MaCanHo DESC;
            """;

        var result = new List<CanHoItemViewModel>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SoLuong", soLuong);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new CanHoItemViewModel
            {
                MaCanHo = reader["PK_MaCanHo"]?.ToString() ?? string.Empty,
                TenPhong = reader["sTenCanHo"]?.ToString() ?? string.Empty,
                GiaCanHo = Convert.ToDouble(reader["fGiaCanHo"]),
                DienTich = Convert.ToDouble(reader["fDienTich"]),
                DiaChiDayDu = reader["DiaChiDayDu"]?.ToString()?.Trim() ?? string.Empty,
                SoDienThoai = reader["sSDT"]?.ToString() ?? string.Empty,
                DuongDanAnh = reader["sDuongDan"] as string
            });
        }

        return result;
    }

    public async Task<List<CanHoItemViewModel>> TimKiemPhongAsync(TimKiemCanHoViewModel boLoc, int soLuong = 200)
    {
        const string sql = """
            SELECT TOP (@SoLuong)
                p.PK_MaCanHo,
                ISNULL(p.sTenCanHo, N'Căn hộ') AS sTenCanHo,
                ISNULL(p.fGiaCanHo, 0) AS fGiaCanHo,
                ISNULL(p.fDienTich, 0) AS fDienTich,
                ISNULL(dc.sDiaChiChiTiet, N'') + N' ' +
                ISNULL(xa.sTenXaPhuongThiTran, N'') + N' ' +
                ISNULL(qh.sTenQuanHuyen, N'') + N' ' +
                ISNULL(tp.sTenTinhThanhPho, N'') AS DiaChiDayDu,
                ISNULL(p.sSDT, N'') AS sSDT,
                (SELECT TOP 1 sDuongDan FROM tblAnh a WHERE a.FK_MaCanHo = p.PK_MaCanHo) AS sDuongDan
            FROM tblCanHo p
            LEFT JOIN tblDiaChi dc ON dc.FK_MaCanHo = p.PK_MaCanHo
            LEFT JOIN tblXaPhuongThiTran xa ON xa.PK_MaXaPhuongThiTran = dc.FK_MaXaPhuongThiTran
            LEFT JOIN tblQuanHuyen qh ON qh.PK_MaQuanHuyen = dc.FK_MaQuanHuyen
            LEFT JOIN tblTinhThanhPho tp ON tp.PK_MaTinhThanhPho = dc.FK_MaTinhThanhPho
            WHERE p.FK_MaKiemDuyet = N'KD001'
              AND ISNULL(p.bTrangThai, 0) = 1
              AND (@MaLoaiCanHo IS NULL OR p.FK_MaLoaiCanHo = @MaLoaiCanHo)
              AND (@GiaMin IS NULL OR p.fGiaCanHo >= @GiaMin)
              AND (@GiaMax IS NULL OR p.fGiaCanHo <= @GiaMax)
              AND (@DienTichMin IS NULL OR p.fDienTich >= @DienTichMin)
              AND (@DienTichMax IS NULL OR p.fDienTich <= @DienTichMax)
            ORDER BY p.dNgayDang DESC, p.PK_MaCanHo DESC;
            """;

        var result = new List<CanHoItemViewModel>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SoLuong", soLuong);
        cmd.Parameters.AddWithValue("@MaLoaiCanHo", string.IsNullOrWhiteSpace(boLoc.MaLoaiCanHo) ? DBNull.Value : boLoc.MaLoaiCanHo.Trim());
        cmd.Parameters.AddWithValue("@GiaMin", boLoc.GiaMin.HasValue ? boLoc.GiaMin.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@GiaMax", boLoc.GiaMax.HasValue ? boLoc.GiaMax.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@DienTichMin", boLoc.DienTichMin.HasValue ? boLoc.DienTichMin.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@DienTichMax", boLoc.DienTichMax.HasValue ? boLoc.DienTichMax.Value : DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new CanHoItemViewModel
            {
                MaCanHo = reader["PK_MaCanHo"]?.ToString() ?? string.Empty,
                TenPhong = reader["sTenCanHo"]?.ToString() ?? string.Empty,
                GiaCanHo = Convert.ToDouble(reader["fGiaCanHo"]),
                DienTich = Convert.ToDouble(reader["fDienTich"]),
                DiaChiDayDu = reader["DiaChiDayDu"]?.ToString()?.Trim() ?? string.Empty,
                SoDienThoai = reader["sSDT"]?.ToString() ?? string.Empty,
                DuongDanAnh = reader["sDuongDan"] as string
            });
        }

        return result;
    }

    public async Task<ChiTietCanHoViewModel?> LayChiTietPhongAsync(string MaCanHo)
    {
        const string sql = """
            SELECT TOP 1
                p.PK_MaCanHo,
                ISNULL(p.sTenCanHo, N'Căn hộ') AS sTenCanHo,
                ISNULL(p.fGiaCanHo, 0) AS fGiaCanHo,
                ISNULL(p.fDienTich, 0) AS fDienTich,
                ISNULL(p.fGiaDien, 0) AS fGiaDien,
                ISNULL(p.fGiaNuoc, 0) AS fGiaNuoc,
                p.dNgayDang,
                ISNULL(p.sSDT, N'') AS sSDT,
                ISNULL(lp.sTenLoaiCanHo, N'') AS sTenLoaiCanHo,
                ISNULL(dc.sDiaChiChiTiet, N'') + N' ' +
                ISNULL(xa.sTenXaPhuongThiTran, N'') + N' ' +
                ISNULL(qh.sTenQuanHuyen, N'') + N' ' +
                ISNULL(tp.sTenTinhThanhPho, N'') AS DiaChiDayDu,
                (SELECT TOP 1 sDuongDan FROM tblAnh a WHERE a.FK_MaCanHo = p.PK_MaCanHo) AS sDuongDan
            FROM tblCanHo p
            LEFT JOIN tblLoaiCanHo lp ON lp.PK_MaLoaiCanHo = p.FK_MaLoaiCanHo
            LEFT JOIN tblDiaChi dc ON dc.FK_MaCanHo = p.PK_MaCanHo
            LEFT JOIN tblXaPhuongThiTran xa ON xa.PK_MaXaPhuongThiTran = dc.FK_MaXaPhuongThiTran
            LEFT JOIN tblQuanHuyen qh ON qh.PK_MaQuanHuyen = dc.FK_MaQuanHuyen
            LEFT JOIN tblTinhThanhPho tp ON tp.PK_MaTinhThanhPho = dc.FK_MaTinhThanhPho
            WHERE p.PK_MaCanHo = @MaCanHo AND p.FK_MaKiemDuyet = N'KD001' AND ISNULL(p.bTrangThai, 0) = 1;
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaCanHo", MaCanHo);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new ChiTietCanHoViewModel
        {
            MaCanHo = reader["PK_MaCanHo"]?.ToString() ?? string.Empty,
            TenPhong = reader["sTenCanHo"]?.ToString() ?? string.Empty,
            GiaCanHo = Convert.ToDouble(reader["fGiaCanHo"]),
            DienTich = Convert.ToDouble(reader["fDienTich"]),
            GiaDien = Convert.ToDouble(reader["fGiaDien"]),
            GiaNuoc = Convert.ToDouble(reader["fGiaNuoc"]),
            NgayDang = reader["dNgayDang"] == DBNull.Value ? null : Convert.ToDateTime(reader["dNgayDang"]),
            SoDienThoai = reader["sSDT"]?.ToString() ?? string.Empty,
            LoaiCanHo = reader["sTenLoaiCanHo"]?.ToString() ?? string.Empty,
            DiaChiDayDu = reader["DiaChiDayDu"]?.ToString()?.Trim() ?? string.Empty,
            DuongDanAnh = reader["sDuongDan"] as string
        };
    }

    public async Task<ChiTietCanHoViewModel?> LayChiTietPhongChoPhepAsync(string MaCanHo, string? maTaiKhoan, bool laQuanTri)
    {
        // Nếu là quản trị hoặc chủ tin -> xem được cả tin chưa duyệt / bị tắt
        const string sql = """
            SELECT TOP 1
                p.PK_MaCanHo,
                ISNULL(p.sTenCanHo, N'Căn hộ') AS sTenCanHo,
                ISNULL(p.fGiaCanHo, 0) AS fGiaCanHo,
                ISNULL(p.fDienTich, 0) AS fDienTich,
                ISNULL(p.fGiaDien, 0) AS fGiaDien,
                ISNULL(p.fGiaNuoc, 0) AS fGiaNuoc,
                p.dNgayDang,
                ISNULL(p.sSDT, N'') AS sSDT,
                ISNULL(lp.sTenLoaiCanHo, N'') AS sTenLoaiCanHo,
                ISNULL(dc.sDiaChiChiTiet, N'') + N' ' +
                ISNULL(xa.sTenXaPhuongThiTran, N'') + N' ' +
                ISNULL(qh.sTenQuanHuyen, N'') + N' ' +
                ISNULL(tp.sTenTinhThanhPho, N'') AS DiaChiDayDu,
                (SELECT TOP 1 sDuongDan FROM tblAnh a WHERE a.FK_MaCanHo = p.PK_MaCanHo) AS sDuongDan
            FROM tblCanHo p
            LEFT JOIN tblLoaiCanHo lp ON lp.PK_MaLoaiCanHo = p.FK_MaLoaiCanHo
            LEFT JOIN tblDiaChi dc ON dc.FK_MaCanHo = p.PK_MaCanHo
            LEFT JOIN tblXaPhuongThiTran xa ON xa.PK_MaXaPhuongThiTran = dc.FK_MaXaPhuongThiTran
            LEFT JOIN tblQuanHuyen qh ON qh.PK_MaQuanHuyen = dc.FK_MaQuanHuyen
            LEFT JOIN tblTinhThanhPho tp ON tp.PK_MaTinhThanhPho = dc.FK_MaTinhThanhPho
            WHERE p.PK_MaCanHo = @MaCanHo
              AND (
                    (p.FK_MaKiemDuyet = N'KD001' AND ISNULL(p.bTrangThai, 0) = 1)
                    OR (@LaQuanTri = 1)
                    OR (p.FK_MaTaiKhoan = @MaTaiKhoan)
                  );
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaCanHo", MaCanHo);
        cmd.Parameters.AddWithValue("@LaQuanTri", laQuanTri ? 1 : 0);
        cmd.Parameters.AddWithValue("@MaTaiKhoan", (object?)maTaiKhoan ?? DBNull.Value);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new ChiTietCanHoViewModel
        {
            MaCanHo = reader["PK_MaCanHo"]?.ToString() ?? string.Empty,
            TenPhong = reader["sTenCanHo"]?.ToString() ?? string.Empty,
            GiaCanHo = Convert.ToDouble(reader["fGiaCanHo"]),
            DienTich = Convert.ToDouble(reader["fDienTich"]),
            GiaDien = Convert.ToDouble(reader["fGiaDien"]),
            GiaNuoc = Convert.ToDouble(reader["fGiaNuoc"]),
            NgayDang = reader["dNgayDang"] == DBNull.Value ? null : Convert.ToDateTime(reader["dNgayDang"]),
            SoDienThoai = reader["sSDT"]?.ToString() ?? string.Empty,
            LoaiCanHo = reader["sTenLoaiCanHo"]?.ToString() ?? string.Empty,
            DiaChiDayDu = reader["DiaChiDayDu"]?.ToString()?.Trim() ?? string.Empty,
            DuongDanAnh = reader["sDuongDan"] as string
        };
    }

    public async Task<string> TaoTaiKhoanAsync(DangKyTaiKhoanViewModel model)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var maMoi = await TaoMaTaiKhoanMoiAsync(conn);
        var hashMatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhau);
        const string sql = """
            INSERT INTO tblTaiKhoan(PK_MaTaiKhoan, sMatKhau, sSDT, sHoTen, sVaiTro)
            VALUES(@MaTaiKhoan, @MatKhau, @SoDienThoai, @HoTen, @VaiTro);
            """;
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaTaiKhoan", maMoi);
        cmd.Parameters.AddWithValue("@MatKhau", hashMatKhau);
        cmd.Parameters.AddWithValue("@SoDienThoai", model.SoDienThoai);
        cmd.Parameters.AddWithValue("@HoTen", model.HoTen);
        cmd.Parameters.AddWithValue("@VaiTro", string.Equals(model.VaiTro, "ChuTro", StringComparison.OrdinalIgnoreCase) ? "ChuTro" : "NguoiDung");
        await cmd.ExecuteNonQueryAsync();
        return maMoi;
    }

    public async Task<bool> KiemTraDangNhapAsync(DangNhapViewModel model)
    {
        return await LayTaiKhoanTheoDangNhapAsync(model) is not null;
    }

    public async Task<TaiKhoanDto?> LayTaiKhoanTheoDangNhapAsync(DangNhapViewModel model)
    {
        const string sql = """
            SELECT TOP 1 PK_MaTaiKhoan, sSDT, sHoTen, sVaiTro, sMatKhau
            FROM tblTaiKhoan
            WHERE sSDT = @SoDienThoai AND bTrangThai = 1;
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SoDienThoai", model.SoDienThoai);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var dbHash = reader["sMatKhau"]?.ToString() ?? string.Empty;
        bool isValid = false;
        try { isValid = BCrypt.Net.BCrypt.Verify(model.MatKhau, dbHash); }
        catch { isValid = (dbHash == model.MatKhau); } // Fallback plain text

        if (!isValid) return null;

        return new TaiKhoanDto
        {
            MaTaiKhoan = reader["PK_MaTaiKhoan"]?.ToString() ?? string.Empty,
            SoDienThoai = reader["sSDT"]?.ToString() ?? string.Empty,
            HoTen = reader["sHoTen"]?.ToString() ?? string.Empty,
            VaiTro = reader["sVaiTro"]?.ToString() ?? string.Empty
        };
    }

    public async Task<TaiKhoanDto?> LayTaiKhoanTheoMaAsync(string maTaiKhoan)
    {
        const string sql = """
            SELECT TOP 1 PK_MaTaiKhoan, sSDT, sHoTen, sVaiTro
            FROM tblTaiKhoan
            WHERE PK_MaTaiKhoan = @MaTaiKhoan;
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new TaiKhoanDto
        {
            MaTaiKhoan = reader["PK_MaTaiKhoan"]?.ToString() ?? string.Empty,
            SoDienThoai = reader["sSDT"]?.ToString() ?? string.Empty,
            HoTen = reader["sHoTen"]?.ToString() ?? string.Empty,
            VaiTro = reader["sVaiTro"]?.ToString() ?? string.Empty
        };
    }

    public async Task CapNhatThongTinTaiKhoanAsync(CapNhatTaiKhoanViewModel model)
    {
        const string sql = """
            UPDATE tblTaiKhoan
            SET sHoTen = @HoTen,
                sMatKhau = COALESCE(NULLIF(@MatKhauMoi, N''), sMatKhau)
            WHERE PK_MaTaiKhoan = @MaTaiKhoan;
            """;

        string? hashMatKhauMoi = string.IsNullOrEmpty(model.MatKhauMoi) ? null : BCrypt.Net.BCrypt.HashPassword(model.MatKhauMoi);

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaTaiKhoan", model.MaTaiKhoan);
        cmd.Parameters.AddWithValue("@HoTen", model.HoTen);
        cmd.Parameters.AddWithValue("@MatKhauMoi", (object?)hashMatKhauMoi ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<TaiKhoanDto>> LayDanhSachTaiKhoanAsync()
    {
        const string sql = """
            SELECT PK_MaTaiKhoan, sSDT, sHoTen, sVaiTro
            FROM tblTaiKhoan
            ORDER BY PK_MaTaiKhoan DESC;
            """;

        var result = new List<TaiKhoanDto>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new TaiKhoanDto
            {
                MaTaiKhoan = reader["PK_MaTaiKhoan"]?.ToString() ?? string.Empty,
                SoDienThoai = reader["sSDT"]?.ToString() ?? string.Empty,
                HoTen = reader["sHoTen"]?.ToString() ?? string.Empty,
                VaiTro = reader["sVaiTro"]?.ToString() ?? string.Empty
            });
        }
        return result;
    }

    public async Task CapNhatTaiKhoanBoiQuanTriAsync(CapNhatTaiKhoanQuanTriViewModel model)
    {
        const string sql = """
            UPDATE tblTaiKhoan
            SET sHoTen = @HoTen,
                sSDT = @SoDienThoai
            WHERE PK_MaTaiKhoan = @MaTaiKhoan;
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaTaiKhoan", model.MaTaiKhoan);
        cmd.Parameters.AddWithValue("@HoTen", model.HoTen);
        cmd.Parameters.AddWithValue("@SoDienThoai", model.SoDienThoai);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task CapLaiMatKhauAsync(string maTaiKhoan, string matKhauMoi)
    {
        const string sql = """
            UPDATE tblTaiKhoan
            SET sMatKhau = @MatKhauMoi
            WHERE PK_MaTaiKhoan = @MaTaiKhoan;
            """;

        var hashMatKhau = BCrypt.Net.BCrypt.HashPassword(matKhauMoi);

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
        cmd.Parameters.AddWithValue("@MatKhauMoi", hashMatKhau);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<LoaiCanHoDto>> LayLoaiCanHoAsync()
    {
        const string sql = """
            SELECT PK_MaLoaiCanHo, sTenLoaiCanHo
            FROM tblLoaiCanHo
            ORDER BY PK_MaLoaiCanHo;
            """;

        var result = new List<LoaiCanHoDto>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new LoaiCanHoDto(
                reader["PK_MaLoaiCanHo"]?.ToString() ?? string.Empty,
                reader["sTenLoaiCanHo"]?.ToString() ?? string.Empty));
        }
        return result;
    }

    public async Task<List<TinhThanhPhoDto>> LayTinhThanhPhoAsync()
    {
        const string sql = """
            SELECT PK_MaTinhThanhPho, sTenTinhThanhPho
            FROM tblTinhThanhPho
            ORDER BY PK_MaTinhThanhPho;
            """;

        var result = new List<TinhThanhPhoDto>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new TinhThanhPhoDto(
                reader["PK_MaTinhThanhPho"]?.ToString() ?? string.Empty,
                reader["sTenTinhThanhPho"]?.ToString() ?? string.Empty));
        }
        return result;
    }

    public async Task<List<QuanHuyenDto>> LayQuanHuyenAsync(string maTinhThanhPho)
    {
        const string sql = """
            SELECT PK_MaQuanHuyen, sTenQuanHuyen
            FROM tblQuanHuyen
            WHERE FK_MaTinhThanhPho = @MaTinh
            ORDER BY PK_MaQuanHuyen;
            """;

        var result = new List<QuanHuyenDto>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaTinh", maTinhThanhPho);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new QuanHuyenDto(
                reader["PK_MaQuanHuyen"]?.ToString() ?? string.Empty,
                reader["sTenQuanHuyen"]?.ToString() ?? string.Empty));
        }
        return result;
    }

    public async Task<List<XaPhuongDto>> LayXaPhuongAsync(string maQuanHuyen)
    {
        const string sql = """
            SELECT PK_MaXaPhuongThiTran, sTenXaPhuongThiTran
            FROM tblXaPhuongThiTran
            WHERE FK_MaQuanHuyen = @MaQuan
            ORDER BY PK_MaXaPhuongThiTran;
            """;

        var result = new List<XaPhuongDto>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaQuan", maQuanHuyen);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new XaPhuongDto(
                reader["PK_MaXaPhuongThiTran"]?.ToString() ?? string.Empty,
                reader["sTenXaPhuongThiTran"]?.ToString() ?? string.Empty));
        }
        return result;
    }

    public async Task<string> TaoTinPhongAsync(TaoTinCanHoViewModel model)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tx = conn.BeginTransaction();

        try
        {
            var MaCanHoMoi = await TaoMaCanHoMoiAsync(conn, tx);
            var maDiaChiMoi = await TaoMaDiaChiMoiAsync(conn, tx);

            const string sqlPhong = """
                INSERT INTO tblCanHo
                    (PK_MaCanHo, FK_MaLoaiCanHo, FK_MaTaiKhoan, FK_MaKiemDuyet,
                     sTenCanHo, fGiaCanHo, fGiaDien, fGiaNuoc, dNgayDang, sSDT, fDienTich, bTrangThai)
                VALUES
                    (@MaCanHo, @MaLoaiCanHo, @MaTaiKhoan, N'KD002',
                     @TenCanHo, @GiaCanHo, @GiaDien, @GiaNuoc, CAST(GETDATE() AS DATE), @SoDienThoai, @DienTich, 0);
                """;

            await using (var cmd = new SqlCommand(sqlPhong, conn, tx))
            {
                cmd.Parameters.AddWithValue("@MaCanHo", MaCanHoMoi);
                cmd.Parameters.AddWithValue("@MaLoaiCanHo", model.MaLoaiCanHo);
                cmd.Parameters.AddWithValue("@MaTaiKhoan", model.MaTaiKhoan);
                cmd.Parameters.AddWithValue("@TenCanHo", model.TenCanHo);
                cmd.Parameters.AddWithValue("@GiaCanHo", model.GiaCanHo);
                cmd.Parameters.AddWithValue("@GiaDien", model.GiaDien);
                cmd.Parameters.AddWithValue("@GiaNuoc", model.GiaNuoc);
                cmd.Parameters.AddWithValue("@SoDienThoai", model.SoDienThoai);
                cmd.Parameters.AddWithValue("@DienTich", model.DienTich);
                await cmd.ExecuteNonQueryAsync();
            }

            const string sqlDiaChi = """
                INSERT INTO tblDiaChi
                    (PK_MaDiaChi, FK_MaCanHo, FK_MaQuanHuyen, FK_MaTinhThanhPho, FK_MaXaPhuongThiTran, sDiaChiChiTiet)
                VALUES
                    (@MaDiaChi, @MaCanHo, @MaQuan, @MaTinh, @MaXa, @DiaChiChiTiet);
                """;

            await using (var cmd = new SqlCommand(sqlDiaChi, conn, tx))
            {
                cmd.Parameters.AddWithValue("@MaDiaChi", maDiaChiMoi);
                cmd.Parameters.AddWithValue("@MaCanHo", MaCanHoMoi);
                cmd.Parameters.AddWithValue("@MaQuan", model.MaQuanHuyen);
                cmd.Parameters.AddWithValue("@MaTinh", model.MaTinhThanhPho);
                cmd.Parameters.AddWithValue("@MaXa", string.IsNullOrWhiteSpace(model.MaXaPhuong) ? DBNull.Value : model.MaXaPhuong);
                cmd.Parameters.AddWithValue("@DiaChiChiTiet", (object?)model.DiaChiChiTiet ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return MaCanHoMoi;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<TaoTinCanHoViewModel?> LayTinPhongDeSuaAsync(string MaCanHo)
    {
        const string sql = """
            SELECT TOP 1
                p.PK_MaCanHo,
                p.FK_MaTaiKhoan,
                p.FK_MaLoaiCanHo,
                p.sTenCanHo,
                ISNULL(p.fGiaCanHo, 0) AS fGiaCanHo,
                ISNULL(p.fDienTich, 0) AS fDienTich,
                ISNULL(p.fGiaDien, 0) AS fGiaDien,
                ISNULL(p.fGiaNuoc, 0) AS fGiaNuoc,
                ISNULL(p.sSDT, N'') AS sSDT,
                dc.FK_MaTinhThanhPho,
                dc.FK_MaQuanHuyen,
                dc.FK_MaXaPhuongThiTran,
                dc.sDiaChiChiTiet
            FROM tblCanHo p
            LEFT JOIN tblDiaChi dc ON dc.FK_MaCanHo = p.PK_MaCanHo
            WHERE p.PK_MaCanHo = @MaCanHo;
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaCanHo", MaCanHo);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new TaoTinCanHoViewModel
        {
            MaCanHo = reader["PK_MaCanHo"]?.ToString(),
            MaTaiKhoan = reader["FK_MaTaiKhoan"]?.ToString() ?? string.Empty,
            MaLoaiCanHo = reader["FK_MaLoaiCanHo"]?.ToString() ?? string.Empty,
            TenCanHo = reader["sTenCanHo"]?.ToString() ?? string.Empty,
            GiaCanHo = Convert.ToDouble(reader["fGiaCanHo"]),
            DienTich = Convert.ToDouble(reader["fDienTich"]),
            GiaDien = Convert.ToDouble(reader["fGiaDien"]),
            GiaNuoc = Convert.ToDouble(reader["fGiaNuoc"]),
            SoDienThoai = reader["sSDT"]?.ToString() ?? string.Empty,
            MaTinhThanhPho = reader["FK_MaTinhThanhPho"]?.ToString() ?? string.Empty,
            MaQuanHuyen = reader["FK_MaQuanHuyen"]?.ToString() ?? string.Empty,
            MaXaPhuong = reader["FK_MaXaPhuongThiTran"] == DBNull.Value ? null : reader["FK_MaXaPhuongThiTran"]?.ToString(),
            DiaChiChiTiet = reader["sDiaChiChiTiet"] == DBNull.Value ? null : reader["sDiaChiChiTiet"]?.ToString()
        };
    }

    public async Task CapNhatTinPhongAsync(TaoTinCanHoViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.MaCanHo))
        {
            throw new ArgumentException("Thiếu mã phòng để cập nhật.", nameof(model));
        }

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tx = conn.BeginTransaction();

        try
        {
            const string sqlPhong = """
                UPDATE tblCanHo
                SET FK_MaLoaiCanHo = @MaLoaiCanHo,
                    sTenCanHo = @TenCanHo,
                    fGiaCanHo = @GiaCanHo,
                    fGiaDien = @GiaDien,
                    fGiaNuoc = @GiaNuoc,
                    sSDT = @SoDienThoai,
                    fDienTich = @DienTich
                WHERE PK_MaCanHo = @MaCanHo;
                """;

            await using (var cmd = new SqlCommand(sqlPhong, conn, tx))
            {
                cmd.Parameters.AddWithValue("@MaCanHo", model.MaCanHo);
                cmd.Parameters.AddWithValue("@MaLoaiCanHo", model.MaLoaiCanHo);
                cmd.Parameters.AddWithValue("@TenCanHo", model.TenCanHo);
                cmd.Parameters.AddWithValue("@GiaCanHo", model.GiaCanHo);
                cmd.Parameters.AddWithValue("@GiaDien", model.GiaDien);
                cmd.Parameters.AddWithValue("@GiaNuoc", model.GiaNuoc);
                cmd.Parameters.AddWithValue("@SoDienThoai", model.SoDienThoai);
                cmd.Parameters.AddWithValue("@DienTich", model.DienTich);
                await cmd.ExecuteNonQueryAsync();
            }

            const string sqlDiaChi = """
                UPDATE tblDiaChi
                SET FK_MaTinhThanhPho = @MaTinh,
                    FK_MaQuanHuyen = @MaQuan,
                    FK_MaXaPhuongThiTran = @MaXa,
                    sDiaChiChiTiet = @DiaChiChiTiet
                WHERE FK_MaCanHo = @MaCanHo;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO tblDiaChi
                        (PK_MaDiaChi, FK_MaCanHo, FK_MaQuanHuyen, FK_MaTinhThanhPho, FK_MaXaPhuongThiTran, sDiaChiChiTiet)
                    VALUES
                        (@MaDiaChi, @MaCanHo, @MaQuan, @MaTinh, @MaXa, @DiaChiChiTiet);
                END
                """;

            await using (var cmd = new SqlCommand(sqlDiaChi, conn, tx))
            {
                cmd.Parameters.AddWithValue("@MaCanHo", model.MaCanHo);
                cmd.Parameters.AddWithValue("@MaTinh", model.MaTinhThanhPho);
                cmd.Parameters.AddWithValue("@MaQuan", model.MaQuanHuyen);
                cmd.Parameters.AddWithValue("@MaXa", string.IsNullOrWhiteSpace(model.MaXaPhuong) ? DBNull.Value : model.MaXaPhuong);
                cmd.Parameters.AddWithValue("@DiaChiChiTiet", (object?)model.DiaChiChiTiet ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MaDiaChi", await TaoMaDiaChiMoiAsync(conn, tx));
                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<List<TinCuaToiDto>> LayDanhSachTinCuaToiAsync(string maTaiKhoan)
    {
        const string sql = """
            SELECT
                p.PK_MaCanHo,
                ISNULL(p.sTenCanHo, N'') AS sTenCanHo,
                ISNULL(p.fGiaCanHo, 0) AS fGiaCanHo,
                ISNULL(p.fDienTich, 0) AS fDienTich,
                p.dNgayDang,
                ISNULL(kd.sTrangThaiDuyet, N'') AS sTrangThaiDuyet,
                ISNULL(p.bTrangThai, 0) AS bTrangThai
            FROM tblCanHo p
            LEFT JOIN tblKiemDuyet kd ON kd.PK_MaKiemDuyet = p.FK_MaKiemDuyet
            WHERE p.FK_MaTaiKhoan = @MaTaiKhoan
            ORDER BY p.dNgayDang DESC, p.PK_MaCanHo DESC;
            """;

        var result = new List<TinCuaToiDto>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new TinCuaToiDto
            {
                MaCanHo = reader["PK_MaCanHo"]?.ToString() ?? string.Empty,
                TenCanHo = reader["sTenCanHo"]?.ToString() ?? string.Empty,
                GiaCanHo = Convert.ToDouble(reader["fGiaCanHo"]),
                DienTich = Convert.ToDouble(reader["fDienTich"]),
                NgayDang = reader["dNgayDang"] == DBNull.Value ? null : Convert.ToDateTime(reader["dNgayDang"]),
                TrangThaiDuyet = reader["sTrangThaiDuyet"]?.ToString() ?? string.Empty,
                TrangThaiHienThi = Convert.ToInt32(reader["bTrangThai"]) == 1
            });
        }
        return result;
    }

    public async Task<List<TinChoDuyetDto>> LayDanhSachTinChoDuyetAsync()
    {
        const string sql = """
            SELECT
                p.PK_MaCanHo,
                ISNULL(p.sTenCanHo, N'') AS sTenCanHo,
                p.FK_MaTaiKhoan,
                ISNULL(tk.sHoTen, N'') AS sHoTen,
                ISNULL(p.sSDT, N'') AS sSDT,
                ISNULL(p.fGiaCanHo, 0) AS fGiaCanHo,
                ISNULL(p.fDienTich, 0) AS fDienTich,
                p.dNgayDang,
                ISNULL(kd.sTrangThaiDuyet, N'') AS sTrangThaiDuyet
            FROM tblCanHo p
            LEFT JOIN tblTaiKhoan tk ON tk.PK_MaTaiKhoan = p.FK_MaTaiKhoan
            LEFT JOIN tblKiemDuyet kd ON kd.PK_MaKiemDuyet = p.FK_MaKiemDuyet
            WHERE p.FK_MaKiemDuyet = N'KD002'
            ORDER BY p.dNgayDang DESC, p.PK_MaCanHo DESC;
            """;

        var result = new List<TinChoDuyetDto>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new TinChoDuyetDto
            {
                MaCanHo = reader["PK_MaCanHo"]?.ToString() ?? string.Empty,
                TenCanHo = reader["sTenCanHo"]?.ToString() ?? string.Empty,
                MaTaiKhoan = reader["FK_MaTaiKhoan"]?.ToString() ?? string.Empty,
                TenChuTro = reader["sHoTen"]?.ToString() ?? string.Empty,
                SoDienThoai = reader["sSDT"]?.ToString() ?? string.Empty,
                GiaCanHo = Convert.ToDouble(reader["fGiaCanHo"]),
                DienTich = Convert.ToDouble(reader["fDienTich"]),
                NgayDang = reader["dNgayDang"] == DBNull.Value ? null : Convert.ToDateTime(reader["dNgayDang"]),
                TrangThaiDuyet = reader["sTrangThaiDuyet"]?.ToString() ?? string.Empty
            });
        }

        return result;
    }

    public async Task CapNhatTrangThaiDuyetTinAsync(string MaCanHo, string maKiemDuyet, bool trangThaiHoatDong)
    {
        const string sql = """
            UPDATE tblCanHo
            SET FK_MaKiemDuyet = @MaKiemDuyet,
                bTrangThai = @TrangThai
            WHERE PK_MaCanHo = @MaCanHo;
            """;

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaCanHo", MaCanHo);
        cmd.Parameters.AddWithValue("@MaKiemDuyet", maKiemDuyet);
        cmd.Parameters.AddWithValue("@TrangThai", trangThaiHoatDong ? 1 : 0);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<string> UploadAnhAsync(IFormFile file, IWebHostEnvironment env)
    {
        var tenFile = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var folder = Path.Combine(env.WebRootPath, "uploads", "phong");
        Directory.CreateDirectory(folder);
        var duongDanDayDu = Path.Combine(folder, tenFile);
        await using var stream = new FileStream(duongDanDayDu, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/phong/{tenFile}";
    }

    public async Task LuuAnhVaoDbAsync(string MaCanHo, string maTaiKhoan, string duongDan)
    {
        const string sql = """
            INSERT INTO tblAnh (PK_MaAnh, FK_MaCanHo, FK_MaTaiKhoan, sDuongDan)
            VALUES (@MaAnh, @MaCanHo, @MaTaiKhoan, @DuongDan);
            """;
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MaAnh", $"ANH{Guid.NewGuid().ToString()[..6].ToUpper()}");
        cmd.Parameters.AddWithValue("@MaCanHo", MaCanHo);
        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
        cmd.Parameters.AddWithValue("@DuongDan", duongDan);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<string> TaoMaTaiKhoanMoiAsync(SqlConnection conn)
    {
        const string sql = """
            SELECT TOP 1 PK_MaTaiKhoan
            FROM tblTaiKhoan
            WHERE PK_MaTaiKhoan LIKE 'TK%'
            ORDER BY PK_MaTaiKhoan DESC;
            """;
        await using var cmd = new SqlCommand(sql, conn);
        var maCuoi = (string?)await cmd.ExecuteScalarAsync();
        if (string.IsNullOrWhiteSpace(maCuoi))
        {
            return "TK0001";
        }

        var so = 0;
        _ = int.TryParse(maCuoi.Replace("TK", string.Empty), out so);
        return $"TK{(so + 1):D4}";
    }

    private static async Task<string> TaoMaCanHoMoiAsync(SqlConnection conn, SqlTransaction tx)
    {
        const string sql = """
            SELECT TOP 1 PK_MaCanHo
            FROM tblCanHo
            WHERE PK_MaCanHo LIKE 'P%'
            ORDER BY PK_MaCanHo DESC;
            """;

        await using var cmd = new SqlCommand(sql, conn, tx);
        var maCuoi = (string?)await cmd.ExecuteScalarAsync();
        if (string.IsNullOrWhiteSpace(maCuoi))
        {
            return "P0001";
        }

        var so = 0;
        _ = int.TryParse(maCuoi.Replace("P", string.Empty), out so);
        return $"P{(so + 1):D4}";
    }

    private static async Task<string> TaoMaDiaChiMoiAsync(SqlConnection conn, SqlTransaction tx)
    {
        const string sql = """
            SELECT TOP 1 PK_MaDiaChi
            FROM tblDiaChi
            WHERE PK_MaDiaChi LIKE 'DC%'
            ORDER BY PK_MaDiaChi DESC;
            """;

        await using var cmd = new SqlCommand(sql, conn, tx);
        var maCuoi = (string?)await cmd.ExecuteScalarAsync();
        if (string.IsNullOrWhiteSpace(maCuoi))
        {
            return "DC0001";
        }

        var so = 0;
        _ = int.TryParse(maCuoi.Replace("DC", string.Empty), out so);
        return $"DC{(so + 1):D4}";
    }

    public async Task<string> TaoToCaoAsync(TaoToCaoViewModel model)
    {
        await ReportLock.WaitAsync();
        try
        {
            var list = await ReadReportsAsync();
            var maMoi = TaoMaToCaoMoi(list);
            list.Add(new ReportRecord
            {
                MaToCao = maMoi,
                MaCanHo = model.MaCanHo,
                MaTaiKhoanNguoiBaoCao = model.MaTaiKhoanNguoiBaoCao,
                LoaiViPham = model.LoaiViPham,
                NoiDung = model.NoiDung,
                NgayTao = DateTime.UtcNow,
                MaKiemDuyet = "KD002"
            });
            await WriteReportsAsync(list);
            return maMoi;
        }
        finally
        {
            ReportLock.Release();
        }
    }

    public async Task<List<ToCaoChoDuyetDto>> LayDanhSachToCaoChoDuyetAsync()
    {
        var reports = await ReadReportsAsync();
        var pending = reports
            .Where(x => string.Equals(x.MaKiemDuyet, "KD002", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.NgayTao)
            .ToList();
        return await MapReportsAsync(pending);
    }

    public async Task<List<ToCaoChoDuyetDto>> LayDanhSachToCaoCuaToiAsync(string maTaiKhoan)
    {
        var reports = await ReadReportsAsync();
        var mine = reports
            .Where(x => string.Equals(x.MaTaiKhoanNguoiBaoCao, maTaiKhoan, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.NgayTao)
            .ToList();
        return await MapReportsAsync(mine);
    }

    public async Task CapNhatTrangThaiDuyetToCaoAsync(string maToCao, string maKiemDuyet)
    {
        await ReportLock.WaitAsync();
        try
        {
            var list = await ReadReportsAsync();
            var item = list.FirstOrDefault(x => string.Equals(x.MaToCao, maToCao, StringComparison.OrdinalIgnoreCase));
            if (item is not null)
            {
                item.MaKiemDuyet = maKiemDuyet;
                await WriteReportsAsync(list);
            }
        }
        finally
        {
            ReportLock.Release();
        }
    }

    public async Task XoaToCaoAsync(string maToCao)
    {
        await ReportLock.WaitAsync();
        try
        {
            var list = await ReadReportsAsync();
            list.RemoveAll(x => string.Equals(x.MaToCao, maToCao, StringComparison.OrdinalIgnoreCase));
            await WriteReportsAsync(list);
        }
        finally
        {
            ReportLock.Release();
        }
    }

    public async Task XoaPhongAsync(string MaCanHo)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tx = conn.BeginTransaction();

        try
        {
            await ExecAsync(conn, tx, "DELETE FROM tblCanHo_DichVu WHERE PK_MaCanHo = @MaCanHo;", ("@MaCanHo", MaCanHo));
            await ExecAsync(conn, tx, "DELETE FROM tblAnh WHERE FK_MaCanHo = @MaCanHo;", ("@MaCanHo", MaCanHo));
            await ExecAsync(conn, tx, "DELETE FROM tblDiaChi WHERE FK_MaCanHo = @MaCanHo;", ("@MaCanHo", MaCanHo));
            await ExecAsync(conn, tx, "DELETE FROM tblCanHo WHERE PK_MaCanHo = @MaCanHo;", ("@MaCanHo", MaCanHo));
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await ReportLock.WaitAsync();
        try
        {
            var list = await ReadReportsAsync();
            list.RemoveAll(x => string.Equals(x.MaCanHo, MaCanHo, StringComparison.OrdinalIgnoreCase));
            await WriteReportsAsync(list);
        }
        finally
        {
            ReportLock.Release();
        }
    }

    public async Task XoaTaiKhoanAsync(string maTaiKhoan)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tx = conn.BeginTransaction();
        var dsPhong = new List<string>();

        try
        {
            // Lấy danh sách phòng của tài khoản
            const string sqlPhong = "SELECT PK_MaCanHo FROM tblCanHo WHERE FK_MaTaiKhoan = @MaTaiKhoan;";
            await using (var cmd = new SqlCommand(sqlPhong, conn, tx))
            {
                cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    dsPhong.Add(reader["PK_MaCanHo"]?.ToString() ?? string.Empty);
                }
            }

            // Xóa các bản ghi phụ thuộc theo phòng
            foreach (var MaCanHo in dsPhong.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                await ExecAsync(conn, tx, "DELETE FROM tblCanHo_DichVu WHERE PK_MaCanHo = @MaCanHo;", ("@MaCanHo", MaCanHo));
                await ExecAsync(conn, tx, "DELETE FROM tblAnh WHERE FK_MaCanHo = @MaCanHo;", ("@MaCanHo", MaCanHo));
                await ExecAsync(conn, tx, "DELETE FROM tblDiaChi WHERE FK_MaCanHo = @MaCanHo;", ("@MaCanHo", MaCanHo));
                await ExecAsync(conn, tx, "DELETE FROM tblCanHo WHERE PK_MaCanHo = @MaCanHo;", ("@MaCanHo", MaCanHo));
            }

            // Xóa tài khoản
            await ExecAsync(conn, tx, "DELETE FROM tblTaiKhoan WHERE PK_MaTaiKhoan = @MaTaiKhoan;", ("@MaTaiKhoan", maTaiKhoan));

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await ReportLock.WaitAsync();
        try
        {
            var list = await ReadReportsAsync();
            list.RemoveAll(x =>
                string.Equals(x.MaTaiKhoanNguoiBaoCao, maTaiKhoan, StringComparison.OrdinalIgnoreCase) ||
                dsPhong.Any(p => string.Equals(p, x.MaCanHo, StringComparison.OrdinalIgnoreCase)));
            await WriteReportsAsync(list);
        }
        finally
        {
            ReportLock.Release();
        }
    }

    private static async Task ExecAsync(SqlConnection conn, SqlTransaction tx, string sql, params (string Name, object Value)[] parameters)
    {
        await using var cmd = new SqlCommand(sql, conn, tx);
        foreach (var p in parameters)
        {
            cmd.Parameters.AddWithValue(p.Name, p.Value);
        }
        await cmd.ExecuteNonQueryAsync();
    }

    private static string TaoMaToCaoMoi(List<ReportRecord> list)
    {
        var maCuoi = list
            .Select(x => x.MaToCao)
            .Where(x => !string.IsNullOrWhiteSpace(x) && x.StartsWith("TC", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(maCuoi))
        {
            return "TC00000001";
        }

        var so = 0;
        _ = int.TryParse(maCuoi.Replace("TC", string.Empty), out so);
        return $"TC{(so + 1):D8}";
    }

    private async Task<List<ToCaoChoDuyetDto>> MapReportsAsync(List<ReportRecord> reports)
    {
        var mapTenPhong = await LayMapTenPhongAsync();
        return reports.Select(x => new ToCaoChoDuyetDto
        {
            MaToCao = x.MaToCao,
            MaCanHo = x.MaCanHo,
            TenCanHo = mapTenPhong.TryGetValue(x.MaCanHo, out var ten) ? ten : x.MaCanHo,
            MaTaiKhoanNguoiBaoCao = x.MaTaiKhoanNguoiBaoCao,
            LoaiViPham = x.LoaiViPham,
            NoiDung = x.NoiDung,
            NgayTao = x.NgayTao,
            TrangThaiDuyet = x.MaKiemDuyet switch
            {
                "KD001" => "Đã duyệt",
                "KD003" => "Từ chối",
                _ => "Chờ duyệt"
            }
        }).ToList();
    }

    private async Task<Dictionary<string, string>> LayMapTenPhongAsync()
    {
        const string sql = "SELECT PK_MaCanHo, ISNULL(sTenCanHo, N'') AS sTenCanHo FROM tblCanHo;";
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var ma = reader["PK_MaCanHo"]?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(ma))
            {
                result[ma] = reader["sTenCanHo"]?.ToString() ?? string.Empty;
            }
        }

        return result;
    }

    private static async Task<List<ReportRecord>> ReadReportsAsync()
    {
        if (!File.Exists(ReportStorePath))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(ReportStorePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<ReportRecord>>(json, JsonOptions) ?? [];
    }

    private static async Task WriteReportsAsync(List<ReportRecord> list)
    {
        var dir = Path.GetDirectoryName(ReportStorePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(list, JsonOptions);
        await File.WriteAllTextAsync(ReportStorePath, json);
    }

    private class ReportRecord
    {
        public string MaToCao { get; set; } = string.Empty;
        public string MaCanHo { get; set; } = string.Empty;
        public string MaTaiKhoanNguoiBaoCao { get; set; } = string.Empty;
        public string LoaiViPham { get; set; } = string.Empty;
        public string? NoiDung { get; set; }
        public DateTime NgayTao { get; set; }
        public string MaKiemDuyet { get; set; } = "KD002";
    }
}