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

			int desired_port_num = Application.Preferences.PortNumber;
			if (desired_port_num < 0)
				//any port
				desired_port_num = 0;
			if (desired_port_num < IPEndPoint.MinPort || desired_port_num > IPEndPoint.MaxPort) {
				Logger.Debug ("Error: Port number must be between 0 and 65535. Trying to bind to any available port.");
				desired_port_num = 0;
			}
			port = desired_port_num;

			if (desired_port_num == 0 && !TryBindFixedOrAnyPort (desired_port_num, out port))
				ThrowUnableToBindAnyPort ();

			try {
				Logger.Debug ("Starting listener on port {0}", port);
				listener = CreateListener (port);
				listener.Start ();
			} catch (HttpListenerException hle) {
				Logger.Debug ("Error starting a http listener on port {0} : {1}", port, hle.Message);
				listener = TryBindAndListenAgain (desired_port_num, out port);
			} catch (SocketException se) {
				Logger.Debug ("Error starting a http listener on port {0} : {1}", port, se.Message);
				listener = TryBindAndListenAgain (desired_port_num, out port);
			}

			if (port != Application.Preferences.PortNumber)
				Logger.Debug ("We have the port : {0}", port);

			listnerThread  = new Thread(TcpServerThread);
			listnerThread.Start();
			Logger.Debug("About to start the Zeroconf Service");
            client = new RegisterService();
			AdvertiseService();
		}

		private HttpListener CreateListener (int actual_port)
		{
			HttpListener listener = new HttpListener ();
			listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
			listener.Prefixes.Add (String.Format ("http://+:{0}/", actual_port));

			return listener;
		}

		// Tries to bind to @desired_port, if that fails
		// then it tries to bind to _any_ port
		// @actual_port contains a valid available port number
		private bool TryBindFixedOrAnyPort (int desired_port, out int actual_port)
		{
			actual_port = -1;
			TcpListener server = new TcpListener (IPAddress.Any, desired_port);

			try {
				server.Start ();
			} catch (SocketException se) {
				if (port > 0) {
					// user requested port
					Logger.Debug ("Unable to bind to requested port number {0}. Error: {1}. " +
							"Trying to bind to any other available port", desired_port, se.Message);
					try {
						server = new TcpListener (IPAddress.Any, 0);
						server.Start ();
					} catch (SocketException se2) {
						Logger.Debug ("Unable to bind to any port, error: {0}", se2.Message);
						return false;
					}
				}
			}

			actual_port = ((IPEndPoint) server.LocalEndpoint).Port;
			server.Stop ();
			return true;
		}

		// Tries to start a http listener at @desired_port, if that fails
		// then it tries to listen on _any_ port
		// @actual_port will have the listener's port number
		private HttpListener TryBindAndListenAgain (int desired_port_num, out int actual_port)
		{
			actual_port = 0;
			if (desired_port_num <= 0)
				ThrowUnableToBindAnyPort ();

			Logger.Debug ("Trying to bind to any available port");
			if (!TryBindFixedOrAnyPort (0, out actual_port))
				ThrowUnableToBindAnyPort ();

			HttpListener listener = CreateListener (actual_port);
			listener.Start ();

			return listener;
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
					Logger.Debug("GiverService: {0}", e.ToString ());
			    }
			}
		}


		void ThrowUnableToBindAnyPort ()
		{
			throw new Exception ("Unable to bind to any port. See .giver.log file in your home directory for details.");
		}
	}
}
