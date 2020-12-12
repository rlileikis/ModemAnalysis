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
		private readonly SerialPort serialPort;


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

			serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
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
			ResetBuffer();
			ResetQueue();
		}

		public bool GotoTestMode()
		{
			if (serialPort.IsOpen == true)
			{
				WritePort("SET,SYSTEM,TEST_MODE,3;"); //Access Test Mode in the device
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool WritePort(string line)
		{
			if (serialPort.IsOpen == true)
			{
				serialPort.Write($"{line}\r\n");
				return true;
			}
			else
			{
				return false;
			}
		}

		private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
		{
			SerialPort sp = (SerialPort)sender;

			try
			{
				int readBytes = sp.BytesToRead;
				byte[] readBuffer = new byte[readBytes];
				sp.Read(readBuffer, 0, readBytes);

				for (int i = 0; i < readBuffer.Length; i++)
				{
					if (Queue_AddByte(readBuffer[i]) == (-1))
					{
						Console.WriteLine("Buffer Full");
						break;
					}
				}
				ProcessReceived();
			}
			catch { }
		}

		public void ProcessReceived()
		{
			while (true)
			{
				int result = Queue_PopByte();
				if (result == (-1))
				{
					break;
				}

		
				if (Convert.ToChar(result) == '\r')
				{
					MessageBox.Show(new string(lineBuffer));
					lineBuffer[index] = Convert.ToChar(result);
					lineBuffer[index + 1] = '\n';
					index += 2;
					
					//(lineBuffer, index, "");
					lineBuffer = new char[Communication.bufferSize];
					index = 0;
				}
				else
				{
					if (Convert.ToChar(result) != '\n')
					{
						lineBuffer[index] = Convert.ToChar(result);
						index++;
					}
				}
			}

		}

		public void ResetBuffer()
		{
			lineBuffer = new char[Communication.bufferSize];
			index = 0;
		}

		#region Queue

		char[] lineBuffer = new char[bufferSize];
		int index = 0;

		public const int bufferSize = 4096;
		private int head = 0;
		private int tail = 0;
		private int fullNotEmpty = 0;
		private byte[] rxBuffer = new byte[bufferSize];

		private int Queue_AddByte(byte _byte)
		{
			if ((fullNotEmpty != 0) && (tail == head)) return -1;
			rxBuffer[head] = _byte;
			head++;
			if (head >= bufferSize) head = 0;
			if (head == tail) fullNotEmpty = 1;
			return 0;
		}

		private int Queue_PopByte()
		{
			int result;
			if (!(fullNotEmpty != 0) && head == tail) return -1;
			result = rxBuffer[tail];
			tail++;
			if (tail >= bufferSize) tail = 0;
			if (tail == head) fullNotEmpty = 0;
			return result;
		}

		private void ResetQueue()
		{
			head = 0;
			tail = 0;
			fullNotEmpty = 0;
			rxBuffer = new byte[bufferSize];
		}
		#endregion






	}
}
