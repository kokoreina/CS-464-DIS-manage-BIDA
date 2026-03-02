using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Dash_boardsBIDA
{
    public partial class QuanLyBan : UserControl
    {
        private DispatcherTimer _uiTimer;

        public QuanLyBan()
        {
            InitializeComponent();

            _uiTimer = new DispatcherTimer();
            _uiTimer.Interval = TimeSpan.FromSeconds(1);
            _uiTimer.Tick += UiTimer_Tick;
            _uiTimer.Start();
        }

        private MainWindow GetMain()
        {
            return Window.GetWindow(this) as MainWindow;
        }

        private void Ban_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            string tableName = GetTableName(btn);
            string type = (btn.Tag == null) ? "Pool" : btn.Tag.ToString();

            MainWindow main = GetMain();
            if (main == null) return;

            main.UpdateTableName(tableName, type);
        }

        private void BatDau_Click(object sender, RoutedEventArgs e)
        {
            Button btn = GetButtonFromMenu(sender);
            if (btn == null) return;

            MainWindow main = GetMain();
            if (main == null) return;

            main.StartTable(GetTableName(btn), btn.Tag == null ? "Pool" : btn.Tag.ToString());
        }

        private void TamDung_Click(object sender, RoutedEventArgs e)
        {
            Button btn = GetButtonFromMenu(sender);
            if (btn == null) return;

            MainWindow main = GetMain();
            if (main == null) return;

            main.PauseTable(GetTableName(btn));
        }

        private void TiepTuc_Click(object sender, RoutedEventArgs e)
        {
            Button btn = GetButtonFromMenu(sender);
            if (btn == null) return;

            MainWindow main = GetMain();
            if (main == null) return;

            main.ResumeTable(GetTableName(btn));
        }

        private void KetThuc_Click(object sender, RoutedEventArgs e)
        {
            Button btn = GetButtonFromMenu(sender);
            if (btn == null) return;

            MainWindow main = GetMain();
            if (main == null) return;

            main.StopTable(GetTableName(btn));
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            MainWindow main = GetMain();
            if (main == null) return;

            foreach (var child in TableGrid.Children)
            {
                Button btn = child as Button;
                if (btn == null) continue;

                string tableName = GetTableName(btn);
                string type = (btn.Tag == null) ? "Pool" : btn.Tag.ToString();

                bool started, running, paused;
                TimeSpan playTime;
                main.GetTableDisplay(tableName, out started, out running, out paused, out playTime);

                // ✅ pending = bàn có phát sinh tiền (món hoặc giờ) nhưng chưa thanh toán/xóa
                bool pending = main.HasPendingPayment(tableName);

                if (paused) btn.Background = Brushes.Yellow;
                else if (pending) btn.Background = Brushes.Orange;
                else btn.Background = Brushes.LightGreen;

                // ✅ luôn giữ POOL/LIBRE trên UI
                if (!started)
                {
                    btn.Content = tableName + "\n" + type.ToUpper();
                }
                else
                {
                    btn.Content = tableName + "\n" + type.ToUpper() + "\n" + playTime.ToString(@"hh\:mm\:ss");
                }

                // ✅ ưu tiên màu pause trước
                if (paused)
                {
                    btn.Background = Brushes.Yellow;
                }
                else if (pending)
                {
                    // ✅ đúng yêu cầu: bàn đã chọn/đã có dữ liệu chưa thanh toán => CAM
                    btn.Background = Brushes.Orange;
                }
                else
                {
                    // mặc định xanh chuối
                    btn.Background = Brushes.LightGreen;
                }
            }
        }

        private Button GetButtonFromMenu(object sender)
        {
            MenuItem mi = sender as MenuItem;
            if (mi == null) return null;

            ContextMenu cm = mi.Parent as ContextMenu;
            if (cm == null) return null;

            return cm.PlacementTarget as Button;
        }

        private string GetTableName(Button btn)
        {
            if (btn?.Content == null) return "Bàn";
            return btn.Content.ToString()
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0]
                .Trim();
        }
    }
}