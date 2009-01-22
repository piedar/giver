/***************************************************************************
 *  GiverService.cs
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
using Mono.Zeroconf;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Giver
{

//	public delegate void ClientConnectedHandler (TcpClient client);
	public delegate void ClientConnectedHandler (HttpListenerContext context);


	public class GiverService
	{		
		#region Private Types
		private RegisterService client;
		private HttpListener listener;
		private Thread listnerThread;
		private bool running;
		private int port;
		//private string host;
		#endregion

		public event ClientConnectedHandler ClientConnected;

		public GiverService()
		{
			Logger.Debug("New GiverService was created");
			running = true;

			TcpListener server = new TcpListener(IPAddress.Any, 0);
			server.Start();
			port = ((IPEndPoint)server.LocalEndpoint).Port;
			server.Stop();
			Logger.Debug("We have the port : {0}", port );

			listener = new HttpListener();
			listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
			listener.Prefixes.Add(String.Format("http://+:{0}/", port));
			listener.Start();

			listnerThread  = new Thread(TcpServerThread);
			listnerThread.Start();
			Logger.Debug("About to start the Zeroconf Service");
            client = new RegisterService();
			AdvertiseService();
		}


        public void Stop ()
		{
			running = false;
			listener.Stop();
			//server.Stop();

            if (client != null) {
                client.Dispose ();
                client = null;
            }
        }

        private void AdvertiseService () 
		{
            try {
				Logger.Debug("Adding Zeroconf Service _giver._tcp");
				TxtRecord txt = new TxtRecord();
				
				txt.Add("User Name", Application.Preferences.UserName);
				txt.Add("Machine Name", Environment.MachineName);
				txt.Add("Version", Defines.Version);
				
				if( Application.Preferences.PhotoType.CompareTo(Preferences.Local) == 0) {					
					txt.Add("PhotoType", Preferences.Local);
					txt.Add("Photo", "none");
				} else if( Application.Preferences.PhotoType.CompareTo(Preferences.Gravatar) == 0) {
					txt.Add("PhotoType", Preferences.Gravatar);
					txt.Add("Photo", Giver.Utilities.GetMd5Sum(Application.Preferences.PhotoLocation));	
				} else if( Application.Preferences.PhotoType.CompareTo(Preferences.Uri) == 0) {
					txt.Add("PhotoType", Preferences.Uri);
					txt.Add("Photo", Application.Preferences.PhotoLocation);
				} else {
					txt.Add("PhotoType", Preferences.None);
					txt.Add("Photo", "none");
				}
				
				client.Name = "giver on " + Application.Preferences.UserName + "@" + Environment.MachineName; 
				client.RegType = "_giver._tcp";
				client.ReplyDomain = "local.";
				client.Port = (short)port;
				client.TxtRecord = txt;

                client.Register();
				Logger.Debug("Avahi Service  _giver._tcp is added");
            } catch (Exception e) {
				Logger.Debug("Exception adding service: {0}", e.Message);
				Logger.Debug("Exception is: {0}", e);
            }
        }


		private void TcpServerThread()
		{
			while(running) {
		        //Logger.Debug("RECEIVE: GiverService: Waiting for a connection... ");
		        
				try
				{
					HttpListenerContext context = listener.GetContext();

		        	//TcpClient client = server.AcceptTcpClient();            
		        	// Logger.Debug("RECEIVE: GiverService: Connected!");

					// Fire off an event here and hand off the connected TcpClient to be handled
					if(ClientConnected != null) {
						ClientConnected(context);
						//ClientConnected(client);
					} else {
						context.Response.StatusCode = (int)HttpStatusCode.Gone;
						context.Response.Close();
						//client.Close();
					}
				}
				catch(HttpListenerException le) {
					// if the exception is not a listener being close, log it
					if(le.ErrorCode != 0) {
						Logger.Debug("Exception in GiverService {0} : {1}", le.ErrorCode, le.Message);
					}
				}
			    catch(Exception e)
			    {
					// this will happen when we close down the service
					Logger.Debug("GiverService: SocketException: {0}", e.Message);
			    }
			}
		}

	}
}
