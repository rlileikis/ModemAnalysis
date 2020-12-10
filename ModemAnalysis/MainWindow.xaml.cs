using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		public MainWindow()
		{
			InitializeComponent();
			//Loaded += MyWindow_Loaded;
		}

		public void printDebug(string str)
		{
			richTextBox_PrintAll.AppendText(str);
			richTextBox_PrintAll.AppendText(Environment.NewLine);
			richTextBox_PrintAll.ScrollToEnd();
		}

		private void Button_Click_Connect(object sender, RoutedEventArgs e)
		{
			printDebug("Prisijungiam prie porto");
		}

		private void MyWindow_Loaded(object sender, RoutedEventArgs e)
		{
			printDebug("Uzkurem programa");
		}

		private void Button_Click_Start(object sender, RoutedEventArgs e)
		{
			printDebug("Startuojam testa");
		}
	}



}
