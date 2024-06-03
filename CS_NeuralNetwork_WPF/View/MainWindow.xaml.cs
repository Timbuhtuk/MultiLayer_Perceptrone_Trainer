using System.Windows;
using CS_NeuralNetwork_WPF.ModelView;


namespace CS_NeuralNetwork_WPF.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new ViewModel() { MainWindow = this };
            DataContext = vm;
            NetVier1.vm = vm;
        }

    }
}
