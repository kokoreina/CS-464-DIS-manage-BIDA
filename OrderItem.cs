using System.ComponentModel;

namespace Dash_boardsBIDA
{
    public class OrderItem : INotifyPropertyChanged
    {
        private int _quantity;
        private int _price;
        private bool _isTimeItem;

        public string Name { get; set; }

        public int Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged("Price");
                OnPropertyChanged("Total");
            }
        }

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged("Quantity");
                OnPropertyChanged("Total");
            }
        }

        // ✅ FIX LỖI: thêm field này để phân biệt "Giờ chơi" với món ăn
        public bool IsTimeItem
        {
            get { return _isTimeItem; }
            set
            {
                _isTimeItem = value;
                OnPropertyChanged("IsTimeItem");
            }
        }

        public int Total
        {
            get { return Price * Quantity; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }
    }
}