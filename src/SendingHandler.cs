////***********************************************************************
//// *  SendingHandler.cs
//// *
//// *  Copyright (C) 2007 Novell, Inc.
//// *
//// *  This program is free software; you can redistribute it and/or
//// *  modify it under the terms of the GNU General Public
//// *  License as published by the Free Software Foundation; either
//// *  version 2 of the License, or (at your option) any later version.
//// *
//// *  This program is distributed in the hope that it will be useful,
//// *  but WITHOUT ANY WARRANTY; without even the implied warranty of
//// *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//// *  General Public License for more details.
//// *
//// *  You should have received a copy of the GNU General Public
//// *  License along with this program; if not, write to the Free
//// *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
//// *
//// **********************************************************************

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Giver
{
	public class SendingHandler
	{
		private Service service;
		private System.Net.HttpWebRequest request;
		//private TcpClient client;
		private Stream stream;
		// private NetworkStream stream;
		
		public SendingHandler(Service service)
		{
			this.service = service;
			UriBuilder urib = new UriBuilder("http", service.Address.ToString(), (int)service.Port);
//			Uri uri = new Uri(requestURI);
			Logger.Debug("Sending request to URI: {0}", urib.Uri.ToString());
			request = (HttpWebRequest) HttpWebRequest.Create(urib.Uri);
    		//client = new TcpClient(service.Address.ToString(), (int)service.Port);
			request.Method = "POST";
			stream = request.GetRequestStream();
		    //stream = client.GetStream();
		}

		public void SendFile(string fileName)
		{
			// Write PayloadInfo
			
		    // Send the message to the connected TcpServer. 
			Write(stream, fileName);
			Write(stream, (uint)PayloadType.File);
		    //stream.Write(data, 0, data.Length);

		    // Close everything.
		    stream.Close();
			
			//stream.Close();         
		    //client.Close();
		}

        unsafe private void MarshalUInt (Stream stream, byte *data)
        {
			byte[] dst = new byte[4];

			dst[0] = data[0];
			dst[1] = data[1];
			dst[2] = data[2];
			dst[3] = data[3];

			stream.Write (dst, 0, 4);
        }

        unsafe private void Write (Stream stream, int val)
        {
            MarshalUInt (stream, (byte*)&val);
        }

        unsafe private void Write (Stream stream, uint val)
        {
            MarshalUInt (stream, (byte*)&val);
        }

        public void Write (Stream stream, string val)
        {
            byte[] utf8_data = Encoding.UTF8.GetBytes (val);
            Write (stream, (uint)utf8_data.Length);
            stream.Write (utf8_data, 0, utf8_data.Length);
            stream.WriteByte (0); //NULL string terminator
        }
	}
}



/*
static void Connect(String server, String message) 
{
  try 
  {
    // Create a TcpClient.
    // Note, for this client to work you need to have a TcpServer 
    // connected to the same address as specified by the server, port
    // combination.
    Int32 port = 13000;
    TcpClient client = new TcpClient(server, port);
    
    // Translate the passed message into ASCII and store it as a Byte array.
    Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);         

    // Get a client stream for reading and writing.
   //  Stream stream = client.GetStream();
    
    NetworkStream stream = client.GetStream();

    // Send the message to the connected TcpServer. 
    stream.Write(data, 0, data.Length);

    Console.WriteLine("Sent: {0}", message);         

    // Receive the TcpServer.response.
    
    // Buffer to store the response bytes.
    data = new Byte[256];

    // String to store the response ASCII representation.
    String responseData = String.Empty;

    // Read the first batch of the TcpServer response bytes.
    Int32 bytes = stream.Read(data, 0, data.Length);
    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
    Console.WriteLine("Received: {0}", responseData);         

    // Close everything.
    stream.Close();         
    client.Close();         
  } 
  catch (ArgumentNullException e) 
  {
    Console.WriteLine("ArgumentNullException: {0}", e);
  } 
  catch (SocketException e) 
  {
    Console.WriteLine("SocketException: {0}", e);
  }
    
  Console.WriteLine("\n Press Enter to continue...");
  Console.Read();
}
*/