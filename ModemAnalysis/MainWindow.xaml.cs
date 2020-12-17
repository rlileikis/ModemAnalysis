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
        readonly string CheckStatusString = "Check Modem status";
        readonly int newFwIndexInComboBox = 0;
        //readonly int oldFwIndexInComboBox = 1;
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
                PrintDebug(e.Message);
            });
        }

        public void PrintDebug(string str)
		{
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
                Comm.CheckIfItIsDeviceOrModem();
            }
            else
            {
                ClosePort();
                MakeLablesUnknownAgain();
                MakeButtonsDisabledAgain();
            }
        }

		private void MyWindow_Loaded(object sender, RoutedEventArgs e)
		{
			PrintDebug("Make sure device is connected to the USB and Power supply");
        }

		private void Button_Click_GoToTestMode(object sender, RoutedEventArgs e)
		{
            if (txtBx_APN.Text == "")
            {
                if (MessageBox.Show("Do you want to use blank APN?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    //If no, then do nothing
                }
                else
                {// If yes do then
                    GoToTestMode();
                }
            }
            else
            {
                GoToTestMode();
            }

        }

		private void GoToTestMode()
		{
            PrintDebug("Going to Test Mode. Please wait..");
            if (Comm.GotoTestMode(GetPortFromComboList()))
			{
				PrintDebug("Test Mode success");
                PrintDebug("Waiting for modem");
            }
			else
			{
				PrintDebug("Device is disconnected, trying to reconnect.");
				if (OpenPort()) PrintDebug("Start test again");
                else PrintDebug("Check connection");
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
            comboBox_DfotaSelection.SelectedIndex = newFwIndexInComboBox;
        }



        public bool OpenPort()
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
                    return true;
				}
				else
				{
					PrintDebug($">>> Can't connect to port {GetPortFromComboList()}");
                    return false;
				}
			}
			else MessageBox.Show("Choose port");
            return false;
            
        }

		private void MakeLablesUnknownAgain()
		{
			lbl_ModVer.Content = unknownStatus;
			lbl_Operator.Content = unknownStatus;
            lbl_OpStatus.Content = unknownStatus;
			lbl_Signal.Content = unknownStatus;
			lbl_Status.Content = unknownStatus;

            lbl_ModVer.Background = Brushes.Transparent;
            lbl_Operator.Background = Brushes.Transparent;
            lbl_OpStatus.Background = Brushes.Transparent;
            lbl_Signal.Background = Brushes.Transparent;
            lbl_Status.Background = Brushes.Transparent;
        }

        private void MakeButtonsDisabledAgain()
        {
            btn_CheckModemStatus.IsEnabled = false;
            btn_GoToTestMode.IsEnabled = false;
            btn_ModemFwUpdate.IsEnabled = false;
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
			//Hard to define all possible answers. doing try catch
			try
			{
                string lastLine = TextBox_PrintAll.Text.Split('\n').LastOrDefault();

                if (lastLine.Contains("ERROR")) lbl_Status.Content = "Error";
                if (lastLine.Contains("READY"))
                {
                    lbl_Status.Content = "Ready for command";
                    lbl_ModVer.Content = unknownStatus;
                }

                

                if (lastLine.Contains("BG96") && !lastLine.Contains("ID,MODEM")) //means that it is IN the Test Mode
                {
                    lbl_ModVer.Content = lastLine; //pagalvoti del kitu modemu
                    ColorModVerLableAccordingly();
                    btn_GoToTestMode.IsEnabled = false;
                    btn_CheckModemStatus.IsEnabled = true;
                    //btn_ModemFwUpdate.IsEnabled = true;
                }

                if (lastLine.Contains("Test Mode")) //means that it is IN the Test Mode
                {
                    btn_GoToTestMode.IsEnabled = false;
                }

                if (lastLine.Contains("COPS"))
                {
                    var match = Regex.Match(lastLine, "\"([^\"]*)\"").Groups[1].Value;

                    //string comPortName = Regex.Match(comboBox_Port.Text, @"COM([^ ]*) ").Groups[1].Value;

                    lbl_Operator.Content = match;
                }

                if (lastLine.Contains("APP"))
                {
                    Comm.ModemInit(txtBx_APN.Text, txtBx_User.Text, txtBx_Pass.Text);
                    lbl_Operator.Content = CheckStatusString;
                    lbl_Signal.Content = CheckStatusString;
                    //btn_CheckModemStatus.IsEnabled = true;
                }

                if (lastLine.Contains("ID,MODEM")) //means that it is NOT in the TestMode
                {
                    var matchG1 = Regex.Match(lastLine, "ID,MODEM,([^\"]*);").Groups[1].Value;
                    if (matchG1 == "") lbl_ModVer.Content = "Reconnect after 10s or go to TestMode";
                    else lbl_ModVer.Content = matchG1;
                    ColorModVerLableAccordingly();
                    btn_GoToTestMode.IsEnabled = true;
                    btn_CheckModemStatus.IsEnabled = false;
                    btn_ModemFwUpdate.IsEnabled = false;
                }

                if (lastLine.Contains("CSQ")) //checks signal level
                {
                    var match = Regex.Match(lastLine, "\\+CSQ:\\ (\\d*),(\\d*)").Groups[1].Value;
                    lbl_Signal.Content = $"{match} out of 31";
                }


                if (lastLine.Contains("REG"))
                {   //Faster to firstly match "REG" instead of doing Regex everytime
                    String pattern1 = "REG: (\\d),(\\d),\"[^\"]*\",\"[^\"]*\",(\\d)";
                    String pattern2 = "REG: (\\d),\"[^\"]*\",\"[^\"]*\",(\\d)";
                    String pattern3 = "REG: (\\d)$";

                    if (Regex.IsMatch(lastLine, pattern1))
                    {
                        var regVal = int.Parse(Regex.Match(lastLine, pattern1).Groups[2].Value);
                        GetOpStatusString(regVal);
                        ModemFwUpdateButtonStatus(regVal);
                    }
                    else if (Regex.IsMatch(lastLine, pattern2))
					{
                        var regVal = int.Parse(Regex.Match(lastLine, pattern2).Groups[1].Value);
                        GetOpStatusString(regVal);
                        ModemFwUpdateButtonStatus(regVal);
                    }
                    else
                    {
                        var regVal = int.Parse(Regex.Match(lastLine, pattern3).Groups[1].Value);
                        GetOpStatusString(regVal);
                        ModemFwUpdateButtonStatus(regVal);
                    }
                }
            }

            catch
			{

			}
                       
        }

		private void GetOpStatusString(int cregVal)
		{
			lbl_OpStatus.Content = cregVal switch
			{
				0 => "No reg, no search",
				1 => "Registered, home",
				2 => "No reg, searching",
				3 => "Red denied",
				4 => "Unknown",
				5 => "Registered, roaming",
				_ => unknownStatus
			};
		}

        private void ModemFwUpdateButtonStatus(int cregVal)
        {
            if (cregVal == 1 || cregVal == 5) btn_ModemFwUpdate.IsEnabled = true;
            else btn_ModemFwUpdate.IsEnabled = false;
        }

        private void ColorModVerLableAccordingly()
		{
			if (lbl_ModVer.Content.ToString() == correctFW) lbl_ModVer.Background = Brushes.LightGreen;
			else lbl_ModVer.Background = Brushes.Tomato;
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
                if (OpenPort()) PrintDebug("Start test again");
                else PrintDebug("Check connection");
            }

        }

		private void comboBox_PortSelection_DropDownOpened(object sender, EventArgs e)
		{
            InitPortNames();
        }

	}



}
