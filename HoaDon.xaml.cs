using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dash_boardsBIDA
{
    public partial class HoaDon : UserControl
    {
        private ObservableCollection<Invoice> _view = new ObservableCollection<Invoice>();
        private List<Invoice> _all = new List<Invoice>();

        // UI controls (FindName để tránh lỗi nếu XAML khác tên)
        private DataGrid _dgInvoices;
        private DataGrid _dgItems;
        private TextBox _txtSearch;
        private Button _btnRefresh;

        private TextBlock _txtInvId;
        private TextBlock _txtInvTime;
        private TextBlock _txtInvTable;
        private TextBlock _txtInvUser;
        private TextBlock _txtInvTimeMoney;
        private TextBlock _txtInvFoodMoney;
        private TextBlock _txtInvTotal;

        public HoaDon()
        {
            InitializeComponent();
            Loaded += HoaDon_Loaded;
        }
        private void FixDetailFont(TextBlock tb, double size)
        {
            if (tb == null) return;
            tb.FontSize = size;
            tb.TextWrapping = TextWrapping.Wrap;
        }
        private void HoaDon_Loaded(object sender, RoutedEventArgs e)
        {
            // map UI by name (nếu XAML bạn đúng tên như mình đưa trước thì sẽ bắt được)
            _dgInvoices = FindName("DgInvoices") as DataGrid;
            _dgItems = FindName("DgInvoiceItems") as DataGrid;
            _txtSearch = FindName("TxtSearchInvoice") as TextBox;
            _btnRefresh = FindName("BtnRefresh") as Button;

            _txtInvId = FindName("TxtInvId") as TextBlock;
            _txtInvTime = FindName("TxtInvTime") as TextBlock;
            _txtInvTable = FindName("TxtInvTable") as TextBlock;
            _txtInvUser = FindName("TxtInvUser") as TextBlock;
            _txtInvTimeMoney = FindName("TxtInvTimeMoney") as TextBlock;
            _txtInvFoodMoney = FindName("TxtInvFoodMoney") as TextBlock;
            _txtInvTotal = FindName("TxtInvTotal") as TextBlock;
            // ✅ Fix font quá to
            FixDetailFont(_txtInvId, 14);
            FixDetailFont(_txtInvTime, 14);

            
            if (_btnRefresh != null) _btnRefresh.Click += (s, ev) => Reload();
            if (_dgInvoices != null) _dgInvoices.SelectionChanged += DgInvoices_SelectionChanged;

            if (_txtSearch != null)
            {
                // placeholder đơn giản
                if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                {
                    _txtSearch.Text = "Tìm theo mã / bàn / user...";
                    _txtSearch.Foreground = Brushes.Gray;
                }

                _txtSearch.GotFocus += (s, ev) =>
                {
                    if (_txtSearch.Text == "Tìm theo mã / bàn / user...")
                    {
                        _txtSearch.Text = "";
                        _txtSearch.Foreground = Brushes.Black;
                    }
                };

                _txtSearch.LostFocus += (s, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                    {
                        _txtSearch.Text = "Tìm theo mã / bàn / user...";
                        _txtSearch.Foreground = Brushes.Gray;
                        ApplyFilter("");
                    }
                };

                _txtSearch.TextChanged += (s, ev) =>
                {
                    if (_txtSearch.Text == "Tìm theo mã / bàn / user...") return;
                    ApplyFilter(_txtSearch.Text);
                };
            }

            Reload();
        }

        private void Reload()
        {
            _all = InvoiceStore.GetAll().OrderByDescending(x => x.CreatedAt).ToList();
            ApplyFilter("");
            ClearDetail();
        }

        private void ApplyFilter(string keyword)
        {
            keyword = (keyword ?? "").Trim().ToLowerInvariant();

            IEnumerable<Invoice> q = _all;

            if (!string.IsNullOrEmpty(keyword))
            {
                q = q.Where(x =>
                    Safe(x.InvoiceId).Contains(keyword) ||
                    Safe(x.TableName).Contains(keyword) ||
                    Safe(x.UserEmail).Contains(keyword));
            }

            _view = new ObservableCollection<Invoice>(q);

            if (_dgInvoices != null)
                _dgInvoices.ItemsSource = _view;
        }

        private static string Safe(string s)
        {
            return (s ?? "").ToLowerInvariant();
        }

        private void DgInvoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var inv = (_dgInvoices != null) ? _dgInvoices.SelectedItem as Invoice : null;
            if (inv == null)
            {
                ClearDetail();
                return;
            }

            SetText(_txtInvId, inv.InvoiceId);
            SetText(_txtInvTime, inv.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
            SetText(_txtInvTable, inv.TableName);
            SetText(_txtInvUser, inv.UserEmail);

            if (_dgItems != null)
                _dgItems.ItemsSource = new ObservableCollection<InvoiceItem>(inv.Items ?? new List<InvoiceItem>());

            SetText(_txtInvTimeMoney, inv.TimeMoney.ToString("N0"));
            SetText(_txtInvFoodMoney, inv.FoodMoney.ToString("N0"));
            SetText(_txtInvTotal, inv.TotalMoney.ToString("N0"));
        }

        private void ClearDetail()
        {
            SetText(_txtInvId, "---");
            SetText(_txtInvTime, "---");
            SetText(_txtInvTable, "---");
            SetText(_txtInvUser, "---");

            if (_dgItems != null) _dgItems.ItemsSource = null;

            SetText(_txtInvTimeMoney, "0");
            SetText(_txtInvFoodMoney, "0");
            SetText(_txtInvTotal, "0");
        }

        private static void SetText(TextBlock tb, string text)
        {
            if (tb != null) tb.Text = text ?? "";
        }
    }

    // ===================== MODELS =====================
    public class Invoice
    {
        public string InvoiceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TableName { get; set; }
        public string UserEmail { get; set; }

        public int TimeMoney { get; set; }
        public int FoodMoney { get; set; }

        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

        public int TotalMoney { get { return TimeMoney + FoodMoney; } }
    }

    public class InvoiceItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }

        public int Total { get { return Quantity * Price; } }
    }

    // ===================== STORAGE (TXT) =====================
    // Format:
    // INV|id|ticks|table_b64|user_b64|time|food
    // ITM|id|name_b64|qty|price
    public static class InvoiceStore
    {
        private static readonly string _filePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "Dash_boardsBIDA", "invoices.txt");

        public static string NewId()
        {
            return "HD" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        public static void Add(Invoice inv)
        {
            if (inv == null) return;
            EnsureDir();

            var lines = new List<string>();
            lines.Add(SerializeInvoice(inv));

            if (inv.Items != null)
            {
                foreach (var it in inv.Items)
                    lines.Add(SerializeItem(inv.InvoiceId, it));
            }

            File.AppendAllLines(_filePath, lines, Encoding.UTF8);
        }

        public static List<Invoice> GetAll()
        {
            EnsureDir();

            if (!File.Exists(_filePath))
                return new List<Invoice>();

            var lines = File.ReadAllLines(_filePath, Encoding.UTF8);

            var map = new Dictionary<string, Invoice>();

            foreach (var raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;

                var parts = raw.Split('|');
                if (parts.Length == 0) continue;

                if (parts[0] == "INV" && parts.Length >= 7)
                {
                    var id = parts[1];
                    long ticks;
                    long.TryParse(parts[2], out ticks);

                    var inv = new Invoice
                    {
                        InvoiceId = id,
                        CreatedAt = new DateTime(ticks),
                        TableName = B64Decode(parts[3]),
                        UserEmail = B64Decode(parts[4]),
                        TimeMoney = ParseInt(parts[5]),
                        FoodMoney = ParseInt(parts[6]),
                        Items = new List<InvoiceItem>()
                    };

                    map[id] = inv;
                }
                else if (parts[0] == "ITM" && parts.Length >= 5)
                {
                    var id = parts[1];
                    if (!map.ContainsKey(id)) continue;

                    var item = new InvoiceItem
                    {
                        Name = B64Decode(parts[2]),
                        Quantity = ParseInt(parts[3]),
                        Price = ParseInt(parts[4])
                    };

                    map[id].Items.Add(item);
                }
            }

            return map.Values.ToList();
        }

        private static int ParseInt(string s)
        {
            int x;
            int.TryParse(s, out x);
            return x;
        }

        private static void EnsureDir()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        private static string SerializeInvoice(Invoice inv)
        {
            return string.Join("|",
                "INV",
                inv.InvoiceId ?? "",
                inv.CreatedAt.Ticks.ToString(),
                B64Encode(inv.TableName ?? ""),
                B64Encode(inv.UserEmail ?? ""),
                inv.TimeMoney.ToString(),
                inv.FoodMoney.ToString()
            );
        }

        private static string SerializeItem(string invoiceId, InvoiceItem it)
        {
            return string.Join("|",
                "ITM",
                invoiceId ?? "",
                B64Encode(it.Name ?? ""),
                it.Quantity.ToString(),
                it.Price.ToString()
            );
        }

        private static string B64Encode(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s ?? "");
            return Convert.ToBase64String(bytes);
        }

        private static string B64Decode(string b64)
        {
            try
            {
                var bytes = Convert.FromBase64String(b64 ?? "");
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "";
            }
        }
    }
}