using System.Windows;
using CS_NeuralNetwork_WPF.ModelView;

namespace CS_NeuralNetwork_WPF.View
{
    /// <summary>
    /// Логика взаимодействия для LearnWindow.xaml
    /// </summary>
    public partial class LearnWindow : Window
    {
        public LearnWindow(ref CS_NeuralNetwork.Config config)
        {
            InitializeComponent();
            DataContext = new LearnModel(ref config) {Owner = this.Owner };
        }
    }
}
