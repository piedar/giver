// /home/calvin/code/giver/src/GiverService.cs created with MonoDevelop
// User: calvin at 2:12 PM 6/25/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Avahi;

namespace giver
{
	public class GiverService
	{
		
		#region Private Types
		private Avahi.Client client;
        private EntryGroup eg;
		private bool running;
        private object eglock;
        private ServerInfo serverInfo;
		#endregion

		
		public GiverService()
		{
	        eglock = new Object();
			serverInfo = new ServerInfo ();
            client = new Avahi.Client ();
            client.StateChanged += OnClientStateChanged;
		}

        public void Stop ()
		{
            running = false;

            if (client != null) {
                client.Dispose ();
                client = null;
            }
        }

        private void OnClientStateChanged (object o, ClientStateArgs args)
		{
            if (publish && args.State == ClientState.Running) {
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
                    string auth = serverInfo.AuthenticationMethod == AuthenticationMethod.None ? "false" : "true";
                    eg.AddService (serverInfo.Name, "_daap._tcp", "", ws.BoundPort,
                                   new string[] { "Password=" + auth, "Machine Name=" + serverInfo.Name,
                                                  "txtvers=1" });
                    eg.Commit ();
                } catch (ClientException e) {
                    if (e.ErrorCode == ErrorCode.Collision && Collision != null) {
                        Collision (this, new EventArgs ());
                    } else {
                        throw e;
                    }
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
            if (args.State == EntryGroupState.Collision && Collision != null) {
                Collision (this, new EventArgs ());
            }
        }



	}
}
