using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Dash_boardsBIDA
{
    public partial class ThongKe : UserControl
    {
        private DatePicker _dpMonth;
        private Button _btnApply;
        private TextBlock _txtTime;
        private TextBlock _txtFood;
        private TextBlock _txtTotal;
        private DataGrid _dgDaily;

        public ThongKe()
        {
            InitializeComponent();
            Loaded += ThongKe_Loaded;
        }

        private void ThongKe_Loaded(object sender, RoutedEventArgs e)
        {
            _dpMonth = FindName("DpMonth") as DatePicker;
            _btnApply = FindName("BtnApply") as Button;
            _txtTime = FindName("TxtMonthTime") as TextBlock;
            _txtFood = FindName("TxtMonthFood") as TextBlock;
            _txtTotal = FindName("TxtMonthTotal") as TextBlock;
            _dgDaily = FindName("DgDaily") as DataGrid;

            if (_dpMonth != null && _dpMonth.SelectedDate == null)
                _dpMonth.SelectedDate = DateTime.Today;

            if (_btnApply != null)
                _btnApply.Click += (s, ev) => Refresh();

            Refresh();
        }

        private void Refresh()
        {
            DateTime picked = DateTime.Today;
            if (_dpMonth != null && _dpMonth.SelectedDate.HasValue)
                picked = _dpMonth.SelectedDate.Value;

            DateTime first = new DateTime(picked.Year, picked.Month, 1);
            DateTime next = first.AddMonths(1);

            var invoices = InvoiceStore.GetAll()
                .Where(x => x.CreatedAt >= first && x.CreatedAt < next)
                .ToList();

            long sumTime = invoices.Sum(x => (long)x.TimeMoney);
            long sumFood = invoices.Sum(x => (long)x.FoodMoney);
            long sumTotal = sumTime + sumFood;

            if (_txtTime != null) _txtTime.Text = sumTime.ToString("N0");
            if (_txtFood != null) _txtFood.Text = sumFood.ToString("N0");
            if (_txtTotal != null) _txtTotal.Text = sumTotal.ToString("N0");

            var daily = invoices
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new DailyRow
                {
                    Date = g.Key,
                    TimeMoney = g.Sum(i => i.TimeMoney),
                    FoodMoney = g.Sum(i => i.FoodMoney)
                })
                .OrderBy(x => x.Date)
                .ToList();

            if (_dgDaily != null)
                _dgDaily.ItemsSource = new ObservableCollection<DailyRow>(daily);
        }

        private class DailyRow
        {
            public DateTime Date { get; set; }
            public int TimeMoney { get; set; }
            public int FoodMoney { get; set; }
            public int TotalMoney { get { return TimeMoney + FoodMoney; } }
        }
    }
}