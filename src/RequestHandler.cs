/***************************************************************************
 *  RequestHandler.cs
 *
 *  Copyright (C) 2007 Novell, Inc.
 *  Written by Calvin Gaisford <calvinrg@gmail.com>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Notifications;
using Mono.Unix;
using System.Diagnostics;

namespace Giver
{
	public class SessionData
	{
		public string sessionID;
		public string name;
		public long size;
		public string type;
		public int count;
		public int countReceived;
		public long bytesReceived;
		public string userName;
		public HttpListenerContext context;
		public Dictionary<string, string> renameFolders;

		public SessionData()
		{
			countReceived = 0;
			renameFolders = new Dictionary<string,string> ();
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
		private System.Object sessionLocker;

		public RequestHandler()
		{
			sessionLocker = new Object();
			sessions = new Dictionary<string, SessionData> ();
			pendingSession = null;
			currentNotification = null;
		}

		public void HandleRequest(HttpListenerContext context)
		{
			//Logger.Debug("Request:{0} came in from {1}", context.Request.Headers[Protocol.Request], 
			//										context.Request.RemoteEndPoint.Address.ToString());

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
			//Logger.Debug("RECEIVE: HandleSendRequest called");

			if(pendingSession != null) {
				Logger.Debug("RECEIVE: HandleSendRequest: Found a pending Session... closing it");
				pendingSession.context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				pendingSession.context.Response.StatusDescription = Protocol.ResponseDeclined;
				pendingSession.context.Response.OutputStream.Close();
				pendingSession.context.Response.Close();
				pendingSession = null;
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

			//Logger.Debug("RECEIVE: Preparing Notification");
			try {
				// place this request into the pending requests and notify the user of the request
				string summary = String.Format(Catalog.GetString("{0} wants to give"), sd.userName);
				string body;
				if(sd.count == 1) {
//				*** TOMBOY HACK **
					if(sd.type.CompareTo(Protocol.ProtocolTypeTomboy) == 0) {
						body = String.Format(Catalog.GetString("Tomboy Note:\n{0}"), sd.name);
					}
					else
						body = String.Format(Catalog.GetString("{0}\nSize: {1} bytes"), sd.name, sd.size);

				}
//				*** END TOMBOY HACK **
//						body = String.Format(Catalog.GetString("{0}\nSize: {1} bytes"), sd.name, sd.size);
				else
					body = String.Format(Catalog.GetString("{0} files\nSize: {1} bytes"), sd.count, sd.size);

				pendingSession = sd;

				//Logger.Debug("RECEIVE: About to do a Gtk.Application.Invoke for the notify dude.");
				Gtk.Application.Invoke( delegate {


					//Logger.Debug("RECEIVE: Inside the Gtk.Application.Invoke dude");
					Gdk.Pixbuf pixbuf = null;

					if(sd.type.CompareTo(Protocol.ProtocolTypeTomboy) == 0)
						pixbuf = Gtk.IconTheme.Default.LoadIcon ("tomboy", 48, 0);
					
					if(pixbuf == null)
						pixbuf = Utilities.GetIcon ("giver-48", 48);

					Notification notify = new Notification(	summary,
															body,
															pixbuf);

					notify.Timeout = 60000;

					notify.AddAction("Accept", Catalog.GetString("Accept"), AcceptNotificationHandler);
					notify.AddAction("Decline", Catalog.GetString("Decline"), DeclineNotificationHandler);
					notify.Closed += ClosedNotificationHandler;

					if(currentNotification != null) {
						Logger.Debug("RECEIVE: HandleSendRequest: Found a notification... closing it");
						currentNotification.Close();
						currentNotification = null;
					}

					currentNotification = notify;
					Application.ShowAppNotification(notify);
					Gnome.Sound.Play(Path.Combine(Giver.Defines.SoundDir, "notify.wav"));
				} );
			} catch (Exception e) {
				Logger.Debug("RECEIVE: Exception attempting to notify {0}", e);

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
			lock(sessionLocker) {
				if(pendingSession != null) {
					pendingSession.context.Response.Headers.Set(Protocol.SessionID, pendingSession.sessionID);
					sessions.Add(pendingSession.sessionID, pendingSession);
					pendingSession.countReceived = 0;
					pendingSession.bytesReceived = 0;
					pendingSession.context.Response.StatusCode = (int)HttpStatusCode.OK;
					pendingSession.context.Response.StatusDescription = Protocol.ResponseOKToSend;
					pendingSession.context.Response.OutputStream.Close();
					pendingSession.context.Response.Close();
					pendingSession.context = null;
					pendingSession = null;
				}
			}
			currentNotification = null;
        }


        /// <summary>
        /// DeclineNotificationHandler
        /// Handles notifications
        /// </summary>
        private void DeclineNotificationHandler (object o, ActionArgs args)
        {
			lock(sessionLocker) {
				if(pendingSession != null) {
					pendingSession.context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
					pendingSession.context.Response.StatusDescription = Protocol.ResponseDeclined;
					pendingSession.context.Response.OutputStream.Close();
					pendingSession.context.Response.Close();
					pendingSession.context = null;
					pendingSession = null;
				}
			}
			currentNotification = null;
        }


        /// <summary>
        /// ClosedNotificationHandler
        /// Handles notifications
        /// </summary>
        private void ClosedNotificationHandler (object o, EventArgs args)
        {
			lock(sessionLocker) {
				if(pendingSession != null) {
					pendingSession.context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
					pendingSession.context.Response.StatusDescription = Protocol.ResponseDeclined;
					pendingSession.context.Response.OutputStream.Close();
					pendingSession.context.Response.Close();
					pendingSession.context = null;
					pendingSession = null;
				}
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

			SessionData sd;

			lock(sessionLocker) {
				if(!sessions.ContainsKey(sessionID)) {
					context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
					context.Response.StatusDescription = Protocol.ResponseInvalidSession;
					context.Response.Close();			
					return;
				}
				else
					sd = sessions[sessionID];
			}

			try{

				// type context.Request.Headers[Protocol.Type]
				// count context.Request.Headers[Protocol.Count]
				string fileName = context.Request.Headers[Protocol.Name];
				string relativePath = context.Request.Headers[Protocol.RelativePath];

				byte[] buffer = new byte[8192];
				int readCount = 0;
				long totalRead = 0;
				int fileInstance = 1;

				// Create the local file path
				string basePath;
				if(relativePath.Length > 0)
				{
					Logger.Debug("RECEIVE: We got a file with a relativePath of {0}", relativePath);
					// first check to see if we have fixed up the base of this path
					string localPath = Path.Combine(Application.Preferences.ReceiveFileLocation, relativePath);
					string rootPath = Directory.GetParent(localPath).FullName;

					string keyPath = relativePath;
					int pathIndex = relativePath.IndexOf(Path.DirectorySeparatorChar);
					if(pathIndex > 0)
						keyPath = relativePath.Substring(0, pathIndex);

					// Logger.Debug("RECEIVE: Looking for the key String {0}", keyPath);

					if(sd.renameFolders.ContainsKey(keyPath))
					{
						if(keyPath.CompareTo(relativePath) == 0) {
							relativePath = sd.renameFolders[keyPath];
						} else {
							string newPath = sd.renameFolders[keyPath] + relativePath.Substring(keyPath.Length);
							relativePath = newPath;
						}
					} else if(rootPath.CompareTo(Application.Preferences.ReceiveFileLocation) == 0) {
						//Logger.Debug("RECEIVE: This is a root folder... do some checking");
						// this is a root folder to be created in the target location
						if(Directory.Exists(localPath)) {
							//Logger.Debug("RECEIVE: Oh Darn, it already exists at {0}", localPath);
							string newRelativePath = relativePath;	
							fileInstance = 1;
							while(Directory.Exists(localPath)) {
								newRelativePath = String.Format("{0}({1})", relativePath, fileInstance);
								localPath = Path.Combine(Application.Preferences.ReceiveFileLocation, newRelativePath);
								fileInstance++;
							}
							//Logger.Debug("RECEIVE: replacing {0} with {1}", relativePath, newRelativePath);
							sd.renameFolders.Add(relativePath, newRelativePath);
							relativePath = newRelativePath;
						} else {
							// There is no conflict but we need to set up this parent
							// for all other incoming files
							sd.renameFolders.Add(relativePath, relativePath);
						}
					}
					Logger.Debug("RECEIVE: Relative Path is now {0}", relativePath);

					basePath = Path.Combine(Application.Preferences.ReceiveFileLocation, relativePath);
				}
				else
					basePath = Application.Preferences.ReceiveFileLocation;


				if(!Directory.Exists(basePath))
					Directory.CreateDirectory(basePath);

				string newFilePath = Path.Combine(basePath, fileName);

//				*** TOMBOY HACK
				if(Path.GetExtension(fileName).CompareTo(".note") == 0) {
					basePath = "/tmp";
					newFilePath = Path.Combine("/tmp", fileName);
				}

				fileInstance++;
				// Loop until there is no file conflict
				while(File.Exists(newFilePath)) {
					newFilePath = Path.Combine(basePath, 
													String.Format("{0}({1}){2}",
													Path.GetFileNameWithoutExtension(fileName),
													fileInstance,
													Path.GetExtension(fileName)));
					fileInstance++;
				}

				FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);

				do {
					readCount = context.Request.InputStream.Read(buffer, 0, 8192);
					totalRead += readCount;
					if(readCount > 0) {
						fs.Write(buffer, 0, readCount);
						fs.Flush();
					}					
				} while( (readCount > 0) && (totalRead <= context.Request.ContentLength64) );

				//Logger.Debug("RECEIVE: We Read from the input stream {0} bytes", totalRead);
				//Logger.Debug("RECEIVE: The content length is {0} bytes", context.Request.ContentLength64);
				fs.Close();

				sd.countReceived++;
				sd.bytesReceived += totalRead;

				Logger.Debug("RECEIVE: We've read {0} files and {1} bytes", sd.countReceived, sd.bytesReceived);

//				*** TOMBOY HACK
				if(Path.GetExtension(newFilePath).CompareTo(".note") == 0) {
					Process p = new Process ();
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.RedirectStandardOutput = true;
					// TODO: Is calling /sbin/lsmod safe enough?  i.e., can we be guaranteed it's gonna be there?
					p.StartInfo.FileName = "tomboy";
					p.StartInfo.Arguments = "--open-note " + newFilePath;
					p.StartInfo.CreateNoWindow = true;
					p.Start ();
					p.StandardOutput.ReadToEnd ();
					p.WaitForExit ();
				}

			
				// if we've received everything we agreed to, remove the session
				if( (sd.count <= sd.countReceived) || (sd.size <= sd.bytesReceived) ) {
					Logger.Debug("RECEIVE: We've either received the number or bytes we should... no more!");
					lock(sessionLocker) {
						sessions.Remove(sessionID);
					}

					//Logger.Debug("RECEIVE: About to do a Gtk.Application.Invoke for the notify dude.");
					Gtk.Application.Invoke( delegate {
						string summary = String.Format(Catalog.GetString("{0} is done giving"), sd.userName);
						string body = String.Format(Catalog.GetString("You have received all of the sent files...Welcome to the sow shul!"));

						//Logger.Debug("RECEIVE: Inside the Gtk.Application.Invoke dude");
						Notification notify = new Notification(	summary, 
																body,
																Utilities.GetIcon ("giver-48", 48));

						currentNotification = notify;
						Application.ShowAppNotification(notify);
						Gnome.Sound.Play(Path.Combine(Giver.Defines.SoundDir, "notify.wav"));
					} );
				}
		
			} catch (Exception e) {
				Logger.Debug("RECEIVE: Exception handing payload {0}", e);
			}
			context.Response.StatusCode = (int)HttpStatusCode.OK;
			context.Response.StatusDescription = Protocol.ResponsePayloadReceived;
			context.Response.OutputStream.Close();
			context.Response.Close();

		}


		private void HandlePhoto(HttpListenerContext context)
		{
			// get the information about what wants to be sent
			if(Application.Preferences.PhotoType.CompareTo(Preferences.Local) != 0)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				context.Response.StatusDescription = Application.Preferences.PhotoLocation;
				context.Response.Close();			
				return;
			}

			try
			{
				FileStream fs = 
					File.Open(Application.Preferences.PhotoLocation, FileMode.Open, FileAccess.Read);
				Stream stream = context.Response.OutputStream;
				context.Response.ContentLength64 = fs.Length;
				context.Response.StatusCode = (int)HttpStatusCode.OK;
				context.Response.StatusDescription = Protocol.ResponsePhotoSent;

				int sizeRead = 0;
				int totalRead = 0;
				byte[] buffer = new byte[8192];

				do {
					sizeRead = fs.Read(buffer, 0, 8192);
					totalRead += sizeRead;
					if(sizeRead > 0) {
						stream.Write(buffer, 0, sizeRead);
					}
				} while(sizeRead == 8192);

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


