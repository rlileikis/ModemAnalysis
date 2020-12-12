using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModemAnalysis
{
	public class Communication
	{
		private SerialPort serialPort;

		public Communication()
		{
			serialPort = new SerialPort();
		}

		public string PortName
		{
			get
			{
				return serialPort.PortName;
			}
			set
			{
				serialPort.PortName = value;
			}
		}

		public bool OpenPort(string portName)
		{
			serialPort.PortName = portName;
			serialPort.Handshake = Handshake.None;
			serialPort.ReadTimeout = 500;
			serialPort.DataBits = 8;
			serialPort.Parity = Parity.None;
			serialPort.StopBits = StopBits.One;			
			try
			{
				serialPort.Open();
				return true;
			}
			catch
			{
				return false;
			}
		}

		public void ClosePort()
		{
			using (serialPort)
			{
				try
				{
					serialPort.DiscardInBuffer();
					serialPort.Close();
				}
				catch (Exception)
				{

				}
			}
		}


	}
}
