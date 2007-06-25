// /home/calvin/code/giver/src/GiverService.cs created with MonoDevelop
// User: calvin at 2:12 PM 6/25/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

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
							new string[] { "UserName=" + Environment.UserName, 
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
