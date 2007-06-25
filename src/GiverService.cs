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

namespace Giver
{
	public class GiverService
	{
		
		#region Private Types
		private Avahi.Client client;
        private EntryGroup eg;
        private object eglock;
		#endregion

		
		public GiverService()
		{
			Logger.Debug("New GiverService was created");
	        eglock = new Object();
            client = new Avahi.Client ();
            client.StateChanged += OnClientStateChanged;
			if(client.State == ClientState.Running)
				RegisterService();
		}

        public void Stop ()
		{
        	UnregisterService ();

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
					eg.AddService("giver on " + Environment.MachineName, "_giver._tcp", "", 8080, 
							new string[] { "User Name=" + Environment.UserName, 
											"Machine Name=" + Environment.MachineName, 
											"Version=" + Defines.Version });
   
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



	}
}
