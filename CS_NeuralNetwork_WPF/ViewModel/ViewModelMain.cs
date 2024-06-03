using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CS_NeuralNetwork_WPF.View;
using CS_NeuralNetwork;
using Microsoft.Win32;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace CS_NeuralNetwork_WPF.ModelView
{
    internal class ViewModel : INotifyPropertyChanged
    {
        #region WPF only fields
        System.Windows.Threading.DispatcherTimer Timer;
        
        private MainWindow mainWindow;
        public event PropertyChangedEventHandler? PropertyChanged;
        public MainWindow MainWindow { get { return mainWindow; } set { if (value != mainWindow) { mainWindow = value; } } }
        public bool Can_Run { get { return !Is_Running; } set { NotifyProperyChanged(); } }
        public bool Can_Stop { get { return Is_Running; } set { NotifyProperyChanged(); } }
        private bool can_Learn = true;
        private bool is_Running = false;
        public bool Can_Learn { get { return can_Learn; } set { can_Learn = value; NotifyProperyChanged(); } }
        public bool Is_Running { 
            get { return is_Running; } 
            set { is_Running = value;
                NotifyProperyChanged(); 
                try {
                    DataWindow.Close(); 
                    NetWindow.Close(); 
                    LearnWindow.Close(); 
                } catch { }
            } 
        }
        public string Config_str { get { return config.ToString(); } set { NotifyProperyChanged(); } }

        private ConcurrentDictionary<int,string> output = new ConcurrentDictionary<int, string>();
        public string Output { get { string? str;output.TryGetValue(1, out str); return str; } set { NotifyProperyChanged(); } }

        private DataWindow dataWindow;
        private NetWindow netWindow;
        private LearnWindow learnWindow;
        public DataWindow DataWindow => dataWindow ??= new DataWindow(ref config) { Owner = MainWindow };
        public NetWindow NetWindow => netWindow ??= new NetWindow(ref config) { Owner = MainWindow };
        public LearnWindow LearnWindow => learnWindow ??= new LearnWindow(ref config) { Owner = MainWindow };

        private ICommand? data_Window_Spawn;
        public ICommand? Data_Window_Spawn => data_Window_Spawn ??= new BaseCommand(
                param =>
                {
                    if (!Is_Running) {
                        try
                        {
                            DataWindow.Show();
                            NotifyProperyChanged();
                        }
                        catch
                        {
                            dataWindow = null;
                            DataWindow.Show();
                            NotifyProperyChanged();
                        }
                    }
                }
            );
        private ICommand? net_Window_Spawn;
        public ICommand? Net_Window_Spawn => net_Window_Spawn ??= new BaseCommand(
                param =>
                {
                    if (!Is_Running) {
                        try
                        {
                            NetWindow.Show();
                            NotifyProperyChanged();
                        }
                        catch
                        {
                            netWindow = null;
                            NetWindow.Show();
                            NotifyProperyChanged();
                        } 
                    }
                    
                }
            );
        private ICommand? learn_Window_Spawn;
        public ICommand? Learn_Window_Spawn => learn_Window_Spawn ??= new BaseCommand(
                param =>
                {
                    try
                    {
                        LearnWindow.Show();
                        NotifyProperyChanged();
                    }
                    catch
                    {
                        learnWindow = null;
                        LearnWindow.Show();
                        NotifyProperyChanged();
                    }
                }
            );

        

        #endregion

        #region CS_NeuralNetwork fields
        static string? project_directory;
        public CS_NeuralNetwork.Config config;
        public Net net;
        public Data data;
        Stopwatch stopwatch;
        Task learn_task;
        private CancellationTokenSource? source;
        #endregion
        public ViewModel() {
            Timer = new System.Windows.Threading.DispatcherTimer();
            Timer.Tick += new EventHandler(Timer_Click);
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            Timer.Start();
            stopwatch = new Stopwatch();
            config = new CS_NeuralNetwork.Config();
            project_directory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            
        }

        private void OnClose(object? sender, EventArgs e)
        {
            File.Delete(config.net_save);
        }
        bool asd = true;
        public void Timer_Click(object sender, EventArgs e) {
            if(asd)
                mainWindow.Closed += OnClose;
            asd = false;
            Config_str = "";
            Output = "";
            Can_Learn = true;
            Can_Stop = true;
            Can_Run = true;
            mainWindow.NetVier1.Update();
            mainWindow.NetVier1.InvalidateVisual();
        }

        #region Commands
        private ICommand? run;
        public ICommand? Run => run ??= new BaseCommand(
                param => {
                    source = new CancellationTokenSource();
                    learn_task = Task.Run(()=>Start(source.Token));
                    Is_Running = true;
                }
            );
        private ICommand? run_no_learn;
        public ICommand? Run_no_learn => run_no_learn ??= new BaseCommand(
                param => {
                    source = new CancellationTokenSource();
                    learn_task = Task.Run(()=>StartNoLearn(source.Token));
                    Is_Running = true;
                }
            );
        private ICommand? stop;
        public ICommand? Stop => stop ??= new BaseCommand(
                param => 
                { 
                    {   Is_Running = false;
                        source.Cancel();
                    } 
                }
            );

        private ICommand? save;
        public ICommand? Save => save ??= new BaseCommand(
                param => {
                    var dialog = new SaveFileDialog();
                    dialog.DefaultExt = ".nn_config";
                    dialog.ShowDialog();
                    if(dialog.FileName!= null && dialog.FileName != "")
                        config.JsonSave(dialog.FileName);
                }
            );
        private ICommand? load;
        public ICommand? Load => load ??= new BaseCommand(
                param =>
                {
                    var dialog = new OpenFileDialog();
                    dialog.Filter = "txt files (*.nn_config)|*.nn_config|All files (*.*)|*.*";
                    dialog.ShowDialog();
                    if (dialog.FileName != null && dialog.FileName != ""){
                        config.JsonLoad(dialog.FileName);
                         int a = 0;}
                    
                
                }
            );
        #endregion
        public void NotifyProperyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Start(CancellationToken token) {

            if (config.multithread)
                net = new Net_Threading(ref config);
            else
                net = new Net(ref config);


            double lowMSE = 0.25, MSE = 1.0, total_learn = 0.0;
            int q = 0, last_good_learn = 0, e = 0;

            net.JsonLoad(config.net_save);
            data = new Data(config);


            Is_Running = true;
            while (MSE > config.stop_learn && !token.IsCancellationRequested)
            {
                
                var result = new StringBuilder();
                stopwatch.Start();


                var answer = data.Answers;
                var answertest = data.AnswersTest;
                //var answer = CS_NeuralNetwork.DataFormat.ToBinary(data.Answers);
                //var answertest = CS_NeuralNetwork.DataFormat.ToBinary(data.AnswersTest);
                var input = data.Inputs;
                var inputtest = data.InputsTest;

                switch (config.use_input_transform)
                {
                    case "to_one":
                        input = CS_NeuralNetwork.DataFormat.ToOne(input);
                        inputtest = CS_NeuralNetwork.DataFormat.ToOne(inputtest);
                        break;
                    case "to_binary":
                        input = CS_NeuralNetwork.DataFormat.ToBinary(input);
                        inputtest = CS_NeuralNetwork.DataFormat.ToBinary(inputtest);
                        break;
                    case "scaling":
                        input = CS_NeuralNetwork.DataFormat.Scaling(input);
                        inputtest = CS_NeuralNetwork.DataFormat.Scaling(inputtest);
                        break;
                    default:
                        break;
                }




                    if(net is Net_Threading Net)
                            MSE = Net.Learn(input, answer, config.epoch, config.threads, token)[0];
                    else
                    {
                        if (config.stochastic)
                            MSE = net.Learn(input, answer, config.epoch, 1000)[0];
                        else
                            MSE = net.Learn(input, answer, config.epoch)[0];
                    }


                var run = Test(inputtest, answertest);
                var run2 = Test(input, answer);
                output.Clear();
                result.AppendLine("N." + q);
                result.AppendLine("Elapsed Time - " + stopwatch.Elapsed.ToString());
                stopwatch.Reset();
                net.JsonSave(config.net_save);

                var coefs_task = GetActualCoefs("C:\\Users\\timpf\\source\\repos\\CSNeuralNet-master\\ACTUAL_COEFS.json");

                if (lowMSE - MSE > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    result.AppendLine("MidSqrEr - " + MSE);
                    result.AppendLine("         - " + (lowMSE - MSE));
                    result.AppendLine("         - " + total_learn);
                    Console.ForegroundColor = ConsoleColor.White;
                    total_learn += lowMSE - MSE;
                    lowMSE = MSE; last_good_learn = q;
                }
                else
                {
                    result.AppendLine("MidSqrEr - " + MSE);
                }

                if (Math.Abs(last_good_learn - q) > 1) { 
                    config.lr-=config.lr*0.5;
                }
                if (config.lr < 0.015) {
                    config.lr = 0.01;
                }
                if (last_good_learn == q)
                {
                    config.lr += config.lr * 0.02;
                }
                //if (config.lr > 1)
                //{
                //    config.lr = 1;
                //}

                result.AppendLine("last good learn N." + last_good_learn);
                result.Append(run.Result);
                result.Append(run2.Result);


                if (!output.TryAdd(1, result.ToString())) {
                    output.TryUpdate(1, result.ToString(), "");
                }
                q++;
                if (!Is_Running)
                {
                    break;
                }

            }
        }
        public void StartNoLearn(CancellationToken token) {
           
            net = new Net(ref config);

            int q = 0, e = 0;
            net.JsonLoad(config.net_save);
            data = new Data(config);


            Is_Running = true;
            while (Is_Running && !token.IsCancellationRequested)
            {
                var result = new StringBuilder();
                
                //var input = CS_NeuralNetwork.DataFormat.ToBinary(data.Inputs);
                var input = data.Inputs;
                var answer = CS_NeuralNetwork.DataFormat.ToBinary(data.Answers);
                var run = Test(CS_NeuralNetwork.DataFormat.ToBinary(data.InputsTest), CS_NeuralNetwork.DataFormat.ToBinary(data.AnswersTest));
                var run2 = Test(input, answer);
                output.Clear();
                var coefs_task = GetActualCoefs("C:\\Users\\timpf\\source\\repos\\CSNeuralNet-master\\ACTUAL_COEFS.json");
                result.Append(run.Result);
                result.Append(run2.Result);
                var coefs = (coefs_task.Result).GetRange(0, input[0].Count);
                coefs.Reverse();
                result.AppendLine(net.FeedForward(CS_NeuralNetwork.DataFormat.ToBinary(coefs.ToArray()))[0] + " " + coefs.Last());

                if (!output.TryAdd(1, result.ToString()))
                {
                    output.TryUpdate(1, result.ToString(), "");
                }
                q++;

                Thread.Sleep(1000);
            }
        }
        async public Task<string> Test(List<List<double>> inputs, List<List<double>> answers)
        {
            var plus = 0;
            var minus = 0;
            var skip1 = 0;
            var skip0 = 0;
            
            var result = new StringBuilder();
            var tes = new StringBuilder();
            for (int q = 0; q < inputs.Count; q++)
            {
                var ff = net.FeedForward(inputs[q]);
                if (ff[0] > 0.5 && answers[q][0] == 1.0) { plus++; }
                else if (ff[0] > 0.5 && answers[q][0] == 0.0) { minus++; }
                else if (ff[0] < 0.5 && answers[q][0] == 1.0) { skip0++; }
                else if (ff[0] < 0.5 && answers[q][0] == 0.0) { skip1++; }
                //tes.AppendLine(ff[0] +" "+ answers[q][0]);
            }
            result.AppendLine("correct 'yes' " + plus);
            result.AppendLine("incorrect 'yes' " + minus);
            result.AppendLine("correct 'no' " + skip1);
            result.AppendLine("incorrect 'no' " + skip0);
            result.AppendLine("winrate " + ((plus + skip1) / (double)answers.Count));
            //result.AppendLine(tes.ToString());
            return result.ToString();

        }
        static async public Task<List<double>> GetActualCoefs(string file)
        {
            for(int q =0;q<10;q++) {
                try
                {
                    List<double> result = new List<double>();
                    using (StreamReader reader = new StreamReader(file))
                    {
                        var row = reader.ReadToEnd();
                        if (row != null && row != "")
                        {
                            result = JsonConvert.DeserializeObject<List<double>>(row);
                        }
                        else { throw new Exception("Bad coefs Save"); }
                    }
                    return result.ToList();
                }
                catch (Exception e) { Console.WriteLine(e); /*System.Environment.Exit(0);*/ }
                Thread.Sleep(400);
            }
            return null; 

        }
    }
    internal class BaseCommand : ICommand
        {
            Predicate<object>? canExecuteMethod;
            Action<object>? executeMethod;
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return canExecuteMethod == null || canExecuteMethod(parameter);
            }
            public BaseCommand(Action<object>? executeMethod, Predicate<object>? canExecuteMethod = null)
            {
                this.canExecuteMethod = canExecuteMethod;
                this.executeMethod = executeMethod;
            }


            public void Execute(object? parameter)
            {
                executeMethod?.Invoke(parameter);
            }
        }
}
