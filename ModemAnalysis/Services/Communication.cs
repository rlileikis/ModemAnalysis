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

		public event EventHandler<ProcessReceivedEventArgs> ProcessReceived;

		public class ProcessReceivedEventArgs : EventArgs
		{
			public string Message { get; set; }
		}

		public Communication()
		{
			serialPort = new SerialPort();
		}
		
		public List<string> modInitList = new List<string>()
		{
			"AT+COPS=0",
			"AT+QGMR"
		};

		public List<string> modStatusList = new List<string>()
		{
			"AT+QCFGEXT=\"fota_apn\"",
			"AT+CPIN?",
			"AT+COPS?",
			"AT+CREG?",
			"AT+CEREG?",
			"AT+CSQ",
			"AT+QGMR"
		};

		public List<string> deviceOrModemList = new List<string>()
		{
			"GET,ID,MODEM;",
			"AT+QGMR"
		};


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
					//Need to rethink later
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool StartDfota(string port, int dfotaUrlIndex)
		{
			if (serialPort.IsOpen == true)
			{
				//CheckModemStatus();
				WritePort($"AT+QFOTADL=\"{DfotaIndexToUrl(dfotaUrlIndex)}\"");
				return true;
			}
			else
			{
				return false;
			}
		}

		private string DfotaIndexToUrl(int index)
		{
			switch (index)
			{
				case 0:
					return "https://ruptelafwsa.blob.core.windows.net/fwsa/BG96FW/BG96MAR02A07M1G_01.016.01.016-BG96MAR02A07M1G_01.018.01.018.bin";

				case 1:
					return "https://ruptelafwsa.blob.core.windows.net/fwsa/BG96FW/BG96MAR02A07M1G_01.018.01.018-BG96MAR02A07M1G_01.016.01.016.bin";

				default:
					return "";
	
			}
		}



		public bool CheckModemStatus()
		{
			if (serialPort.IsOpen == true)
			{				
				foreach (var command in modStatusList)
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
				Thread.Sleep(200);
				WritePort($"AT+QCFGEXT=\"fota_apn\",0,\"{apn}\",\"{user}\",\"{pass}\"");
				Thread.Sleep(100);
				foreach (var command in modInitList)
				{
					WritePort(command);
					Thread.Sleep(100); // negrazu
					
				}return true;
			}
			else
			{
				return false;
			}

		}

		public bool CheckIfItIsDeviceOrModem()
		{
			if (serialPort.IsOpen == true)
			{
				foreach (var command in deviceOrModemList)
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
				args.Message = new string(returnMessage);
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
