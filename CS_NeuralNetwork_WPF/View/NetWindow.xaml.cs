using System.Windows;
using CS_NeuralNetwork_WPF.ModelView;


namespace CS_NeuralNetwork_WPF.View
{
    /// <summary>
    /// Логика взаимодействия для NetWindow.xaml
    /// </summary>
    public partial class NetWindow : Window
    {

        public NetWindow(ref CS_NeuralNetwork.Config config)
        {
            InitializeComponent();
            DataContext = new NetModel(ref config);
        }

    }
}
