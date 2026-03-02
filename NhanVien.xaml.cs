using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Dash_boardsBIDA
{
    // =========================
    // CODE-BEHIND (1 file)
    // =========================
    public partial class NhanVien : Window
    {
        public NhanVien()
        {
            InitializeComponent();
            DataContext = new EmployeeViewModel();
        }

        private void Exit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Nếu bạn muốn quay về MainWindow:
            // new MainWindow().Show();
            // Close();

            // Hiện tại đóng cửa sổ cho an toàn
            Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đăng xuất!");
            // bạn có thể show LoginWindow rồi Close()
        }
    }

    // =========================
    // MODEL
    // =========================
    public class Employee : INotifyPropertyChanged
    {
        private string _id;
        private string _tenNV;
        private string _vaiTro;
        private string _trangThai;
        private string _loaiNV;
        private string _diaChi;
        private string _sdt;
        private DateTime? _ngaySinh;
        private DateTime? _ngayVaoLam;
        private string _taiKhoan;
        private string _matKhau;

        public string ID { get => _id; set { _id = value; OnPropertyChanged(nameof(ID)); } }
        public string TenNV { get => _tenNV; set { _tenNV = value; OnPropertyChanged(nameof(TenNV)); } }
        public string VaiTro { get => _vaiTro; set { _vaiTro = value; OnPropertyChanged(nameof(VaiTro)); } }
        public string TrangThai { get => _trangThai; set { _trangThai = value; OnPropertyChanged(nameof(TrangThai)); } }
        public string LoaiNV { get => _loaiNV; set { _loaiNV = value; OnPropertyChanged(nameof(LoaiNV)); } }
        public string DiaChi { get => _diaChi; set { _diaChi = value; OnPropertyChanged(nameof(DiaChi)); } }
        public string SDT { get => _sdt; set { _sdt = value; OnPropertyChanged(nameof(SDT)); } }

        public DateTime? NgaySinh { get => _ngaySinh; set { _ngaySinh = value; OnPropertyChanged(nameof(NgaySinh)); } }
        public DateTime? NgayVaoLam { get => _ngayVaoLam; set { _ngayVaoLam = value; OnPropertyChanged(nameof(NgayVaoLam)); } }

        public string TaiKhoan { get => _taiKhoan; set { _taiKhoan = value; OnPropertyChanged(nameof(TaiKhoan)); } }
        public string MatKhau { get => _matKhau; set { _matKhau = value; OnPropertyChanged(nameof(MatKhau)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    // =========================
    // COMMAND
    // =========================
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    // =========================
    // VIEWMODEL
    // =========================
    public class EmployeeViewModel : INotifyPropertyChanged
    {
        public string CurrentUserName { get; set; } = "Hoài Hải";

        public ObservableCollection<Employee> Employees { get; } = new ObservableCollection<Employee>();
        public ObservableCollection<Employee> FilteredEmployees { get; } = new ObservableCollection<Employee>();

        private Employee _selectedEmployee;
        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged(nameof(SelectedEmployee));

                // Nếu user click chọn 1 nhân viên có trong danh sách => thoát chế độ thêm mới
                if (_selectedEmployee != null && Employees.Contains(_selectedEmployee))
                {
                    IsAddMode = false;
                }

                RefreshCommands();
            }
        }

        private bool _isAddMode;
        public bool IsAddMode
        {
            get => _isAddMode;
            set
            {
                _isAddMode = value;
                OnPropertyChanged(nameof(IsAddMode));
                RefreshCommands();
            }
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged(nameof(SearchKeyword));
                ApplyFilter();
            }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelAddCommand { get; }   // (tuỳ chọn) nút Hủy

        public EmployeeViewModel()
        {
            // Demo data
            Employees.Add(new Employee
            {
                ID = "NV001",
                TenNV = "Nguyễn Văn A",
                VaiTro = "Nhân viên",
                TrangThai = "Đang làm",
                LoaiNV = "Full-time",
                DiaChi = "Đà Nẵng",
                SDT = "0901234567",
                NgaySinh = new DateTime(2002, 5, 15),
                NgayVaoLam = DateTime.Today.AddMonths(-5),
                TaiKhoan = "nva",
                MatKhau = "123456"
            });

            Employees.Add(new Employee
            {
                ID = "NV002",
                TenNV = "Trần Thị B",
                VaiTro = "Quản lý",
                TrangThai = "Đang làm",
                LoaiNV = "Full-time",
                DiaChi = "Đà Nẵng",
                SDT = "0908888888",
                NgaySinh = new DateTime(2000, 10, 10),
                NgayVaoLam = DateTime.Today.AddYears(-1),
                TaiKhoan = "ttb",
                MatKhau = "123456"
            });

            AddCommand = new RelayCommand(StartAddEmployee);

            // Không cho xóa khi đang add mode (vì nhân viên nháp chưa nằm trong list)
            DeleteCommand = new RelayCommand(DeleteEmployee, () => SelectedEmployee != null && !IsAddMode);

            // Save được cả khi add mode và khi sửa nhân viên có sẵn
            SaveCommand = new RelayCommand(SaveEmployee, () => SelectedEmployee != null);

            CancelAddCommand = new RelayCommand(CancelAdd, () => IsAddMode);

            ApplyFilter();
            SelectedEmployee = FilteredEmployees.FirstOrDefault();
        }

        // ✅ THAY vì add luôn vào Employees, ta chỉ tạo “nhân viên nháp” để nhập ở form bên phải
        private void StartAddEmployee()
        {
            IsAddMode = true;

            SelectedEmployee = new Employee
            {
                ID = GenerateNextId(),
                TenNV = "",
                VaiTro = "Nhân viên",
                TrangThai = "Đang làm",
                LoaiNV = "Full-time",
                DiaChi = "",
                SDT = "",
                NgaySinh = null,
                NgayVaoLam = DateTime.Today,
                TaiKhoan = "",
                MatKhau = ""
            };

            // KHÔNG gọi ApplyFilter() ở đây vì SelectedEmployee (nháp) không nằm trong list
            RefreshCommands();
        }

        private void CancelAdd()
        {
            IsAddMode = false;
            ApplyFilter();
            SelectedEmployee = FilteredEmployees.FirstOrDefault();
        }

        private void DeleteEmployee()
        {
            if (SelectedEmployee == null) return;

            var ok = MessageBox.Show(
                $"Xóa nhân viên {SelectedEmployee.ID} - {SelectedEmployee.TenNV} ?",
                "Xác nhận",
                MessageBoxButton.OKCancel
            );
            if (ok != MessageBoxResult.OK) return;

            Employees.Remove(SelectedEmployee);
            ApplyFilter();
            SelectedEmployee = FilteredEmployees.FirstOrDefault();
        }

        private void SaveEmployee()
        {
            if (SelectedEmployee == null) return;

            // Validate cơ bản
            if (string.IsNullOrWhiteSpace(SelectedEmployee.TenNV))
            {
                MessageBox.Show("Họ tên không được để trống!");
                return;
            }

            if (!string.IsNullOrWhiteSpace(SelectedEmployee.SDT) &&
                SelectedEmployee.SDT.Any(ch => !char.IsDigit(ch)))
            {
                MessageBox.Show("SĐT chỉ được chứa chữ số!");
                return;
            }

            // ✅ Nếu đang ở chế độ thêm mới => giờ mới add vào list
            if (IsAddMode)
            {
                // Check trùng ID (phòng trường hợp hiếm)
                if (Employees.Any(e => e.ID == SelectedEmployee.ID))
                {
                    SelectedEmployee.ID = GenerateNextId();
                }

                Employees.Add(SelectedEmployee);
                IsAddMode = false;

                ApplyFilter();
                // đảm bảo chọn đúng item vừa thêm
                SelectedEmployee = FilteredEmployees.FirstOrDefault(e => e.ID == Employees.Last().ID);

                MessageBox.Show("Đã thêm nhân viên mới (demo).");
                return;
            }

            // ✅ Còn nếu không add mode => coi như lưu sửa nhân viên đang chọn
            MessageBox.Show("Đã lưu thay đổi (demo).");
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredEmployees.Clear();
            var keyword = (SearchKeyword ?? "").Trim().ToLowerInvariant();

            var query = Employees.AsEnumerable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x =>
                    (x.ID ?? "").ToLowerInvariant().Contains(keyword) ||
                    (x.TenNV ?? "").ToLowerInvariant().Contains(keyword) ||
                    (x.VaiTro ?? "").ToLowerInvariant().Contains(keyword) ||
                    (x.TrangThai ?? "").ToLowerInvariant().Contains(keyword)
                );
            }

            foreach (var item in query) FilteredEmployees.Add(item);

            // ✅ Quan trọng: nếu đang add mode thì ĐỪNG tự đổi SelectedEmployee (vì SelectedEmployee là “nháp”)
            if (!IsAddMode)
            {
                if (SelectedEmployee != null && !FilteredEmployees.Contains(SelectedEmployee))
                    SelectedEmployee = FilteredEmployees.FirstOrDefault();
            }

            RefreshCommands();
        }

        private string GenerateNextId()
        {
            int max = 0;
            foreach (var e in Employees)
            {
                if (string.IsNullOrWhiteSpace(e.ID)) continue;
                if (!e.ID.StartsWith("NV")) continue;

                var digits = new string(e.ID.Skip(2).Where(char.IsDigit).ToArray());
                if (int.TryParse(digits, out int n))
                    if (n > max) max = n;
            }
            return "NV" + (max + 1).ToString("000");
        }

        private void RefreshCommands()
        {
            DeleteCommand?.RaiseCanExecuteChanged();
            SaveCommand?.RaiseCanExecuteChanged();
            CancelAddCommand?.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}