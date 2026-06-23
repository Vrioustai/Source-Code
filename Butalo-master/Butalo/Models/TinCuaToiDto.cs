namespace Butalo.Models;

public class TinCuaToiDto
{
    public string MaCanHo { get; set; } = string.Empty;
    public string TenCanHo { get; set; } = string.Empty;
    public double GiaCanHo { get; set; }
    public double DienTich { get; set; }
    public DateTime? NgayDang { get; set; }
    public string TrangThaiDuyet { get; set; } = string.Empty;
    public bool TrangThaiHienThi { get; set; }
}

