namespace Butalo.Models;

public class TinChoDuyetDto
{
    public string MaCanHo { get; set; } = string.Empty;
    public string TenCanHo { get; set; } = string.Empty;
    public string MaTaiKhoan { get; set; } = string.Empty;
    public string TenChuTro { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public double GiaCanHo { get; set; }
    public double DienTich { get; set; }
    public DateTime? NgayDang { get; set; }
    public string TrangThaiDuyet { get; set; } = string.Empty;
}

