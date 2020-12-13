using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModemAnalysis.Services
{
	public class TestStep : Communication
	{
		//public TestStep()
		//{
		//	serialPort = new SerialPort();
		//}
		//public bool GotoTestMode(string port)
		//{
		//	if (serialPort.IsOpen == true)
		//	{
		//		WritePort("SET,SYSTEM,TEST_MODE,3;"); //Access Test Mode in the device
		//		while (!OpenPort(port))
		//		{
		//			//Device restarts itself, so we are waiting for it to be connected again.
		//		}
		//		return true;
		//	}
		//	else
		//	{
		//		return false;
		//	}
		//}

		//public bool SendAT()
		//{
		//	if (serialPort.IsOpen == true)
		//	{
		//		WritePort("ATI");
		//		return true;
		//	}
		//	else
		//	{
		//		return false;
		//	}
			
		//}


		//public bool InitializeModem(string apn, string user, string pass)
		//{


		//	return false;
		//}


	}
}
