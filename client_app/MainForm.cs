using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static System.IO.Ports.SerialPort;

namespace SimpleCryptoClient_v1
{
    public partial class MainForm : Form
    {
        Stopwatch stopwatch;
        double frequency;
        bool highRes;

        public string input;
        public string output;
        public string label;
        public MainForm()
        {
            InitializeComponent();
            frequency = Stopwatch.Frequency;
            highRes = Stopwatch.IsHighResolution;
            stopwatch = new Stopwatch();
        }

        private void btnOpenPort_Click(object sender, EventArgs e)
        {
            serialPort1.PortName = comboBox1.Text;
            serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
            serialPort1.Open();
            Thread.Sleep(100);
            if (serialPort1.IsOpen)
            {
                textBox1.AppendText("Port open" + Environment.NewLine);
                btnOpenPort.Enabled    = false;
                btnExit.Enabled        = false;
                btnClosePort.Enabled   = true;
                btnClearOutput.Enabled = true;
                btnTest.Enabled        = true;
            }
                
        }

        private void btnClosePort_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            Thread.Sleep(100);
            if (!serialPort1.IsOpen)
            {
                textBox1.AppendText("Port closed" + Environment.NewLine);
                btnOpenPort.Enabled    = true;
                btnExit.Enabled        = true;
                btnClosePort.Enabled   = false;
                btnClearOutput.Enabled = false;
                btnTest.Enabled        = false;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClearOutput_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            textBox1.AppendText(Environment.NewLine);
            textBox1.AppendText(label + Environment.NewLine);
            textBox1.AppendText("Stopwatch" + Environment.NewLine);
            textBox1.AppendText("Frequency=" + frequency.ToString() + Environment.NewLine);
            textBox1.AppendText("IsHighResolution=" + highRes.ToString() + Environment.NewLine);
            count = 0; right = 0; wrong = 0;
            encrypt();
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            string line = serialPort1.ReadLine();
           
            this.BeginInvoke(new LineReceivedEvent(LineReceived), line);
        }

        public int count = 0; int right = 0; int wrong = 0;

        private delegate void LineReceivedEvent(string line);
        private void LineReceived(string line)
        {
            runStopwatch = false;

            if (line.Equals(output))
                right++;
            else
                wrong++;

            count++;

            if (count <= 1000)
                encrypt();
            else
                textBox1.AppendText($"Count= {count} Right={right} Wrong= {wrong}" + Environment.NewLine);
        }

        //-----------------------------------------------------------------

        public void encrypt()
        {
            runStopwatch = true;
            Write(input);
        }

        public bool runStopwatch
        {
            get { 
                return stopwatch.IsRunning; 
            }
            set {
                if (value)
                {
                    if (!stopwatch.IsRunning)
                        stopwatch.Start();
                }
                else
                {
                    if (stopwatch.IsRunning)
                    {
                        stopwatch.Stop();
                        textBox1.AppendText(stopwatch.ElapsedTicks.ToString() + Environment.NewLine);
                        stopwatch.Reset();
                    }
                }
            }
        }

        public void Write(string data)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    byte[] OutString = new byte[64];
                    OutString = Encoding.ASCII.GetBytes(data + Environment.NewLine);
                    serialPort1.Write(OutString, 0, OutString.Length);
                }
                catch
                {
                    textBox1.AppendText("Error writing to serial" + Environment.NewLine);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = GetPortNames();
            foreach (string port in ports)
                comboBox1.Items.Add(port);

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
                
            comboBox2.SelectedIndex = 9;
            comboBox3.SelectedIndex = 0;

            if (comboBox3.SelectedIndex == 0)
            {
                input = "$10 00112233445566778899AABBCCDDEEFF";
                output = "45C8188BB1DF1A812734C3DF5E2C9FAF";
                label = "Encrypt Test";
            }
            else
            {
                input = "$20 45C8188BB1DF1A812734C3DF5E2C9FAF";
                output = "00112233445566778899AABBCCDDEEFF";
                label = "Decrypt Test";
            }
        }



        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == 0)
            {
                input = "$10 00112233445566778899AABBCCDDEEFF";
                output = "45C8188BB1DF1A812734C3DF5E2C9FAF";
                label = "Encrypt Test";
            }
            else
            {
                input = "$20 45C8188BB1DF1A812734C3DF5E2C9FAF";
                output = "00112233445566778899AABBCCDDEEFF";
                label = "Decrypt Test";
            }
        }
    }
}
