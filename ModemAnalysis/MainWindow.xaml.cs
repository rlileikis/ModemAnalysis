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
		readonly Communication Comm = new Communication();
   
        private bool isConnected = false;

        readonly string correctFW = "BG96MAR02A07M1G_01.018.01.018";
        readonly string unknownStatus = "Unknown";
		readonly string CheckStatusString = "Check Modem status";
        readonly int newFwIndexInComboBox = 0;
        public MainWindow()
		{
            InitializeComponent();
            //Loaded += MyWindow_Loaded;
            InitPortNames();
            InitDfotaUrls();
            Comm.ProcessReceived += C_ProcessReceived;

        }

        // Example https://docs.microsoft.com/en-us/dotnet/standard/events/how-to-raise-and-consume-events
        public void C_ProcessReceived(object sender, ProcessReceivedEventArgs e)
        {
            this.Dispatcher.Invoke(() => 
            {
                PrintDebug(e.Message);
            });
        }

        public void PrintDebug(string str)
		{
			TextBox_PrintAll.AppendText($"{str}{Environment.NewLine}");
			//TextBox_PrintAll.AppendText(str);
			//TextBox_PrintAll.AppendText(Environment.NewLine);
			TextBox_PrintAll.ScrollToEnd();
		}

        private void Button_Click_Connect(object sender, RoutedEventArgs e)
		{
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
            MakeLablesUnknownAgain();
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
            Comm.CheckModemStatus();
        }

        private void Button_ModemInit_Click(object sender, RoutedEventArgs e)
        {
            Comm.ModemInit(txtBx_APN.Text, txtBx_User.Text, txtBx_Pass.Text);
        }

        private void InitPortNames()
        {
            string trimmedComPortName;
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
				//string lastLine = TextBox_PrintAll.Text.Split('\n').LastOrDefault();

				string[] textboxstring = TextBox_PrintAll.Text.Split('\n');
				string lastLine = textboxstring[textboxstring.Length - 2];
				AnalyseReceivedData(lastLine);
			}

			catch
			{
			}
                       
        }

		private void AnalyseReceivedData(string lastLine)
		{
			if (lastLine.Contains("ERROR")) lbl_Status.Content = "Error";
			if (lastLine.Contains("READY"))
			{
				MakeLablesUnknownAgain();
				lbl_Status.Content = "Ready for command";
			}

			if (lastLine.Contains("BG96") && !lastLine.Contains("ID,MODEM")) //means that it is IN the Test Mode
			{
				lbl_ModVer.Content = lastLine; //pagalvoti del kitu modemu
				ColorModVerLableAccordingly();
				btn_GoToTestMode.IsEnabled = false;
				btn_CheckModemStatus.IsEnabled = true;
			}

			if (lastLine.Contains("Test Mode")) //means that it is IN the Test Mode
			{
				btn_GoToTestMode.IsEnabled = false;
			}

			if (lastLine.Contains("COPS"))
			{
				var match = Regex.Match(lastLine, "\"([^\"]*)\"").Groups[1].Value;
				lbl_Operator.Content = match;
			}

			if (lastLine.Contains("APP"))
			{
				Comm.ModemInit(txtBx_APN.Text, txtBx_User.Text, txtBx_Pass.Text);
				lbl_Operator.Content = CheckStatusString;
				lbl_Signal.Content = CheckStatusString;
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
				string pattern1 = "REG: (\\d),(\\d),\"[^\"]*\",\"[^\"]*\",(\\d)";
				string pattern2 = "REG: (\\d),\"[^\"]*\",\"[^\"]*\",(\\d)";
				string pattern3 = "REG: (\\d)$";

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

			if (lastLine.Contains("HTTPSTART")) //FOTA started
			{
				btn_CheckModemStatus.IsEnabled = false;
				btn_ModemFwUpdate.IsEnabled = false;
			}

			if (lastLine.Contains("END")) //FOTA finished
			{
				string httpendPattern = "\\+QIND: \"FOTA\",\"HTTPEND\",(\\d*)";
				string endPattern = "\\+QIND: \"FOTA\",\"END\",(\\d*)";
				if (Regex.IsMatch(lastLine, httpendPattern))
				{
					var fotaStatus = int.Parse(Regex.Match(lastLine, httpendPattern).Groups[1].Value);
					PrintDebug($"DFOTA status: {PrintDfotaDownStatus(fotaStatus)}");
				}

				if (Regex.IsMatch(lastLine, endPattern))
				{
					var fotaStatus = int.Parse(Regex.Match(lastLine, endPattern).Groups[1].Value);
					PrintDebug($"DFOTA status: {PrintDfotaUpgrStatus(fotaStatus)}");
					if (fotaStatus == 0) 
					{
						PrintDebug($"DFOTA status: {PrintDfotaUpgrStatus(fotaStatus)}");
						btn_CheckModemStatus.IsEnabled = true;
						lbl_ModVer.Content = unknownStatus;
						lbl_ModVer.Background = Brushes.Transparent;
					}
				}
			}
		}

		private string PrintDfotaDownStatus(int fotaStatus)
		{
			return fotaStatus switch
			{
				0 => "Download successful, wait for upgrade",
				701 => "HTTP(S) unknown error",
				702 => "Server connection failed",
				703 => "Request failed",
				704 => "Download timeout",
				706 => "File not exist",
				707 => "Write data to file failed",
				708 => "Downloaded file is too large",
				_ => unknownStatus
			};
		}

		private string PrintDfotaUpgrStatus(int fotaStatus)
		{
			return fotaStatus switch
			{
				0 => "Upgraded successfully",
				504 => "Firmware upgrade failed",
				505 => "Upgrade package not exist",
				506 => "Upgrade package check failed",
				511 => "Package is mismatched with the current firmware",
				_ => unknownStatus
			};
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
            if (Comm.StartDfota(comboBox_DfotaSelection.SelectedIndex))
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

		private void ComboBox_PortSelection_DropDownOpened(object sender, EventArgs e)
		{
            InitPortNames();
        }
	}



}
