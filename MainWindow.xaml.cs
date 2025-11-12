using System.Windows;
using OrderViewApp.ViewModels;

namespace OrderViewApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // ViewModelをDataContextに設定
            DataContext = new MainViewModel();
            
            // 画面表示後にデータを読み込む
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 画面が表示された後にデータを読み込む
            if (DataContext is MainViewModel viewModel)
            {
                await viewModel.LoadOrdersAsync();
            }
        }
    }
}