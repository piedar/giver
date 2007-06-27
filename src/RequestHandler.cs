////***********************************************************************
//// *  RequestHandler.cs
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
using System.Collections.Generic;

namespace Giver
{
	public class SessionData
	{
		public string sessionID;
		public string name;
		public long size;
		public string type;
		public int count;

		public SessionData()
		{
		}
	}


	/// <summary>
	/// Receives and handles all incoming giver requests
	/// </summary>	
	public class RequestHandler
	{
		private Dictionary<string, SessionData> sessions;

		public RequestHandler()
		{
			sessions = new Dictionary<string, SessionData> ();
		}

		public void HandleRequest(HttpListenerContext context)
		{
			Logger.Debug("Request came in {0}", context.Request.Headers["Request"]);

			if(context.Request.Headers["Request"].CompareTo("Send") == 0) {
				HandleSendRequest(context);
			}
			else if(context.Request.Headers["Request"].CompareTo("Payload") == 0) {
				HandlePayload(context);
			}
			else {
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				context.Response.StatusDescription = "Unknown request";
				context.Response.Close();
			}
		}

		private void HandleSendRequest(HttpListenerContext context)
		{
			// get the information about what wants to be sent

			SessionData sd = new SessionData();
			sd.count = Convert.ToInt32(context.Request.Headers["Count"]);
			sd.name = context.Request.Headers["Name"];
			sd.type = context.Request.Headers["Type"];
			sd.size = Convert.ToInt32(context.Request.Headers["Size"]);
			sd.sessionID = System.Guid.NewGuid().ToString();


			// Ask the user to accept at this point, then do the following if they accept
			context.Response.Headers.Set("Response", "OKToSend");
			context.Response.Headers.Set("SessionID", sd.sessionID);
			sessions.Add(sd.sessionID, sd);
			context.Response.StatusCode = (int)HttpStatusCode.OK;
			context.Response.OutputStream.Close();
			context.Response.Close();
		}

		private void HandlePayload(HttpListenerContext context)
		{
			// get the information about what wants to be sent

			string sessionID = context.Request.Headers["SessionID"];
			if( (sessionID == null) || (sessionID.Length < 1) ) {
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				context.Response.StatusDescription = "No SessionID in HTTP Header";
				context.Response.Close();			
				return;
			}

			if(!sessions.ContainsKey(sessionID)) {
				context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				context.Response.StatusDescription = "SessionID is invalid";
				context.Response.Close();			
				return;
			}

			SessionData sd = sessions[sessionID];
			if(sd.name.CompareTo(context.Request.Headers["Name"]) == 0) {
				// start receiving the file
				byte[] buffer = new byte[2048];
				int readCount = 0;
				long totalRead = 0;
				int fileInstance = 1;

				string newFilePath = Path.Combine(System.Environment.GetFolderPath
												(System.Environment.SpecialFolder.Desktop), sd.name);
				while(File.Exists(newFilePath)) {
					newFilePath = Path.Combine(System.Environment.GetFolderPath
											(System.Environment.SpecialFolder.Desktop), 
											String.Format("{0}({1}){2}",
													Path.GetFileNameWithoutExtension(sd.name),
													fileInstance,
													Path.GetExtension(sd.name)));
					fileInstance++;
				}

				FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);

				do {
					readCount = context.Request.InputStream.Read(buffer, 0, 2048);
					totalRead += readCount;
					if(readCount > 0)
						fs.Write(buffer, 0, readCount);					
				} while( (readCount > 0) && (totalRead <= context.Request.ContentLength64) );

				Logger.Debug("We Read from the input stream {0} bytes", totalRead);
				Logger.Debug("The content length is {0} bytes", context.Request.ContentLength64);
				fs.Close();
			}

			context.Response.StatusCode = (int)HttpStatusCode.OK;
			context.Response.OutputStream.Close();
			context.Response.Close();
		}

	}
}


