////***********************************************************************
//// *  ReceivingHandler.cs
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
	/// <summary>
	/// Receives and handles all incoming giver requests
	/// </summary>	
	public class ReceivingHandler
	{
		private TcpClient client;
		private NetworkStream stream;
		private PayloadInfo payloadInfo;

		public ReceivingHandler(TcpClient client)
		{
			this.client = client;
			this.stream = client.GetStream();
			ReadPayloadInfo();
			Logger.Debug("We are being send the file: {0}", payloadInfo.Name);
		}

		private void ReadPayloadInfo()
		{
			try {
				int size;

				size = ReadInt();
				string name = ReadString(size);
				PayloadType type = (PayloadType) ReadInt();

				payloadInfo = new PayloadInfo(name, type);
			} catch (Exception e) {
				Logger.Debug("Errror reading the PayloadInfo : {0}", e.Message);
				payloadInfo = null;
			}
		}

		private int ReadInt()
		{
			int bytesRead;
			Byte[] buffer = new Byte[4];

			bytesRead = stream.Read(buffer, 0, 4);
			if(bytesRead != 4)
				throw new Exception("Unable to read 4 bytes");

			return (int)buffer[0];
		}	

		private string ReadString(int size)
		{
			int bytesRead;

			// Check for a very large string size here and blow if it's too big
			if(size > 32768)
				throw new Exception("String too big to read");

			Byte[] buffer = new Byte[size];

			bytesRead = stream.Read(buffer, 0, size);
			if(bytesRead < size)
				throw new Exception("Unable to read all bytes from stream");

			return System.Convert.ToBase64String(buffer);
		}
	}
}



/*
class MyTcpListener
{
  public static void Main()
  { 
    TcpListener server=null;   
    try
    {
      // Set the TcpListener on port 13000.
      Int32 port = 13000;
      IPAddress localAddr = IPAddress.Parse("127.0.0.1");
      
      // TcpListener server = new TcpListener(port);
      server = new TcpListener(localAddr, port);

      // Start listening for client requests.
      server.Start();
         
      // Buffer for reading data
      Byte[] bytes = new Byte[256];
      String data = null;

      // Enter the listening loop.
      while(true) 
      {
        Console.Write("Waiting for a connection... ");
        
        // Perform a blocking call to accept requests.
        // You could also user server.AcceptSocket() here.
        TcpClient client = server.AcceptTcpClient();            
        Console.WriteLine("Connected!");

        data = null;

        // Get a stream object for reading and writing
        NetworkStream stream = client.GetStream();

        int i;

        // Loop to receive all the data sent by the client.
        while((i = stream.Read(bytes, 0, bytes.Length))!=0) 
        {   
          // Translate data bytes to a ASCII string.
          data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
          Console.WriteLine("Received: {0}", data);
       
          // Process the data sent by the client.
          data = data.ToUpper();

          byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

          // Send back a response.
          stream.Write(msg, 0, msg.Length);
          Console.WriteLine("Sent: {0}", data);            
        }
         
        // Shutdown and end connection
        client.Close();
      }
    }
    catch(SocketException e)
    {
      Console.WriteLine("SocketException: {0}", e);
    }
    finally
    {
       // Stop listening for new clients.
       server.Stop();
    }

      
    Console.WriteLine("\nHit enter to continue...");
    Console.Read();
  }   
}

*/
