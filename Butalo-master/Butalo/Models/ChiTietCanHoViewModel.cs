namespace Butalo.Models;

public class ChiTietCanHoViewModel : CanHoItemViewModel
{
    public string LoaiCanHo { get; set; } = string.Empty;
    public double GiaDien { get; set; }
    public double GiaNuoc { get; set; }
    public DateTime? NgayDang { get; set; }
}
