using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CS_NeuralNetwork;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace CS_NeuralNetwork_WPF.ModelView
{
    class DataModel : INotifyPropertyChanged
    {
        public DataModel(ref Config config/*,ref Action NPC*/) {
            this.Config = config;
            NOne_file_creation = !One_file_creation;
            data_transforms = new ObservableCollection<string> { "nothing","to_one","to_binary","scaling"};
        }
        public Config Config { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;


        public ObservableCollection<string> data_transforms { get; set; }
        public string ComboBoxItem { get { return Config.use_input_transform; } set { Config.use_input_transform = value; NotifyProperyChanged(); } }



        public string InputsTest_file { get { return Config.inputs_test_filename; } set { Config.inputs_test_filename = value; NotifyProperyChanged(); } }
        public string AnswersTest_file { get { return Config.answers_test_filename; } set { Config.answers_test_filename = value; NotifyProperyChanged(); } }
        public string Inputs_file { get { return Config.inputs_filename; } set { Config.inputs_filename = value; NotifyProperyChanged(); } }
        public string Answers_file { get { return Config.answers_filename; } set { Config.answers_filename = value; NotifyProperyChanged(); } }
        
        
        public bool One_file_creation { get { return Config.one_file_creation; } set { Config.one_file_creation = value; NOne_file_creation = !value; NotifyProperyChanged(); } }
        private bool nOne_file_creation;
        public bool NOne_file_creation { get { return nOne_file_creation; } set { nOne_file_creation = value; NotifyProperyChanged(); } }
        
        
        private ICommand? inputs_file_brows;
        public ICommand? Inputs_file_brows => inputs_file_brows ??= new BaseCommand(
                param =>
                {
                    Inputs_file = getFilename(Inputs_file);
                }
            );
        private ICommand? inputsTest_file_brows;
        public ICommand? InputsTest_file_brows => inputsTest_file_brows ??= new BaseCommand(
                param =>
                {
                    InputsTest_file = getFilename(InputsTest_file);
                }
            );
        private ICommand? answers_file_brows;
        public ICommand? Answers_file_brows => answers_file_brows ??= new BaseCommand(
                param =>
                {
                    Answers_file = getFilename(Answers_file);
                }
            );
        private ICommand? answersTest_file_brows;
        public ICommand? AnswersTest => answersTest_file_brows ??= new BaseCommand(
                param =>
                {
                    AnswersTest_file = getFilename(AnswersTest_file);
                }
            );
        private string getFilename(string file)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt files (*.json)|*.json|All files (*.*)|*.*";
            dialog.ShowDialog();
            if (dialog.FileName != null && dialog.FileName != "")
            {
                return dialog.FileName;
            }
            return file;
        }
        private void NotifyProperyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
