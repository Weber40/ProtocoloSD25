using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace WavyLauncher
{
    public partial class Form1 : Form
    {
        int aggregatorCount = 0;
        int wavyCount = 0;
        List<Process> wavyProcesses = new List<Process>();

        public Form1()
        {
            InitializeComponent();
            int btnWidth = 240;
            int btnHeight = 60;

            Button btnServer = new Button { Text = "Abrir Server", Left = 10, Top = 10, Width = btnWidth, Height = btnHeight };
            btnServer.Click += (s, e) => StartProcess(@"server\server.csproj");

            Button btnAggregator = new Button { Text = "Novo Aggregator", Left = 10, Top = 20 + btnHeight, Width = btnWidth, Height = btnHeight };
            btnAggregator.Click += (s, e) => {
                aggregatorCount++;
                StartProcess(@"aggregator\aggregator.csproj");
            };

            Button btnWavyAdd = new Button { Text = "Novo WAVY", Left = 10, Top = 30 + 2 * btnHeight, Width = btnWidth, Height = btnHeight };
            btnWavyAdd.Click += (s, e) => {
                wavyCount++;
                var proc = StartProcessWithReturn(@"wavy\wavy.csproj", wavyCount.ToString());
                wavyProcesses.Add(proc);
            };

            Button btnWavyRemove = new Button { Text = "Remover WAVY", Left = 10, Top = 40 + 3 * btnHeight, Width = btnWidth, Height = btnHeight };
            btnWavyRemove.Click += (s, e) => {
                if (wavyProcesses.Count > 0)
                {
                    var last = wavyProcesses[wavyProcesses.Count - 1];
                    try { if (!last.HasExited) last.Kill(); } catch { }
                    wavyProcesses.RemoveAt(wavyProcesses.Count - 1);
                    wavyCount--;
                }
            };

            Controls.Add(btnServer);
            Controls.Add(btnAggregator);
            Controls.Add(btnWavyAdd);
            Controls.Add(btnWavyRemove);
        }

        void StartProcess(string projectPath)
        {
            string root = @"C:\Users\tiago\sd2425parte2\ProtocoloSD25-1\SD25p2\wavy";
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/k dotnet run --project {projectPath}",
                WorkingDirectory = root,
                UseShellExecute = true
            });
        }

        Process StartProcessWithReturn(string projectPath, string args)
        {
            string root = @"C:\Users\tiago\sd2425parte2\ProtocoloSD25-1\SD25p2\wavy";
            return Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/k dotnet run --project {projectPath} {args}",
                WorkingDirectory = root,
                UseShellExecute = true
            });
        }
    }
}
