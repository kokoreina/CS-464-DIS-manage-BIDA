using System.Windows;

namespace Dash_boardsBIDA
{
    public partial class AddFoodWindow : Window
    {
        public AddFoodWindow()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã thêm món!");
            this.Close();
        }
    }
}