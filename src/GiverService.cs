//***********************************************************************
// *  GiverService.cs
// *
// *  Copyright (C) 2007 Novell, Inc.
// *
// *  This program is free software; you can redistribute it and/or
// *  modify it under the terms of the GNU General Public
// *  License as published by the Free Software Foundation; either
// *  version 2 of the License, or (at your option) any later version.
// *
// *  This program is distributed in the hope that it will be useful,
// *  but WITHOUT ANY WARRANTY; without even the implied warranty of
// *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// *  General Public License for more details.
// *
// *  You should have received a copy of the GNU General Public
// *  License along with this program; if not, write to the Free
// *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
// *
// **********************************************************************

using System;
using Avahi;
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
		private Avahi.Client client;
        private EntryGroup eg;
        private object eglock;
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
	        eglock = new Object();
			running = true;

			TcpListener server = new TcpListener(IPAddress.Any, 0);
			server.Start();
			port = ((IPEndPoint)server.LocalEndpoint).Port;
			server.Stop();
			Logger.Debug("We have the port : {0}", port );

			listener = new HttpListener();
			listener.AuthenticationSchemes = AuthenticationSchemes.None;
			listener.Prefixes.Add(String.Format("http://+:{0}/", port));
			listener.Start();

			listnerThread  = new Thread(TcpServerThread);
			listnerThread.Start();
			Logger.Debug("About to create the Avahi client");
            client = new Avahi.Client ();
            client.StateChanged += OnClientStateChanged;
			if(client.State == ClientState.Running)
				RegisterService();
		}


        public void Stop ()
		{
			running = false;
        	UnregisterService ();
			listener.Stop();
			//server.Stop();

            if (client != null) {
                client.Dispose ();
                client = null;
            }
        }


        private void OnClientStateChanged (object o, ClientStateArgs args)
		{  
			Logger.Debug("OnClientStateChanged called with state = {0}", args.State);

            if (args.State == ClientState.Running) {
                RegisterService ();
            }
        }


        private void RegisterService () 
		{
            lock (eglock) {

                if (eg != null) {
                    eg.Reset ();
                } else {
                    eg = new EntryGroup (client);
                    eg.StateChanged += OnEntryGroupStateChanged;
                }

                try {
					Logger.Debug("Adding Avahi Service  _giver._tcp");
					string[] txtStrings;

					if(	Application.Preferences.HasPhoto && 
						Application.Preferences.PhotoIsUri &&
						(Application.Preferences.PhotoLocation != null) ) {
						txtStrings = new string[] { "User Name=" + Environment.UserName, 
													"Machine Name=" + Environment.MachineName, 
													"Version=" + Defines.Version,
													"Photo=" + Application.Preferences.PhotoLocation };
					} else if( Application.Preferences.HasPhoto && (!Application.Preferences.PhotoIsUri) ) {
						txtStrings = new string[] { "User Name=" + Environment.UserName, 
													"Machine Name=" + Environment.MachineName, 
													"Version=" + Defines.Version,
													"Photo=local" };
					} else {
						txtStrings = new string[] { "User Name=" + Environment.UserName, 
													"Machine Name=" + Environment.MachineName, 
													"Version=" + Defines.Version,
													"Photo=none" };
					}

					eg.AddService(	"giver on " + Environment.UserName + "@" + Environment.MachineName, 
									"_giver._tcp", "", (ushort)port, txtStrings);

                    eg.Commit ();
					Logger.Debug("Avahi Service  _giver._tcp is added");
                } catch (Exception e) {
					Logger.Debug("Exception adding service: {0}", e.Message);
					Logger.Debug("Exception is: {0}", e);
                }
            }
        }


        private void UnregisterService () {
            lock (eglock) {
                if (eg == null)
                    return;

                eg.Reset ();
                eg.Dispose ();
                eg = null;
            }
        }


        private void OnEntryGroupStateChanged (object o, EntryGroupStateArgs args) {
			Logger.Debug("GiverService:OnEntryGroupStateChanged was called state: {0}", args.State.ToString());
        }


		private void TcpServerThread()
		{
			while(running) {
		        Logger.Debug("GiverService: Waiting for a connection... ");
		        
				try
				{
					HttpListenerContext context = listener.GetContext();

		        	//TcpClient client = server.AcceptTcpClient();            
		        	Logger.Debug("GiverService: Connected!");

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
