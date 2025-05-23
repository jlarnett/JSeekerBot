using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NUnit.Framework.Api;
using Tensorflow;
using Path = System.IO.Path;

namespace JSeekerBot.UI
{
    /// <summary>
    /// Interaction logic for RunForm.xaml
    /// </summary>
    public partial class RunControl : UserControl
    {
        private Mutex accessingData;

        public RunControl()
        {
            accessingData = new Mutex(false);
            InitializeComponent();
            //  DispatcherTimer setup
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(5);
            dispatcherTimer.Start();
        }

        //  System.Windows.Threading.DispatcherTimer.Tick handler
        //  Updates the current seconds display and calls
        //  InvalidateRequerySuggested on the CommandManager to force 
        //  the Command to raise the CanExecuteChanged event.
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Updating the Label which displays the current second
            LoadBotData();
            CommandManager.InvalidateRequerySuggested();
        }

        private async void RefreshBotData()
        {
            await RunInBackground(TimeSpan.FromSeconds(5), LoadBotData);
        }
        
        private async void LoadBotData()
        {
            var botData = new BotData();

            //Load Settings File

            accessingData.WaitOne();

            if (File.Exists(Directory.GetCurrentDirectory() + "\\BotData.json"))
            {
                using (StreamReader reader = new StreamReader("BotData.json"))
                {
                    string fileJson = reader.ReadToEnd();
                    botData = JsonSerializer.Deserialize<BotData>(fileJson);
                }
            }

            accessingData.ReleaseMutex();

            if (ApplicationSubmittedCounterText.Text != botData.ApplicationSubmitted || JobProcessCounterText.Text != botData.JobsProcessed)
            {
                //The counters don't match what is stored in the botData file. Meaning run just finished or app starting
                RunExecutingLoadingIcon.Visibility = Visibility.Hidden;
                ApplicationSubmittedCounterText.Text = botData.ApplicationSubmitted;
                JobProcessCounterText.Text = botData.JobsProcessed;
            }
        }

        async Task RunInBackground(TimeSpan timeSpan, Action action)
        {
            var periodicTimer = new PeriodicTimer(timeSpan);
            while (await periodicTimer.WaitForNextTickAsync())
            {
                action();
            }
        }

        private void StandardOutputReceiver(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Receives the child process' standard output
            if (! string.IsNullOrEmpty(outLine.Data))
            {

                var botData = new BotData();

                //Load Settings File
                accessingData.WaitOne();

                var path = Directory.GetCurrentDirectory() + "\\BotData.json";

                if (File.Exists(path))
                {
                    using StreamReader reader = new StreamReader("BotData.json");
                    string fileJson = reader.ReadToEnd();
                    botData = JsonSerializer.Deserialize<BotData>(fileJson);
                }

                if (outLine.Data.Contains("Jobs Processed - "))
                {
                    var jobs = outLine.Data.Split("Jobs Processed - ");
                    botData.JobsProcessed = jobs[1];

                    var json = JsonSerializer.Serialize(botData);
                    File.WriteAllText("BotData.json",json);

                }

                if (outLine.Data.Contains("Applications Submitted - "))
                {
                    var jobs = outLine.Data.Split("Applications Submitted - ");
                    botData.ApplicationSubmitted = jobs[1];

                    var json = JsonSerializer.Serialize(botData);
                    File.WriteAllText("BotData.json",json);
                }

                accessingData.ReleaseMutex();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var ps1File = @$"{currentDirectory}\runtests.ps1";
            var startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy ByPass -File \"{ps1File}\"",
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden
                
            };

            var process = Process.Start(startInfo);

            if (process != null)
            {
                RunExecutingLoadingIcon.Visibility = Visibility.Visible;
                process.EnableRaisingEvents = true;
                process.BeginOutputReadLine();
                process.OutputDataReceived += new DataReceivedEventHandler(StandardOutputReceiver);
                process.ErrorDataReceived += ProcessOnErrorDataReceived;
            }

            LoadBotData();
        }

        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            // Receives the child process' standard output
            if (! string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine(e.Data);
            }
        }
    }

    public struct BotData()
    {
        public string JobsProcessed { get; set; } = "0";
        public string ApplicationSubmitted { get; set; } = "0";
    }

    public static class WindowsCmdCommand
    {
        public static void Run(string command, out string output, out string error, string directory = null)
        {
            using Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    Arguments = "/c " + command,
                    CreateNoWindow = true,
                    WorkingDirectory = directory ?? string.Empty
                }
            };
            process.Start();
            process.WaitForExit();
            output = process.StandardOutput.ReadToEnd();
            error = process.StandardError.ReadToEnd();
        }
    }
}
