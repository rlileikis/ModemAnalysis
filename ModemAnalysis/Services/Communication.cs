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
			serialPort.BaudRate = 115200;
			serialPort.Handshake = Handshake.None;
			serialPort.ReadTimeout = 500;
			serialPort.DataBits = 8;
			serialPort.Parity = Parity.None;
			serialPort.StopBits = StopBits.One;

			//serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

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

		public bool GotoTestMode()
		{
			if (serialPort.IsOpen == true)
			{
				serialPort.Write("SET,SYSTEM,TEST_MODE,3;\r\n");
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool WritePort(byte[] data)
		{
			if (serialPort.IsOpen == true)
			{
				serialPort.Write(data, 0, data.Length);
				return true;
			}
			else
			{
				return false;
			}
		}



	}
}
