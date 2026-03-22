using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;

namespace PortoesApp
{
    public class Form1 : Form
    {
        private SerialManager _serialManager;
        private List<GateConfig> _gateConfigs;
        private Button[] _gateButtons;

        private ToolStripComboBox cmbPorts;
        private ToolStripComboBox cmbBaudRate;
        private ToolStripMenuItem btnConnect;
        private ToolStripMenuItem btnRefreshPorts;
        private ToolStripStatusLabel lblStatus;
        private TableLayoutPanel tlpGates;

        public Form1()
        {
            InitializeComponent();
            _serialManager = new SerialManager();
            this.FormClosing += Form1_FormClosing;
            this.Load += Form1_Load;
        }

        private void InitializeComponent()
        {
            this.Text = "Painel de Controle de Portões - Arduino Mega";
            this.Size = new Size(950, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 300);

            // MenuStrip (Menu Superior)
            MenuStrip menu = new MenuStrip();
            ToolStripMenuItem configMenu = new ToolStripMenuItem("Configurações");
            
            ToolStripMenuItem portMenu = new ToolStripMenuItem("Porta COM");
            cmbPorts = new ToolStripComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
            portMenu.DropDownItems.Add(cmbPorts);

            ToolStripMenuItem baudMenu = new ToolStripMenuItem("Baud Rate");
            cmbBaudRate = new ToolStripComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbBaudRate.Items.AddRange(new object[] { "9600", "19200", "38400", "57600", "115200" });
            cmbBaudRate.SelectedItem = "9600";
            baudMenu.DropDownItems.Add(cmbBaudRate);

            btnRefreshPorts = new ToolStripMenuItem("Atualizar Portas");
            btnRefreshPorts.Click += (s, e) => LoadPorts();

            btnConnect = new ToolStripMenuItem("Conectar");
            btnConnect.Click += BtnConnect_Click;

            configMenu.DropDownItems.Add(portMenu);
            configMenu.DropDownItems.Add(baudMenu);
            configMenu.DropDownItems.Add(new ToolStripSeparator());
            configMenu.DropDownItems.Add(btnRefreshPorts);
            configMenu.DropDownItems.Add(btnConnect);
            
            menu.Items.Add(configMenu);
            this.MainMenuStrip = menu;

            // StatusStrip (Rodapé)
            StatusStrip statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("Desconectado");
            lblStatus.ForeColor = Color.Red;
            lblStatus.Font = new Font(this.Font, FontStyle.Bold);
            statusStrip.Items.Add(lblStatus);

            // Tabela de matriz (Grid 8x2) de botões dinâmicos
            tlpGates = new TableLayoutPanel();
            tlpGates.Dock = DockStyle.Fill;
            tlpGates.ColumnCount = 8;
            tlpGates.RowCount = 2;
            tlpGates.Padding = new Padding(10);
            for (int i = 0; i < 8; i++) tlpGates.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5f));
            for (int i = 0; i < 2; i++) tlpGates.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            this.Controls.Add(tlpGates);
            this.Controls.Add(statusStrip);
            this.Controls.Add(menu);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadPorts();
            
            var serialCfg = ConfigManager.LoadSerialConfig();
            if (cmbPorts.Items.Contains(serialCfg.Item1)) cmbPorts.SelectedItem = serialCfg.Item1;
            if (cmbBaudRate.Items.Contains(serialCfg.Item2)) cmbBaudRate.SelectedItem = serialCfg.Item2;

            _gateConfigs = ConfigManager.LoadGateConfigs();
            SetupGateButtons();
            
            UpdateUIState();

            // Tenta conectar automaticamente à última porta gravada
            if (cmbPorts.SelectedItem != null && cmbPorts.SelectedItem.ToString() == serialCfg.Item1)
            {
                BtnConnect_Click(null, null);
            }
        }

        private void LoadPorts()
        {
            string currentPort = cmbPorts.SelectedItem != null ? cmbPorts.SelectedItem.ToString() : null;
            cmbPorts.Items.Clear();
            
            cmbPorts.Items.AddRange(SerialPort.GetPortNames());
            
            if (!string.IsNullOrEmpty(currentPort) && cmbPorts.Items.Contains(currentPort))
                cmbPorts.SelectedItem = currentPort;
            else if (cmbPorts.Items.Count > 0)
                cmbPorts.SelectedIndex = 0;
        }

        private void SetupGateButtons()
        {
            _gateButtons = new Button[16];
            for (int i = 0; i < 16; i++)
            {
                int gateId = i + 1;
                GateConfig config = _gateConfigs.FirstOrDefault(c => c.Id == gateId);
                
                Button btn = new Button();
                btn.Dock = DockStyle.Fill;
                btn.Margin = new Padding(5);
                btn.Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold);
                btn.Text = config != null ? config.Name : string.Format("Portão {0}", gateId);
                btn.Tag = config != null ? config.Command : gateId.ToString();
                
                btn.Click += GateButton_Click;
                
                int row = i / 8;
                int col = i % 8;
                
                _gateButtons[i] = btn;
                tlpGates.Controls.Add(btn, col, row);
            }
        }

        private void GateButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string command = btn != null ? btn.Tag as string : null;
            if (btn != null && command != null)
            {
                DialogResult dialogResult = MessageBox.Show(string.Format("Tem certeza que deseja acionar o {0}?", btn.Text), "Confirmação de Segurança", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        _serialManager.SendCommand(command);
                        DisableButtonTemporarily(btn, 10000); // 10 segundos de pausa anti-spam
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Erro ao enviar comando: {0}", ex.Message), "Falha de Comunicação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Bloqueia o botão para evitar envio acidental múltiplo, liberando-o automaticamente
        private void DisableButtonTemporarily(Button btn, int milliseconds)
        {
            btn.Enabled = false;
            
            Timer t = new Timer();
            t.Interval = milliseconds;
            t.Tick += (s, ev) => 
            {
                if (_serialManager.IsOpen)
                {
                    btn.Enabled = true;
                }
                t.Stop();
                t.Dispose();
            };
            t.Start();
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (_serialManager.IsOpen)
            {
                _serialManager.Disconnect();
            }
            else
            {
                if (cmbPorts.SelectedItem == null)
                {
                    MessageBox.Show("Nenhuma porta COM encontrada/selecionada. Verifique as conexões USB do Arduino.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string port = cmbPorts.SelectedItem.ToString();
                int baudRate = int.Parse(cmbBaudRate.SelectedItem.ToString());

                try
                {
                    _serialManager.Connect(port, baudRate);
                    ConfigManager.SaveSerialConfig(port, baudRate.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Erro de Hardware ao abrir {0}: {1}", port, ex.Message), "Acesso Negado ou Ocupado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
            UpdateUIState();
        }

        private void UpdateUIState()
        {
            bool isConnected = _serialManager.IsOpen;

            cmbPorts.Enabled = !isConnected;
            cmbBaudRate.Enabled = !isConnected;
            btnRefreshPorts.Enabled = !isConnected;
            
            if (isConnected)
            {
                btnConnect.Text = "Desconectar";
                lblStatus.Text = string.Format("Conectado em {0} @ {1} baud", cmbPorts.SelectedItem, cmbBaudRate.SelectedItem);
                lblStatus.ForeColor = Color.DarkGreen;
            }
            else
            {
                btnConnect.Text = "Conectar";
                lblStatus.Text = "Desconectado";
                lblStatus.ForeColor = Color.Red;
            }

            foreach (var btn in _gateButtons)
                if (btn != null) btn.Enabled = isConnected;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _serialManager.Disconnect();
        }
    }
}
