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
            btnServer.Click += (s, e) => {
                StartProcess(@"server\server.csproj");
                StartProcess(@"DataAnalysis\DataAnalysis.csproj"); // Inicia DataAnalysis junto com o Server
            };

            Button btnAggregator = new Button { Text = "Novo Aggregator", Left = 10, Top = 20 + btnHeight, Width = btnWidth, Height = btnHeight };
            btnAggregator.Click += (s, e) => {
                aggregatorCount++;
                StartProcess(@"aggregator\aggregator.csproj");
                StartProcess(@"PreprocessingService\PreprocessingService.csproj"); // Inicia PreprocessingService junto com o Aggregator
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

// dotnet run --project WavyLauncher/WavyLauncher.csproj



/*
namespace WavyLauncher
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DateTimePicker dtInicio;
        private System.Windows.Forms.DateTimePicker dtFim;
        private System.Windows.Forms.Label lblMediaTemp;
        private System.Windows.Forms.Label lblMediaPress;
        private System.Windows.Forms.Label lblMaxTemp;
        private System.Windows.Forms.Label lblMinTemp;
        private System.Windows.Forms.Button btnCarregarDados;
        private System.Windows.Forms.Button btnCalcular;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dtInicio = new System.Windows.Forms.DateTimePicker();
            this.dtFim = new System.Windows.Forms.DateTimePicker();
            this.lblMediaTemp = new System.Windows.Forms.Label();
            this.lblMediaPress = new System.Windows.Forms.Label();
            this.lblMaxTemp = new System.Windows.Forms.Label();
            this.lblMinTemp = new System.Windows.Forms.Label();
            this.btnCarregarDados = new System.Windows.Forms.Button();
            this.btnCalcular = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(500, 300);
            this.dataGridView1.TabIndex = 0;
            // 
            // dtInicio
            // 
            this.dtInicio.Location = new System.Drawing.Point(530, 30);
            this.dtInicio.Name = "dtInicio";
            this.dtInicio.Size = new System.Drawing.Size(200, 23);
            this.dtInicio.TabIndex = 1;
            // 
            // dtFim
            // 
            this.dtFim.Location = new System.Drawing.Point(530, 70);
            this.dtFim.Name = "dtFim";
            this.dtFim.Size = new System.Drawing.Size(200, 23);
            this.dtFim.TabIndex = 2;
            // 
            // lblMediaTemp
            // 
            this.lblMediaTemp.AutoSize = true;
            this.lblMediaTemp.Location = new System.Drawing.Point(530, 120);
            this.lblMediaTemp.Name = "lblMediaTemp";
            this.lblMediaTemp.Size = new System.Drawing.Size(120, 15);
            this.lblMediaTemp.TabIndex = 3;
            this.lblMediaTemp.Text = "Média Temperatura:";
            // 
            // lblMediaPress
            // 
            this.lblMediaPress.AutoSize = true;
            this.lblMediaPress.Location = new System.Drawing.Point(530, 150);
            this.lblMediaPress.Name = "lblMediaPress";
            this.lblMediaPress.Size = new System.Drawing.Size(108, 15);
            this.lblMediaPress.TabIndex = 4;
            this.lblMediaPress.Text = "Média Pressão:";
            // 
            // lblMaxTemp
            // 
            this.lblMaxTemp.AutoSize = true;
            this.lblMaxTemp.Location = new System.Drawing.Point(530, 180);
            this.lblMaxTemp.Name = "lblMaxTemp";
            this.lblMaxTemp.Size = new System.Drawing.Size(120, 15);
            this.lblMaxTemp.TabIndex = 5;
            this.lblMaxTemp.Text = "Máx Temperatura:";
            // 
            // lblMinTemp
            // 
            this.lblMinTemp.AutoSize = true;
            this.lblMinTemp.Location = new System.Drawing.Point(530, 210);
            this.lblMinTemp.Name = "lblMinTemp";
            this.lblMinTemp.Size = new System.Drawing.Size(117, 15);
            this.lblMinTemp.TabIndex = 6;
            this.lblMinTemp.Text = "Min Temperatura:";
            // 
            // btnCarregarDados
            // 
            this.btnCarregarDados.Location = new System.Drawing.Point(530, 250);
            this.btnCarregarDados.Name = "btnCarregarDados";
            this.btnCarregarDados.Size = new System.Drawing.Size(120, 23);
            this.btnCarregarDados.TabIndex = 7;
            this.btnCarregarDados.Text = "Carregar Dados";
            this.btnCarregarDados.UseVisualStyleBackColor = true;
            this.btnCarregarDados.Click += new System.EventHandler(this.btnCarregarDados_Click);
            // 
            // btnCalcular
            // 
            this.btnCalcular.Location = new System.Drawing.Point(660, 250);
            this.btnCalcular.Name = "btnCalcular";
            this.btnCalcular.Size = new System.Drawing.Size(70, 23);
            this.btnCalcular.TabIndex = 8;
            this.btnCalcular.Text = "Calcular";
            this.btnCalcular.UseVisualStyleBackColor = true;
            this.btnCalcular.Click += new System.EventHandler(this.btnCalcular_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.dtInicio);
            this.Controls.Add(this.dtFim);
            this.Controls.Add(this.lblMediaTemp);
            this.Controls.Add(this.lblMediaPress);
            this.Controls.Add(this.lblMaxTemp);
            this.Controls.Add(this.lblMinTemp);
            this.Controls.Add(this.btnCarregarDados);
            this.Controls.Add(this.btnCalcular);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
*/