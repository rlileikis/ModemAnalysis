﻿using ModemAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
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
    
namespace ModemAnalysis
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        private List<ApnSetting> ApnSettings { get; set; }

        private bool isConnected = false;
        public MainWindow()
		{
            InitializeComponent();
            //Loaded += MyWindow_Loaded;
            InitPortNames();
        }

		public void printDebug(string str)
		{
			richTextBox_PrintAll.AppendText(str);
			richTextBox_PrintAll.AppendText(Environment.NewLine);
			richTextBox_PrintAll.ScrollToEnd();
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
			printDebug("Uzkurem programa");
		}

		private void Button_Click_Start(object sender, RoutedEventArgs e)
		{
			printDebug("Startuojam testa");
		}

        private void InitPortNames()
        {
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
                            string comPortName = queryObj["Caption"].ToString().Split('(', ')')[1];
                            comboBox_PortSelection.Items.Add(comPortName + " - " + queryObj["Description"]);
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
            Communication OpenCom = new Communication();
            if (comboBox_PortSelection.SelectedIndex > -1)
            {
                var trimmedComPortName = comboBox_PortSelection.Text.Split(' ')[0];

                if (OpenCom.OpenPort(trimmedComPortName))
                {
                    btn_Connect.Content = "Disconnect";
                    comboBox_PortSelection.IsEnabled = false;
                    isConnected = true;
                    printDebug($">>> Connected to port {trimmedComPortName}");
                }
                else
                {
                    printDebug($">>> Can't connect to port {trimmedComPortName}");
                }
            }
            else MessageBox.Show("Choose port");


        }

        void ClosePort()
        {
            Communication CloseCom = new Communication();
            var trimmedComPortName = comboBox_PortSelection.Text.Split(' ')[0];
            CloseCom.ClosePort();
			isConnected = false;
            btn_Connect.Content = "Connect";
            comboBox_PortSelection.IsEnabled = true;
			printDebug($">>> Port {trimmedComPortName} disconnected");
		}




    }



}
