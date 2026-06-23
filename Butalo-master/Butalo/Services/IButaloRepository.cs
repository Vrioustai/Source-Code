using Butalo.Models;

namespace Butalo.Services;

public interface IButaloRepository
{
    Task<List<CanHoItemViewModel>> LayDanhSachPhongAsync(int soLuong = 30);
    Task<List<CanHoItemViewModel>> TimKiemPhongAsync(TimKiemCanHoViewModel boLoc, int soLuong = 200);
    Task<ChiTietCanHoViewModel?> LayChiTietPhongAsync(string MaCanHo);
    Task<ChiTietCanHoViewModel?> LayChiTietPhongChoPhepAsync(string MaCanHo, string? maTaiKhoan, bool laQuanTri);
    Task<string> TaoTaiKhoanAsync(DangKyTaiKhoanViewModel model); 
    Task<bool> KiemTraDangNhapAsync(DangNhapViewModel model);
    Task<TaiKhoanDto?> LayTaiKhoanTheoDangNhapAsync(DangNhapViewModel model);
    Task<TaiKhoanDto?> LayTaiKhoanTheoMaAsync(string maTaiKhoan);
    Task CapNhatThongTinTaiKhoanAsync(CapNhatTaiKhoanViewModel model);
    Task<List<TaiKhoanDto>> LayDanhSachTaiKhoanAsync();
    Task CapNhatTaiKhoanBoiQuanTriAsync(CapNhatTaiKhoanQuanTriViewModel model);
    Task CapLaiMatKhauAsync(string maTaiKhoan, string matKhauMoi);

    Task<List<LoaiCanHoDto>> LayLoaiCanHoAsync();
    Task<List<TinhThanhPhoDto>> LayTinhThanhPhoAsync();
    Task<List<QuanHuyenDto>> LayQuanHuyenAsync(string maTinhThanhPho);
    Task<List<XaPhuongDto>> LayXaPhuongAsync(string maQuanHuyen);

    Task<string> TaoTinPhongAsync(TaoTinCanHoViewModel model);
    Task<TaoTinCanHoViewModel?> LayTinPhongDeSuaAsync(string MaCanHo);
    Task CapNhatTinPhongAsync(TaoTinCanHoViewModel model);
    Task<List<TinCuaToiDto>> LayDanhSachTinCuaToiAsync(string maTaiKhoan);
    Task<List<TinChoDuyetDto>> LayDanhSachTinChoDuyetAsync();
    Task CapNhatTrangThaiDuyetTinAsync(string MaCanHo, string maKiemDuyet, bool trangThaiHoatDong);

    Task<string> TaoToCaoAsync(TaoToCaoViewModel model);
    Task<List<ToCaoChoDuyetDto>> LayDanhSachToCaoChoDuyetAsync();
    Task<List<ToCaoChoDuyetDto>> LayDanhSachToCaoCuaToiAsync(string maTaiKhoan);
    Task CapNhatTrangThaiDuyetToCaoAsync(string maToCao, string maKiemDuyet);
    Task XoaToCaoAsync(string maToCao);
    Task XoaPhongAsync(string MaCanHo);

    Task XoaTaiKhoanAsync(string maTaiKhoan);

    Task<string> UploadAnhAsync(IFormFile file, IWebHostEnvironment env);
    Task LuuAnhVaoDbAsync(string MaCanHo, string maTaiKhoan, string duongDan);
}
