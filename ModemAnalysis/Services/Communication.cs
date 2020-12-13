using ModemAnalysis.Services;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ModemAnalysis
{
	public class Communication
	{
		public readonly SerialPort serialPort;

		Semaphore _sync = new Semaphore(1, 1);

		public event EventHandler<ProcessReceivedEventArgs> ProcessReceived;

		public class ProcessReceivedEventArgs : EventArgs
		{
			public string message { get; set; }
		}

		public Communication()
		{
			serialPort = new SerialPort();
		}
		
		public List<string> atInit = new List<string>()
		{
			//"AT+QICSGP=1,1,\"\",\"\",\"\"",

			"AT+CPIN?",
			"AT+COPS?",
			"AT+CREG?",
			"AT+QGMR"
		};


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
			try // Need to find better solution
			{
			serialPort.PortName = portName;
			serialPort.BaudRate = 115200;
			serialPort.Handshake = Handshake.None;
			serialPort.ReadTimeout = 500;
			serialPort.DataBits = 8;
			serialPort.Parity = Parity.None;
			serialPort.StopBits = StopBits.One;

			serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

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

		public bool GotoTestMode(string port)
		{
			if (serialPort.IsOpen == true)
			{
				WritePort("SET,SYSTEM,TEST_MODE,3;"); //Access Test Mode in the device
				while (!OpenPort(port))
				{
					//Device restarts itself, so we are waiting for it to be connected again.
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool SendAT()
		{
			if (serialPort.IsOpen == true)
			{
				WritePort("ATI");
				return true;
			}
			else
			{
				return false;
			}

		}


		public bool CheckModemStatus()
		{
			if (serialPort.IsOpen == true)
			{				
				foreach (var command in atInit)
				{
					WritePort(command);
					Thread.Sleep(100); // negrazu
				}
				return true;
			}
			else
			{
				return false;
			}

		}

		public bool ModemInit(string apn, string user, string pass)
		{
			if (serialPort.IsOpen == true)
			{
				foreach (var command in atInit)
				{
					WritePort(command);
					Thread.Sleep(200); // negrazu
					
				}return true;
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
				serialPort.Write($"{line}\r\n"); //Sends command to the device
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
			ProcessReceivedEventArgs args = new ProcessReceivedEventArgs();
			try
			{
				int readBytes = sp.BytesToRead;
				byte[] readBuffer = new byte[readBytes];
				sp.Read(readBuffer, 0, readBytes);

				for (int i = 0; i < readBuffer.Length; i++)
				{
					if (Queue_AddByte(readBuffer[i]) == (-1))
					{
						break;
					}
				}
				while (true)
				{
					int result = Queue_PopByte();
					if (result == (-1))
					{
						break;
					}
					if (Convert.ToChar(result) == '\r')
					{   
						CompressAndSendToMainWindow(args);
						lineBuffer[index] = Convert.ToChar(result);
						lineBuffer[index + 1] = '\n';
						index += 2;
						lineBuffer = new char[bufferSize];
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
			catch
			{

			}

		}

		private void CompressAndSendToMainWindow(ProcessReceivedEventArgs args)
		{
			char[] returnMessage = new char[index];
			for (var i = 0; i < index; i++)
				returnMessage[i] = lineBuffer[i];

			var returnMessageString = new string(returnMessage);
			if (!(returnMessageString == "" || returnMessageString == "OK"))
			{
				args.message = new string(returnMessage);
				OnProcessReceived(args);
			}
		}

		protected virtual void OnProcessReceived(ProcessReceivedEventArgs e)
		{
			EventHandler<ProcessReceivedEventArgs> handler = ProcessReceived;
			if (handler != null)
			{
				handler(this, e);
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
