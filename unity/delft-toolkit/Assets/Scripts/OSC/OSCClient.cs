//
//	  UnityOSC - Open Sound Control interface for the Unity3d game engine
//
//	  Copyright (c) 2012 Jorge Garcia Martin
//
// 	  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// 	  documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// 	  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// 	  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// 	  The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// 	  of the Software.
//
// 	  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// 	  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// 	  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// 	  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// 	  IN THE SOFTWARE.
//

using System;
using System.Net;
using UnityEngine;

#if NETFX_CORE
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else
using System.Net.Sockets;
#endif



namespace UnityOSC
{
	/// <summary>
	/// Dispatches OSC messages to the specified destination address and port.
	/// </summary>
	
	public class OSCClient
	{
		#region Constructors
		public OSCClient (IPAddress address, int port)
		{
			_ipAddress = address;
			_port = port;
			Connect();
		}
		#endregion
		
		#region Member Variables
		private IPAddress _ipAddress;
		private int _port;
#if NETFX_CORE
		DatagramSocket socket;		
		HostName _hostname;
#else
		private UdpClient _udpClient;
#endif
		#endregion
		
		#region Properties
		public IPAddress ClientIPAddress
		{
			get
			{
				return _ipAddress;
			}
		}
		
		public int Port
		{
			get
			{
				return _port;
			}
		}
		#endregion
	
		#region Methods
		/// <summary>
		/// Connects the client to a given remote address and port.
		/// </summary>
		public void Connect()
		{
#if NETFX_CORE
			Debug.Log("OSCClient Connect start...");
			_hostname = new HostName(_ipAddress.ToString());
			socket = new DatagramSocket();

			Debug.Log("exit start:"+_ipAddress.ToString());
#else
			if(_udpClient != null) Close();
			_udpClient = new UdpClient();
			try
			{
				_udpClient.Connect(_ipAddress, _port);	
			}
			catch
			{
				throw new Exception(String.Format("Can't create client at IP address {0} and port {1}.", _ipAddress,_port));
			}
#endif		
		}
		
		/// <summary>
		/// Closes the client.
		/// </summary>
		public void Close()
		{
#if NETFX_CORE
			socket.Dispose();
			Debug.Log("OSC CLIENT UWP CLOSE");
#else
			_udpClient.Close();
			_udpClient = null;
#endif
		}
		
		/// <summary>
		/// Sends an OSC packet to the defined destination and address of the client.
		/// </summary>
		/// <param name="packet">
		/// A <see cref="OSCPacket"/>
		/// </param>
#if NETFX_CORE
		public async void Send(OSCPacket packet)
		{
			byte[] data = packet.BinaryData;
			//Debug.Log("OSCCLIENT-SEND:" + System.Text.Encoding.ASCII.GetString(data));

			using (var writer = new DataWriter(await socket.GetOutputStreamAsync(_hostname, _port.ToString()))){
				try 
				{
					writer.WriteBytes(data);
					await writer.StoreAsync();

				}
				catch
				{
					throw new Exception(String.Format("Can't send OSC packet to client {0} : {1}:Length{2}", _ipAddress, _port,data.Length));
				}


			}

		}
#else
		public void Send(OSCPacket packet)
		{
			byte[] data = packet.BinaryData;
			try 
			{
				_udpClient.Send(data, data.Length);
			}
			catch
			{
				throw new Exception(String.Format("Can't send OSC packet to client {0} : {1}", _ipAddress, _port));
			}
		}
#endif
		#endregion
	}
}

