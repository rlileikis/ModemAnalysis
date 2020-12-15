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

        readonly string correctFW = "BG96MAR02A07M1G_01.018.01.018";
        readonly string unknownStatus = "Unknown";
        readonly string waitingString = "Waiting";
        public MainWindow()
		{
            InitializeComponent();
            //Loaded += MyWindow_Loaded;
            InitPortNames();
            InitDfotaUrls();
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
				Comm.DeviceOrModem();
			}
			else
            {
                ClosePort();
            }
        }

		private void MyWindow_Loaded(object sender, RoutedEventArgs e)
		{
			PrintDebug("Make sure device is connected to the USB and Power supply");
            //PrintDebug("Connect to the device");
        }

		private void Button_Click_GoToTestMode(object sender, RoutedEventArgs e)
		{
            if (txtBx_APN.Text == "")
            {
                if (MessageBox.Show("Do you want to use blank APN?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    //do no stuff
                    //do nothing
                }
                else
                {// do yes stuff
                    PrintDebug("Going to Test Mode. Please wait..");
                    GoToTestMode();
                }

            }
            else
            {
                PrintDebug("Going to Test Mode. Please wait..");
                GoToTestMode();
            }

        }

		private void GoToTestMode()
		{
			if (Comm.GotoTestMode(GetPortFromComboList())) // pakeist comm
			{
				PrintDebug("Test Mode success");
                PrintDebug("Waiting for modem");
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
            var response = Comm.CheckModemStatus();
            //PrintDebug(response);
            //TestSteps.SendAT();
        }

        private void btn_ModemInit_Click(object sender, RoutedEventArgs e)
        {
            //Comm.WritePort("ATI");
            var response = Comm.ModemInit(txtBx_APN.Text, txtBx_User.Text, txtBx_Pass.Text);
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
                            string comPortName = Regex.Match(queryObj["Caption"].ToString(), @"\(COM([^)]*)\)").Groups[1].Value;
                            trimmedComPortName = "COM" + comPortName;  // add trimmed COM back
                            comboBox_PortSelection.Items.Add(trimmedComPortName + " - " + queryObj["Description"]);
                         }
                    }
                }
                comboBox_PortSelection.SelectedIndex = 0;
            }
            catch (ManagementException e)
            {
                MessageBox.Show(e.Message);
            }

        }


        private void InitDfotaUrls()
		{
            //https://ruptelafwsa.blob.core.windows.net/fwsa/BG96FW/BG96MAR02A07M1G_01.018.01.018-BG96MAR02A07M1G_01.016.01.016.bin
            //https://ruptelafwsa.blob.core.windows.net/fwsa/BG96FW/BG96MAR02A07M1G_01.016.01.016-BG96MAR02A07M1G_01.018.01.018.bin
            comboBox_DfotaSelection.Items.Add("BG96MAR02A07M1G_01.016.01.016 -> BG96MAR02A07M1G_01.018.01.018.bin"); // ID: 0
            comboBox_DfotaSelection.Items.Add("BG96MAR02A07M1G_01.018.01.018 -> BG96MAR02A07M1G_01.016.01.016.bin"); // ID: 1
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
                    MakeLablesUnknownAgain();
                    stat_TestMode.Fill = Brushes.Green;
				}
				else
				{
					PrintDebug($">>> Can't connect to port {GetPortFromComboList()}");
				}
			}
			else MessageBox.Show("Choose port");
            
        }

		private void MakeLablesUnknownAgain()
		{
			lbl_ModVer.Content = unknownStatus;
			lbl_Operator.Content = unknownStatus;
			lbl_Signal.Content = unknownStatus;
			lbl_Status.Content = unknownStatus;

            lbl_ModVer.Background = Brushes.Transparent;
            lbl_Operator.Background = Brushes.Transparent;
            lbl_Signal.Background = Brushes.Transparent;
            lbl_Status.Background = Brushes.Transparent;
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

            if (lastLine.Contains("BG96")) //means that it is IN the Test Mode
            {
                lbl_ModVer.Content = lastLine; //pagalvoti del kitu modemu
                if (lbl_ModVer.Content.ToString() == correctFW) lbl_ModVer.Background = Brushes.LightGreen;
                else lbl_ModVer.Background = Brushes.Tomato;
                btn_GoToTestMode.IsEnabled = false;
            }

            if (lastLine.Contains("Test Mode")) //means that it is IN the Test Mode
            {
                btn_GoToTestMode.IsEnabled = false;
            }

            if (lastLine.Contains("COPS"))
            {
                //string patern = @"+COPS: (d),(d),"([^"]*),(d)";
                var match = Regex.Match(lastLine, "\"([^\"]*)\"").Groups[1].Value; //fix

                //string comPortName = Regex.Match(comboBox_Port.Text, @"COM([^ ]*) ").Groups[1].Value;

                lbl_Operator.Content = match;
            }

            if (lastLine.Contains("APP"))
            {
                Comm.ModemInit(txtBx_APN.Text, txtBx_User.Text, txtBx_Pass.Text);
                lbl_Operator.Content = waitingString;
                lbl_Signal.Content = waitingString;
            }

            if (lastLine.Contains("HW")) //means that it is NOT in the TestMode
            {
                btn_GoToTestMode.IsEnabled = true;
            }

            if (lastLine.Contains("CSQ")) //means that it is NOT in the TestMode
            {
                lbl_Signal.Content = lastLine;
            }

        }

		private void Button_GoToUpdateModemFw(object sender, RoutedEventArgs e)
		{


            if (Comm.StartDfota(GetPortFromComboList(), comboBox_DfotaSelection.SelectedIndex))
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

		private void comboBox_PortSelection_DropDownOpened(object sender, EventArgs e)
		{
            InitPortNames();
        }

	}



}
