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
using Notifications;
using Mono.Unix;

namespace Giver
{
	public class SessionData
	{
		public string sessionID;
		public string name;
		public long size;
		public string type;
		public int count;
		public string userName;
		public HttpListenerContext context;

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
		private SessionData pendingSession;
		private Notification currentNotification;

		public RequestHandler()
		{
			sessions = new Dictionary<string, SessionData> ();
			pendingSession = null;
			currentNotification = null;
		}

		public void HandleRequest(HttpListenerContext context)
		{
			Logger.Debug("Request:{0} came in from {1}", context.Request.Headers[Protocol.Request], 
													context.Request.RemoteEndPoint.Address.ToString());

			if(context.Request.Headers[Protocol.Request].CompareTo(Protocol.Send) == 0) {
				HandleSendRequest(context);
			}
			else if(context.Request.Headers[Protocol.Request].CompareTo(Protocol.Payload) == 0) {
				HandlePayload(context);
			}
			else if(context.Request.Headers[Protocol.Request].CompareTo(Protocol.Photo) == 0) {
				HandlePhoto(context);
			}
			else {
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				context.Response.StatusDescription = Protocol.ResponseUnknown;
				context.Response.Close();
			}
		}

		private void HandleSendRequest(HttpListenerContext context)
		{
			Logger.Debug("HandleSendRequest called");

			if(pendingSession != null) {
				Logger.Debug("HandleSendRequest: Found a pending Session... closing it");
				pendingSession.context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				pendingSession.context.Response.StatusDescription = Protocol.ResponseDeclined;
				pendingSession.context.Response.OutputStream.Close();
				pendingSession.context.Response.Close();
				pendingSession = null;
			}

			if(currentNotification != null) {
				Logger.Debug("HandleSendRequest: Found a notification... closing it");
				currentNotification.Close();
				currentNotification = null;
			}

			// get the information about what wants to be sent
			SessionData sd = new SessionData();
			sd.count = Convert.ToInt32(context.Request.Headers[Protocol.Count]);
			sd.name = context.Request.Headers[Protocol.Name];
			sd.type = context.Request.Headers[Protocol.Type];
			sd.userName = context.Request.Headers[Protocol.UserName];
			sd.size = Convert.ToInt32(context.Request.Headers[Protocol.Size]);
			sd.sessionID = System.Guid.NewGuid().ToString();
			sd.context = context;

			try {
				// place this request into the pending requests and notify the user of the request
				string summary = String.Format(Catalog.GetString("{0} wants to \"Give\""), sd.userName);
				string body = String.Format(Catalog.GetString("{0}\nSize: {1} bytes"), sd.name, sd.size);

				Notification notify = new Notification(	summary, 
														body,
														Utilities.GetIcon ("giver-48", 48));
				notify.Timeout = 60000;
				

				notify.AddAction("Accept", Catalog.GetString("Accept"), AcceptNotificationHandler);
				notify.AddAction("Decline", Catalog.GetString("Decline"), DeclineNotificationHandler);
				notify.Closed += ClosedNotificationHandler;
				currentNotification = notify;
				pendingSession = sd;

				Gtk.Application.Invoke( delegate {
					Application.ShowAppNotification(notify);
					Gnome.Sound.Play(Path.Combine(Giver.Defines.SoundDir, "notify.wav"));
				} );
			} catch (Exception e) {
				Logger.Debug("Exception attempting to notify {0}", e);

				pendingSession.context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				pendingSession.context.Response.StatusDescription = Protocol.ResponseDeclined;
				pendingSession.context.Response.OutputStream.Close();
				pendingSession.context.Response.Close();
				pendingSession.context = null;
				pendingSession = null;
				currentNotification = null;
			}			
		}


        /// <summary>
        /// AcceptNotificationHandler
        /// Handles notifications
        /// </summary>
        private void AcceptNotificationHandler (object o, ActionArgs args)
        {
			if(pendingSession != null) {
				pendingSession.context.Response.Headers.Set(Protocol.SessionID, pendingSession.sessionID);
				sessions.Add(pendingSession.sessionID, pendingSession);
				pendingSession.context.Response.StatusCode = (int)HttpStatusCode.OK;
				pendingSession.context.Response.StatusDescription = Protocol.ResponseOKToSend;
				pendingSession.context.Response.OutputStream.Close();
				pendingSession.context.Response.Close();
				pendingSession.context = null;
				pendingSession = null;
			}
			currentNotification = null;
        }


        /// <summary>
        /// DeclineNotificationHandler
        /// Handles notifications
        /// </summary>
        private void DeclineNotificationHandler (object o, ActionArgs args)
        {
			if(pendingSession != null) {
				pendingSession.context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				pendingSession.context.Response.StatusDescription = Protocol.ResponseDeclined;
				pendingSession.context.Response.OutputStream.Close();
				pendingSession.context.Response.Close();
				pendingSession.context = null;
				pendingSession = null;
			}
			currentNotification = null;
        }


        /// <summary>
        /// ClosedNotificationHandler
        /// Handles notifications
        /// </summary>
        private void ClosedNotificationHandler (object o, EventArgs args)
        {
			if(pendingSession != null) {
				pendingSession.context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				pendingSession.context.Response.StatusDescription = Protocol.ResponseDeclined;
				pendingSession.context.Response.OutputStream.Close();
				pendingSession.context.Response.Close();
				pendingSession.context = null;
				pendingSession = null;
			}
			currentNotification = null;
        }


		private void HandlePayload(HttpListenerContext context)
		{
			// get the information about what wants to be sent

			string sessionID = context.Request.Headers[Protocol.SessionID];
			if( (sessionID == null) || (sessionID.Length < 1) ) {
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				context.Response.StatusDescription = Protocol.ResponseMissingSession;
				context.Response.Close();			
				return;
			}

			if(!sessions.ContainsKey(sessionID)) {
				context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				context.Response.StatusDescription = Protocol.ResponseInvalidSession;
				context.Response.Close();			
				return;
			}

			SessionData sd = sessions[sessionID];
			if(sd.name.CompareTo(context.Request.Headers[Protocol.Name]) == 0) {
				byte[] buffer = new byte[2048];
				int readCount = 0;
				long totalRead = 0;
				int fileInstance = 1;

				// Create the local file path
				string newFilePath = Path.Combine(System.Environment.GetFolderPath
												(System.Environment.SpecialFolder.Desktop), sd.name);
				// Loop until there is no file conflict
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
			context.Response.StatusDescription = Protocol.ResponsePayloadReceived;
			context.Response.OutputStream.Close();
			context.Response.Close();
			sessions.Remove(sessionID);
		}


		private void HandlePhoto(HttpListenerContext context)
		{
			// get the information about what wants to be sent
			if(!Application.Preferences.HasPhoto) {
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				context.Response.StatusDescription = Protocol.ResponseNoPhoto;
				context.Response.Close();			
				return;
			}

			if(Application.Preferences.PhotoIsUri)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				context.Response.StatusDescription = Application.Preferences.PhotoLocation;
				context.Response.Close();			
				return;
			}

			try
			{
				FileStream fs = new FileStream(Application.Preferences.PhotoLocation, FileMode.Open);
				Stream stream = context.Response.OutputStream;
				context.Response.ContentLength64 = fs.Length;
				context.Response.StatusCode = (int)HttpStatusCode.OK;
				context.Response.StatusDescription = Protocol.ResponsePhotoSent;

				int sizeRead = 0;
				int totalRead = 0;
				byte[] buffer = new byte[2048];

				do {
					sizeRead = fs.Read(buffer, 0, 2048);
					totalRead += sizeRead;
					if(sizeRead > 0) {
						stream.Write(buffer, 0, sizeRead);
					}
				} while(sizeRead == 2048);

				stream.Close();
				fs.Close();
				context.Response.Close();

			} catch (Exception e) {
				Logger.Debug("Exception when sending photo {0}", e.Message);
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				context.Response.StatusDescription = Protocol.ResponseNoPhoto;
				context.Response.Close();			
			}
		}
	}
}


