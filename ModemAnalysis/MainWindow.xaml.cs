using ModemAnalysis.Models;
using ModemAnalysis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using static ModemAnalysis.Communication;

namespace ModemAnalysis
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        private List<ApnSetting> ApnSettings { get; set; }

		readonly Communication Comm = new Communication();
        readonly TestStep TestSteps = new TestStep();
   
        private bool isConnected = false;

        

        public MainWindow()
		{
            InitializeComponent();
            //Loaded += MyWindow_Loaded;
            InitPortNames();

            Comm.ProcessReceived += c_ProcessReceived;

        }

        public void c_ProcessReceived(object sender, ProcessReceivedEventArgs e)
        {
            this.Dispatcher.Invoke(() => 
            {
                PrintDebug(e.message);
            });
        }

        public void PrintDebug(string str)
		{
			//richTextBox_PrintAll.AppendText(str);
			//richTextBox_PrintAll.AppendText(Environment.NewLine);
			//richTextBox_PrintAll.ScrollToEnd();
			TextBox_PrintAll.AppendText(str);
			TextBox_PrintAll.AppendText(Environment.NewLine);
			TextBox_PrintAll.ScrollToEnd();
		}

        private void Button_Click_Connect(object sender, RoutedEventArgs e)
		{
			//printDebug("Prisijungiam prie porto");
            if (isConnected == false) 
            {
                OpenPort();
            }
            else
            {
                ClosePort();
            }
        }

		private void MyWindow_Loaded(object sender, RoutedEventArgs e)
		{
			PrintDebug("Uzkurem programa");
		}

		private void Button_Click_GoToTestMode(object sender, RoutedEventArgs e)
		{
			PrintDebug("Startuojam testa");
            if (Comm.GotoTestMode(GetPortFromComboList())) // pakeist comm
            {
                PrintDebug("Perejom i test mode");
                
            }
            else
            {
                PrintDebug("Device is disconnected, trying to reconnect.");
                OpenPort();
                PrintDebug("Start test again");
            }
                
        }

        private void Button_CheckModemStatus(object sender, RoutedEventArgs e)
        {
            //Comm.WritePort("ATI");
            var response = Comm.ModemInit(txtBx_APN.Text, txtBx_Pass.Text, txtBx_User.Text);
            //PrintDebug(response);
            //TestSteps.SendAT();
        }

        private void Button_ModemInit(object sender, RoutedEventArgs e)
        {
            //Comm.WritePort("ATI");
            var response = Comm.ModemInit(txtBx_APN.Text, txtBx_Pass.Text, txtBx_User.Text);
            //PrintDebug(response);
            //TestSteps.SendAT();
        }

        private void InitPortNames()
        {
            string trimmedComPortName = "";
            comboBox_PortSelection.Items.Clear();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity"); //returns information about the devices found in Device Manager. https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-pnpentity

                foreach (ManagementObject queryObj in searcher.Get()) 
                {
                    if (queryObj["Caption"] != null)
                    {
                        if (queryObj["Caption"].ToString().Contains("(COM")) //Finds all devices with caption "COM"
                        {
                            //trimmedComPortName = queryObj["Caption"].ToString().Split('(', ')')[1];
                            string comPortName = Regex.Match(queryObj["Caption"].ToString(), @"\(COM([^)]*)\)").Groups[1].Value;
                           // comPortName = "2";
                            trimmedComPortName = "COM" + comPortName;  // add trimmed COM back

                            comboBox_PortSelection.Items.Add(trimmedComPortName + " - " + queryObj["Description"]);
                         }
                    }
                }
            }
            catch (ManagementException e)
            {
                MessageBox.Show(e.Message);
            }

        }
        public void OpenPort()
        {

            if (comboBox_PortSelection.SelectedIndex > -1)
			{
				if (Comm.OpenPort(GetPortFromComboList()))
				{
					btn_Connect.Content = "Disconnect";
					comboBox_PortSelection.IsEnabled = false;
					isConnected = true;
					PrintDebug($">>> Connected to port {GetPortFromComboList()}");
					stat_TestMode.Fill = Brushes.Green;
				}
				else
				{
					PrintDebug($">>> Can't connect to port {GetPortFromComboList()}");
				}
			}
			else MessageBox.Show("Choose port");
            
        }

		private string GetPortFromComboList()
		{
			return comboBox_PortSelection.SelectedItem.ToString().Substring(0, comboBox_PortSelection.SelectedItem.ToString().IndexOf(" "));
		}

		void ClosePort()
        {
            Comm.ClosePort();
			isConnected = false;
            btn_Connect.Content = "Connect";
            comboBox_PortSelection.IsEnabled = true;
			PrintDebug($">>> Port {GetPortFromComboList()} disconnected");
		}


		private void TextBox_PrintAll_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
            string lastLine = TextBox_PrintAll.Text.Split('\n').LastOrDefault();

            if (lastLine.Contains("ERROR")) lbl_Status.Content = "Error";
            if (lastLine.Contains("READY")) lbl_Status.Content = "Ready";

            if (lastLine.Contains("BG96"))
            {
                lbl_ModVer.Content = lastLine; //pagalvoti del kitu modemu
            }

            if (lastLine.Contains("COPS"))
            {
                //var match = Regex.Match(lastLine, @"key : (?<+COPS: 0,0,>)").Groups[1].Value; //fix
                lbl_Operator.Content = lastLine;
            }

        }

		private void Button_GoToUpdateModemFw(object sender, RoutedEventArgs e)
		{
            //AT+QFOTADL="https://ruptelafwsa.blob.core.windows.net/fwsa/BG96FW/BG96MAR02A07M1G_01.018.01.018-BG96MAR02A07M1G_01.016.01.016.bin"
            //https://ruptelafwsa.blob.core.windows.net/fwsa/BG96FW/BG96MAR02A07M1G_01.016.01.016-BG96MAR02A07M1G_01.018.01.018.bin
            if (Comm.GotoModemUpdate(GetPortFromComboList(), txtBx_DfotaUrl.Text))
            {
                PrintDebug("DFOTA update initiated");

            }
            else
            {
                PrintDebug("Device is disconnected, trying to reconnect.");
                OpenPort();
                PrintDebug("Start test again");
            }

        }
    }



}
