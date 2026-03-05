using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Dash_boardsBIDA
{
    public partial class MainWindow : Window
    {
        // ================= LOGIN =================
        public string CurrentUserEmail = "admin1@gmail.com";

        // ================= TABLE =================
        public static string CurrentTable = "";
        private HoaDon _hoaDonControl;
        private ThongKe _thongKeControl;
        private readonly Dictionary<string, ObservableCollection<OrderItem>> _tableOrders
            = new Dictionary<string, ObservableCollection<OrderItem>>();

        private readonly Dictionary<string, TableSession> _sessions
            = new Dictionary<string, TableSession>();

        private ObservableCollection<OrderItem> _orders = new ObservableCollection<OrderItem>();
        public ObservableCollection<NhanVien> DanhSachNhanVien = new ObservableCollection<NhanVien>();
        private readonly QuanLyBan _quanLyBanControl;
        private readonly DispatcherTimer _uiTimer;

        // ================= FOODS =================
        private readonly List<Food> _menuFoods = new List<Food>();

        public MainWindow()
        {
            InitializeComponent();
            _hoaDonControl = new HoaDon();
            _thongKeControl = new ThongKe();
            // User header
            if (TxtUser != null) TxtUser.Text = CurrentUserEmail;

            // Load table control
            _quanLyBanControl = new QuanLyBan();
            MainContent.Content = _quanLyBanControl;

            // Orders binding
            OrderList.ItemsSource = _orders;

            // Foods
            LoadDefaultFoods();
            DisplayFoods();

            // Timer refresh UI (time money)
            _uiTimer = new DispatcherTimer();
            _uiTimer.Interval = TimeSpan.FromSeconds(1);
            _uiTimer.Tick += (s, e) => RefreshOrderUI();
            _uiTimer.Start();

            Loaded += (s, e) => ShowQuanLyBan();
        }

        // Nếu LoginWindow muốn truyền email
        public MainWindow(string email) : this()
        {
            if (!string.IsNullOrWhiteSpace(email))
                CurrentUserEmail = email;

            if (TxtUser != null) TxtUser.Text = CurrentUserEmail;
        }

        // ================= UI SWITCH =================
        public void ShowMenu()
        {
            MenuPanel.Visibility = Visibility.Visible;
            MainContent.Visibility = Visibility.Collapsed;
        }

        public void ShowQuanLyBan()
        {
            MenuPanel.Visibility = Visibility.Collapsed;
            MainContent.Visibility = Visibility.Visible;
            MainContent.Content = _quanLyBanControl;
        }
        private void OpenHoaDon_Click(object sender, RoutedEventArgs e)
        {
            MenuPanel.Visibility = Visibility.Collapsed;
            MainContent.Visibility = Visibility.Visible;
            MainContent.Content = _hoaDonControl;
        }

        private void OpenThongKe_Click(object sender, RoutedEventArgs e)
        {
            MenuPanel.Visibility = Visibility.Collapsed;
            MainContent.Visibility = Visibility.Visible;
            MainContent.Content = _thongKeControl;
        }
        private void OpenEmployeePage_Click(object sender, RoutedEventArgs e)
        {
            // Nếu NhanVien đang là Window
            var nv = new NhanVien();
            nv.Show();        // hoặc ShowDialog()
        }
        private void OpenMenu_Click(object sender, RoutedEventArgs e) => ShowMenu();
        private void OpenTable_Click(object sender, RoutedEventArgs e) => ShowQuanLyBan();
        private void BackToTable_Click(object sender, RoutedEventArgs e) => ShowQuanLyBan();

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đăng xuất!");
            //Nếu em muốn quay lại LoginWindow:
             new LoginWindow().Show();
            this.Close();
        }

        // ================= SELECT TABLE (QuanLyBan gọi) =================
        public void UpdateTableName(string tableName, string type)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return;

            CurrentTable = tableName;
            if (TxtSelectedTable != null) TxtSelectedTable.Text = tableName;

            EnsureTable(tableName, type);

            _orders = _tableOrders[tableName];
            OrderList.ItemsSource = _orders;

            ShowMenu();
            RefreshOrderUI();
        }

        private void EnsureTable(string tableName, string type)
        {
            if (!_tableOrders.ContainsKey(tableName))
                _tableOrders[tableName] = new ObservableCollection<OrderItem>();

            if (!_sessions.ContainsKey(tableName))
                _sessions[tableName] = new TableSession(type);
            else
                _sessions[tableName].SetType(type);
        }

        // ================= FOODS =================
        private void LoadDefaultFoods()
        {
            _menuFoods.Clear();

            // "Giờ chơi" dùng để mở bàn + tính phí mở bàn 30 phút
            _menuFoods.Add(new Food { Name = "Giờ chơi", Price = 49000, Image = "Images/so9.png" });

            _menuFoods.Add(new Food { Name = "Coca", Price = 10000, Image = "Images/coca.png" });
            _menuFoods.Add(new Food { Name = "Pepsi", Price = 10000, Image = "Images/pepsi.png" });
            _menuFoods.Add(new Food { Name = "Tiger lon", Price = 20000, Image = "Images/tiger.png" });
            _menuFoods.Add(new Food { Name = "Trái cây", Price = 40000, Image = "Images/traicay.png" });
            _menuFoods.Add(new Food { Name = "Khô mực", Price = 50000, Image = "Images/khomuc.png" });
        }

        private void DisplayFoods(List<Food> foods = null)
        {
            if (FoodWrapPanel == null) return;

            FoodWrapPanel.Children.Clear();

            // ⭐ nếu không truyền thì dùng menu gốc
            var showFoods = foods ?? _menuFoods;

            foreach (Food food in showFoods)
            {
                Border card = new Border();
                card.Style = (Style)FindResource("FoodCard");

                StackPanel stack = new StackPanel();

                Image img = new Image
                {
                    Height = 110,
                    Source = new BitmapImage(
                        new Uri(food.Image, UriKind.Relative))
                };

                TextBlock name = new TextBlock
                {
                    Text = food.Name,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                TextBlock price = new TextBlock
                {
                    Text = food.Price.ToString("N0") + " VND",
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Button btn = new Button
                {
                    Content = "ĐẶT",
                    Tag = food.Name + "|" + food.Price,
                    Style = (Style)FindResource("OrderBtn")
                };

                btn.Click += AddOrder_Click;

                stack.Children.Add(img);
                stack.Children.Add(name);
                stack.Children.Add(price);
                stack.Children.Add(btn);

                card.Child = stack;
                FoodWrapPanel.Children.Add(card);
            }
        }
        // ================= SEARCH BOX PLACEHOLDER =================
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = SearchBox.Text.Trim().ToLower();

            // rỗng → hiện lại tất cả
            if (string.IsNullOrWhiteSpace(keyword)
                || keyword == "tìm kiếm món ăn")
            {
                DisplayFoods();
                return;
            }

            // ⭐ lọc món
            var filteredFoods = _menuFoods
                .Where(f => f.Name.ToLower().Contains(keyword))
                .ToList();

            DisplayFoods(filteredFoods);
        }

        // ================= ORDER =================
        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentTable))
            {
                MessageBox.Show("Chưa chọn bàn!");
                return;
            }

            Button btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            string[] data = btn.Tag.ToString().Split('|');
            string name = data[0];
            int price = 0;
            int.TryParse(data[1], out price);

            // ensure table data
            string type = _sessions.ContainsKey(CurrentTable) ? _sessions[CurrentTable].Type : "Pool";
            EnsureTable(CurrentTable, type);

            // ===== "Giờ chơi" = mở bàn + phí mở bàn 30p =====
            if (string.Equals(name, "Giờ chơi", StringComparison.OrdinalIgnoreCase))
            {
                TableSession s = _sessions[CurrentTable];

                if (!s.HasStarted)
                    StartTable(CurrentTable, type);
                else if (s.IsRunning)
                    PauseTable(CurrentTable);
                else
                    ResumeTable(CurrentTable);
                RefreshOrderUI();
                return;
            }

            // ===== món ăn/nước bình thường =====
            ObservableCollection<OrderItem> list = _tableOrders[CurrentTable];

            // Không đụng dòng giờ chơi
            OrderItem exist = list.FirstOrDefault(x => x.Name == name);
            if (exist != null)
                exist.Quantity++;
            else
                list.Add(new OrderItem { Name = name, Quantity = 1, Price = price });

            RefreshOrderUI();
        }

        // ================= TABLE SESSION (TIME) =================
        // ================= TABLE SESSION (TIME) =================

        // QuanLyBan gọi được
        public void StartTable(string tableName, string type)
        {
            EnsureTable(tableName, type);

            TableSession s = _sessions[tableName];

            if (!s.HasStarted)
            {
                s.Start();

                // ✅ bắt đầu từ 0 phút
                s.OpenFeeMinutes = 0;

                EnsureTimeRow(tableName);
            }
        }
        // QuanLyBan gọi được
        public void PauseTable(string tableName)
        {
            if (!_sessions.ContainsKey(tableName)) return;
            _sessions[tableName].Pause();
            RefreshOrderUI();
        }

        // QuanLyBan gọi được
        public void ResumeTable(string tableName)
        {
            if (!_sessions.ContainsKey(tableName)) return;
            _sessions[tableName].Resume();
            RefreshOrderUI();
        }

        // QuanLyBan gọi được
        public void StopTable(string tableName)
        {
            if (!_sessions.ContainsKey(tableName)) return;
            _sessions[tableName].Stop();
            RefreshOrderUI();
        }

        // QuanLyBan gọi để đổi màu + hiện giờ chạy
        public void GetTableDisplay(string tableName, out bool started, out bool running, out bool paused, out TimeSpan playTime)
        {
            started = false; running = false; paused = false; playTime = TimeSpan.Zero;

            if (!_sessions.ContainsKey(tableName)) return;

            TableSession s = _sessions[tableName];
            started = s.HasStarted;
            running = s.IsRunning;
            paused = s.IsPaused;
            playTime = s.GetPlayTime();
        }

        private void StopTableNow(string tableName)
        {
            if (!_sessions.ContainsKey(tableName)) return;
            _sessions[tableName].Stop();
        }

        private void ResetTableAll(string tableName)
        {
            if (_sessions.ContainsKey(tableName))
                _sessions[tableName].Reset();

            if (_tableOrders.ContainsKey(tableName))
                _tableOrders[tableName].Clear();
        }

        private void EnsureTimeRow(string tableName)
        {
            ObservableCollection<OrderItem> list = _tableOrders[tableName];

            // tìm đúng dòng "Giờ chơi"
            OrderItem timeRow = list.FirstOrDefault(x => x.Name == "Giờ chơi");
            if (timeRow == null)
            {
                list.Insert(0, new OrderItem
                {
                    Name = "Giờ chơi",
                    Quantity = 1,
                    Price = 0
                });
            }
        }

        private int GetFoodMoney(string tableName)
        {
            if (!_tableOrders.ContainsKey(tableName)) return 0;

            // loại trừ dòng "Giờ chơi"
            return _tableOrders[tableName]
                .Where(x => x.Name != "Giờ chơi")
                .Sum(x => x.Total);
        }

        private int GetTimeMoney(string tableName)
        {
            if (!_sessions.ContainsKey(tableName)) return 0;
            return _sessions[tableName].GetTimeMoney();
        }

        private void RefreshOrderUI()
        {
            if (string.IsNullOrEmpty(CurrentTable)) return;
            if (!_tableOrders.ContainsKey(CurrentTable)) return;

            // update dòng "Giờ chơi" = tiền bàn hiện tại (nếu đã mở)
            OrderItem timeRow = _tableOrders[CurrentTable].FirstOrDefault(x => x.Name == "Giờ chơi");
            if (timeRow != null)
            {
                int timeMoney = GetTimeMoney(CurrentTable);
                timeRow.Price = timeMoney;   // Quantity=1 => Total = timeMoney
                timeRow.Quantity = 1;
            }

            OrderList.Items.Refresh();

            int total = GetFoodMoney(CurrentTable) + GetTimeMoney(CurrentTable);
            if (TotalText != null) TotalText.Text = total.ToString("N0") + " VND";
        }

        // ================= BUTTON: CLEAR TABLE =================
        private void BtnClearTable_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentTable))
            {
                MessageBox.Show("Chưa chọn bàn!");
                return;
            }

            // Dừng tiền NGAY
            StopTableNow(CurrentTable);

            // Reset hết
            ResetTableAll(CurrentTable);

            RefreshOrderUI();
            MessageBox.Show("Đã xóa bàn và dừng tính tiền!");
        }
        private void RefreshHoaDonThongKe()
        {
            // refresh bằng cách tạo mới control (đơn giản + chắc chắn)
            // vì HoaDon/ThongKe của mình load dữ liệu ở Loaded.
            _hoaDonControl = new HoaDon();
            _thongKeControl = new ThongKe();

            // nếu user đang đứng ở màn nào thì thay luôn để thấy dữ liệu mới
            if (MainContent.Content is HoaDon)
                MainContent.Content = _hoaDonControl;

            if (MainContent.Content is ThongKe)
                MainContent.Content = _thongKeControl;
        }
        // ================= BUTTON: PAY =================
        private void BtnThanhToan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentTable))
            {
                MessageBox.Show("Vui lòng chọn bàn cần thanh toán!");
                return;
            }

            int timeMoney = GetTimeMoney(CurrentTable);
            int foodMoney = GetFoodMoney(CurrentTable);
            int total = timeMoney + foodMoney;

            if (total <= 0)
            {
                MessageBox.Show("Bàn này chưa có dữ liệu thanh toán (chưa mở bàn và chưa order món).");
                return;
            }

            // Dừng tiền NGAY khi bấm thanh toán (đúng yêu cầu)
            StopTableNow(CurrentTable);
            RefreshOrderUI();

            MessageBoxResult rs = MessageBox.Show(
                "Thanh toán cho: " + CurrentTable + "\n\n" +
                "Tiền bàn: " + timeMoney.ToString("N0") + " VND\n" +
                "Tiền đồ ăn/nước: " + foodMoney.ToString("N0") + " VND\n" +
                "----------------------\n" +
                "TỔNG: " + total.ToString("N0") + " VND\n\n" +
                "(Đã dừng tính tiền. OK để chốt & xóa bàn)",
                "Xác nhận thanh toán",
                MessageBoxButton.OKCancel);

            if (rs == MessageBoxResult.OK)
            {
                ResetTableAll(CurrentTable);
                RefreshOrderUI();
                MessageBox.Show("Thanh toán thành công!");
            }
            else
            {
                // Cancel: vẫn dừng (vì em yêu cầu bấm thanh toán là dừng ngay)
            }
            var inv = new Invoice
            {
                InvoiceId = InvoiceStore.NewId(),
                CreatedAt = DateTime.Now,
                TableName = CurrentTable,
                UserEmail = CurrentUserEmail,
                TimeMoney = timeMoney,
                FoodMoney = foodMoney,
                Items = _tableOrders[CurrentTable]
          .Where(x => !x.IsTimeItem)
          .Select(x => new InvoiceItem
          {
              Name = x.Name,
              Quantity = x.Quantity,
              Price = x.Price
          }).ToList()
            };

            // Lưu vào file
            InvoiceStore.Add(inv);

            // ================== RESET BÀN ==================
            if (_sessions.ContainsKey(CurrentTable))
                _sessions[CurrentTable].Reset();

            if (_tableOrders.ContainsKey(CurrentTable))
                _tableOrders[CurrentTable].Clear();

            RefreshOrderUI();
            MessageBox.Show("Thanh toán thành công!");

            // Refresh lại hóa đơn & thống kê nếu đang mở
            RefreshHoaDonThongKe();
        }

        // ================= ADD FOOD WINDOW =================
        private void BtnThemMon_Click(object sender, RoutedEventArgs e)
        {
            AddFoodWindow win = new AddFoodWindow();

            if (win.ShowDialog() == true)
            {
                Food newFood = win.NewFood;

                if (newFood != null)
                {
                    _menuFoods.Add(newFood);

                    // refresh menu
                    DisplayFoods();
                }
            }
        }

        // ================= SEARCH BOX PLACEHOLDER =================
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null && tb.Text == "Tìm kiếm món ăn") tb.Text = "";
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null && string.IsNullOrWhiteSpace(tb.Text)) tb.Text = "Tìm kiếm món ăn";
        }

        // ================= SESSION CLASS =================
        private class TableSession
        {
            public string Type { get; private set; } = "Pool";
            public int RatePerHour { get; private set; } = 50000;

            public bool HasStarted { get; private set; }
            public bool IsRunning { get; private set; }
            public bool IsPaused { get; private set; }

            private DateTime _startTime;
            private DateTime _pauseStart;
            private TimeSpan _pausedTotal = TimeSpan.Zero;

            public int OpenFeeMinutes = 0; // phí mở bàn (phút)

            public TableSession(string type)
            {
                SetType(type);
                Reset();
            }

            public void SetType(string type)
            {
                if (string.IsNullOrWhiteSpace(type))
                    type = "Pool";

                Type = type;

                // ✅ tất cả bàn đều 49k/h
                RatePerHour = 49000;
            }

            public void Start()
            {
                HasStarted = true;
                IsRunning = true;
                IsPaused = false;

                _startTime = DateTime.Now;
                _pausedTotal = TimeSpan.Zero;
            }

            public void Resume()
            {
                if (!HasStarted)
                {
                    Start();
                    return;
                }

                if (IsPaused)
                {
                    TimeSpan paused = DateTime.Now - _pauseStart;
                    _pausedTotal = _pausedTotal + paused;
                }

                IsPaused = false;
                IsRunning = true;
            }

            public void Pause()
            {
                if (!HasStarted) return;
                if (!IsRunning) return;

                IsPaused = true;
                IsRunning = false;
                _pauseStart = DateTime.Now;
            }

            public void Stop()
            {
                if (!HasStarted) return;
                IsRunning = false;
                IsPaused = false;
            }

            public void Reset()
            {
                HasStarted = false;
                IsRunning = false;
                IsPaused = false;
                _pausedTotal = TimeSpan.Zero;
                OpenFeeMinutes = 0;
            }

            public TimeSpan GetPlayTime()
            {
                if (!HasStarted) return TimeSpan.Zero;

                if (IsPaused)
                {
                    return (_pauseStart - _startTime) - _pausedTotal;
                }

                return (DateTime.Now - _startTime) - _pausedTotal;
            }

            public int GetTimeMoney()
            {
                if (!HasStarted) return 0;

                double minutes = GetPlayTime().TotalMinutes + OpenFeeMinutes;
                if (minutes < 0) minutes = 0;

                double money = minutes * (RatePerHour / 60.0);
                return (int)Math.Round(money);
            }
        }
    }
}