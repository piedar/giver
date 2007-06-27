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
		public SendingHandler()
		{
		}


		public void SendFile(Service service, string file)
		{
			UriBuilder urib = new UriBuilder("http", service.Address.ToString(), (int)service.Port);
			Logger.Debug("Sending request to URI: {0}", urib.Uri.ToString());
			System.Net.HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(urib.Uri);

			string fileName = System.IO.Path.GetFileName(file);
			// Send request to send the file
			request.Method = "POST";
			request.Headers.Set("Request", "Send");
			request.Headers.Set("Type", "File");
			request.Headers.Set("Count", "1");
			request.Headers.Set("Name", fileName);
			request.Headers.Set("Size", "2034");
			request.ContentLength = 0;
			request.GetRequestStream().Close();

			// Read the response to the request
			WebResponse response = request.GetResponse();

			if(response.Headers["Response"].CompareTo("OKToSend") == 0) {
				Logger.Debug("Response was OKToSend");

				string sessionID = response.Headers["SessionID"];
				response.Close();
				
				request = (HttpWebRequest) HttpWebRequest.Create(urib.Uri);
				request.Method = "POST";
				request.Headers.Set("SessionID", sessionID);
				request.Headers.Set("Request", "Payload");
				request.Headers.Set("Type", "File");
				request.Headers.Set("Count", "1");
				request.Headers.Set("Name", fileName);

				try {
					System.IO.FileStream filestream = new FileStream(file, FileMode.Open);
					request.Headers.Set("Size", filestream.Length.ToString());
					request.ContentLength = filestream.Length;
					Stream stream = request.GetRequestStream();
					
					int sizeRead = 0;
					int totalRead = 0;
					byte[] buffer = new byte[2048];

					do {
						sizeRead = filestream.Read(buffer, 0, 2048);
						totalRead += sizeRead;
						if(sizeRead > 0) {
							stream.Write(buffer, 0, sizeRead);
						}
					} while(sizeRead == 2048);
					Logger.Debug("We Read from the file {0} bytes", totalRead);
					Logger.Debug("The content length is {0} bytes", filestream.Length);

					stream.Close();
				} catch (Exception e) {
					Logger.Debug("Exception when sending file: {0}", e.Message);
					Logger.Debug("Exception {0}", e);
				}

				// go ahead and send the file			
			} else {
				Logger.Debug("Not OKToSend");
			}
		}

	}
}
