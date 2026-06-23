namespace Butalo.Models;

public class CanHoItemViewModel
{
    public string MaCanHo { get; set; } = string.Empty;
    public string TenPhong { get; set; } = string.Empty;
    public double GiaCanHo { get; set; }
    public double DienTich { get; set; }
    public string DiaChiDayDu { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string? DuongDanAnh { get; set; }
}
