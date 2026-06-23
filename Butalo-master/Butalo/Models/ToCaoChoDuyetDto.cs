namespace Butalo.Models;

public class ToCaoChoDuyetDto
{
    public string MaToCao { get; set; } = string.Empty;
    public string MaCanHo { get; set; } = string.Empty;
    public string TenCanHo { get; set; } = string.Empty;
    public string MaTaiKhoanNguoiBaoCao { get; set; } = string.Empty;
    public string LoaiViPham { get; set; } = string.Empty;
    public string? NoiDung { get; set; }
    public DateTime? NgayTao { get; set; }
    public string TrangThaiDuyet { get; set; } = string.Empty;
}

