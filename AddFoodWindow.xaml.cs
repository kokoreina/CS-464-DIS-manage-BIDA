using Microsoft.Win32;
using System;
using System.Windows;
using System.Xml.Linq;

namespace Dash_boardsBIDA
{
    public partial class AddFoodWindow : Window
    {
        private string selectedImage = "";

        public Food NewFood { get; private set; }

        public AddFoodWindow()
        {
            InitializeComponent();
        }

        private void BtnChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image|*.png;*.jpg;*.jpeg";

            if (dlg.ShowDialog() == true)
            {
                selectedImage = dlg.FileName;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenMon.Text) ||
                string.IsNullOrWhiteSpace(txtGia.Text))
            {
                MessageBox.Show("Nhập đầy đủ thông tin!");
                return;
            }

            int price;
            if (!int.TryParse(txtGia.Text, out price))
            {
                MessageBox.Show("Giá không hợp lệ!");
                return;
            }

            // ✅ tạo món mới
            NewFood = new Food
            {
                Name = txtTenMon.Text,
                Price = price,
                Image = selectedImage
            };

            DialogResult = true; // trả kết quả về MainWindow
            Close();
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}