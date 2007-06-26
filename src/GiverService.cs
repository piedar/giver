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

	public delegate void ClientConnectedHandler (TcpClient client);

	public class GiverService
	{		
		#region Private Types
		private Avahi.Client client;
        private EntryGroup eg;
        private object eglock;
		private TcpListener server;
		private Thread listnerThread;
		private bool running;
		#endregion

		public event ClientConnectedHandler ClientConnected;

		public GiverService()
		{
			Logger.Debug("New GiverService was created");
	        eglock = new Object();
			running = true;
			server = new TcpListener(IPAddress.Any, 0);
			server.Start();
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
			server.Stop();

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
					eg.AddService(	"giver on " + Environment.UserName + "@" + Environment.MachineName, 
									"_giver._tcp", "", (ushort)((IPEndPoint)server.LocalEndpoint).Port, 
									new string[] { "User Name=" + Environment.UserName, 
													"Machine Name=" + Environment.MachineName, 
													"Version=" + Defines.Version });

//					Gdk.Pixbuf pixbuf = new Gdk.Pixbuf("/home/calvin/calvin.png");
//					pixbuf = pixbuf.ScaleSimple(8,8, Gdk.InterpType.Bilinear);
//					byte[] photo = pixbuf.SaveToBuffer("png");
//					eg.AddRecord("Photo", RecordClass.In, RecordType.Txt, 20, photo, photo.Length);
//					eg.AddRecord("Photo", RecordClass.In, RecordType.Txt, 20, photo, photo.Length);

                    eg.Commit ();
					Logger.Debug("Avahi Service  _giver._tcp is added");
                } catch (Exception e) {
					Logger.Debug("Exception adding service: {0}", e.Message);
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
		        	TcpClient client = server.AcceptTcpClient();            
		        	Logger.Debug("GiverService: Connected!");

					// Fire off an event here and hand off the connected TcpClient to be handled
					if(ClientConnected != null) {
						ClientConnected(client);
					} else {
						client.Close();
					}
				}
			    catch(SocketException e)
			    {
					// this will happen when we close down the service
					Logger.Debug("GiverService: SocketException: {0}", e.Message);
			    }
			}
		}

	}
}
