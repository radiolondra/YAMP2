using Avalonia.Controls;
using YAMP2.ViewModels;

namespace YAMP2.Views
{
    public partial class MainWindow : Window
    {
        private static MainWindow _this;

        MainWindowViewModel viewModel = new MainWindowViewModel();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
            _this = this;

            if (!Avalonia.Controls.Design.IsDesignMode)
            {
                Opened += MainWindow_Opened;
            }
        }

        public static MainWindow GetInstance()
        {
            return _this;
        }

        private void MainWindow_Opened(object? sender, System.EventArgs e)
        {
            var tmp = VideoPlayerViewControl.GetInstance();
            tmp.SetPlayerHandle();

        }
    }
}
