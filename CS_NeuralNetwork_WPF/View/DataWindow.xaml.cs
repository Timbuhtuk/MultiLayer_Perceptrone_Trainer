using System.Windows;
using CS_NeuralNetwork_WPF.ModelView;

namespace CS_NeuralNetwork_WPF.View
{
    /// <summary>
    /// Логика взаимодействия для DataWindow.xaml
    /// </summary>
    public partial class DataWindow : Window
    {
        public DataWindow(ref CS_NeuralNetwork.Config config)
        {
            InitializeComponent();
            DataContext = new DataModel(ref config);
        }
        public void Update()
        {
            MainWindow? window = Owner as MainWindow;
            DataContext = /*new ViewModelAdd(window.DataContext as ViewModel) { DataWindow = this }*/null;
        }
    }
}
