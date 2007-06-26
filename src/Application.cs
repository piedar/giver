//***********************************************************************
// *  Application.cs
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
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

using Gtk;
using Gdk;
using Gnome;
using Mono.Unix;
using Mono.Unix.Native;
using Notifications;

namespace Giver
{
	class Application
	{
		#region Private Static Types
		private static Giver.Application application = null;
		private static System.Object locker = new System.Object();
		#endregion
		
		#region Private Types
        private Gnome.Program program;
		private Gdk.Pixbuf onPixBuf;
		private Gdk.Pixbuf offPixBuf;
		private Gtk.Image trayImage;
		private Egg.TrayIcon trayIcon;	
		private GiverService service;
		private ServiceLocator locator;
		#endregion
	
		#region Public Static Properties
		public static Application Instance
		{
			get
			{
				lock(locker)
				{
					if(application == null)
					{
						lock(locker)
						{
							application = new Application();
						}
					}
					return application;
				}
			}
		}
		#endregion

		#region Constructors	
		private Application ()
		{
			Init(null);
		}
	
		private Application (string[] args)
		{
			Init(args);
		}
		#endregion


		#region Private Methods
		private void Init(string[] args)
		{
//			Logger.Debug ("Giver::Application::Init - called");
			Gtk.Application.Init ();
			program = 
				new Gnome.Program (
						"giver",
						Defines.Version,
						Gnome.Modules.UI,
						args);

			locator = new ServiceLocator();
			service = new GiverService();
			locator.Removed += OnServicesChanged;
			locator.Found += OnServicesChanged;

			//tray = new NotificationArea("RtcApplication");
			SetupTrayIcon();
		}
	
    	private void OnServicesChanged (object o, ServiceArgs args)
		{
			if(locator.Count > 0)
				trayImage.Pixbuf = onPixBuf;
			else
				trayImage.Pixbuf = offPixBuf;

		}



		private void SetupTrayIcon ()
		{
//			Logger.Debug ("Creating TrayIcon");
			
			EventBox eb = new EventBox();
			onPixBuf = Utilities.GetIcon ("giver-24", 24);
			offPixBuf = Utilities.GetIcon ("giveroff-24", 24);
			if(locator.Count > 0)
				trayImage = new Gtk.Image(onPixBuf);
			else
				trayImage = new Gtk.Image(offPixBuf);
			eb.Add(trayImage); 
			//new Image(Gtk.Stock.DialogWarning, IconSize.Menu)); // using stock icon

			// hooking event
			eb.ButtonPressEvent += new ButtonPressEventHandler (this.OnTrayIconClick);
			trayIcon = new Egg.TrayIcon("Giver");
			trayIcon.Add(eb);
			// showing the trayicon
			trayIcon.ShowAll();			
		}



		private void OnQuit (object sender, EventArgs args)
		{
			Logger.Info ("OnQuitAction called - terminating application");

			locator.Stop();
			service.Stop();

			Gtk.Main.Quit ();
			//program.Quit (); // Should this be called instead?
		}
		
		
		private void OnTrayIconClick (object o, ButtonPressEventArgs args) // handler for mouse click
		{
			if (args.Event.Button == 1) {
				ShowGiverTargets(args);
			} else if (args.Event.Button == 3) {
   				// FIXME: Eventually get all these into UIManagerLayout.xml file
      			Menu popupMenu = new Menu();
      			
      			ImageMenuItem status = new ImageMenuItem (
						Catalog.GetString ("Show online status ..."));
      			//status.Activated += OnPeople;
      			popupMenu.Add (status);
      			
      			SeparatorMenuItem separator = new SeparatorMenuItem ();
      			popupMenu.Add (separator);
      			
      			ImageMenuItem preferences = new ImageMenuItem (Gtk.Stock.Preferences, null);
      			//preferences.Activated += OnPreferences;
      			popupMenu.Add (preferences);

      			separator = new SeparatorMenuItem ();
      			popupMenu.Add (separator);

      			ImageMenuItem quit = new ImageMenuItem (
      					Catalog.GetString ("Quit"));
      			quit.Activated += OnQuit;
      			popupMenu.Add (quit);
      			
				popupMenu.ShowAll(); // shows everything
      			//popupMenu.Popup(null, null, null, IntPtr.Zero, args.Event.Button, args.Event.Time);
      			popupMenu.Popup(null, null, null, args.Event.Button, args.Event.Time);
   			}
		}		

		private void ShowGiverTargets(ButtonPressEventArgs args)
		{
			bool foundItems = false;
 			Menu popupMenu = new Menu();
 			
//			Logger.Debug("looping through found services");
			foreach(Giver.Service s in locator.Services) {
//				Logger.Debug("A Service was found!");
				foundItems = true;

				GiverMenuItem item = new GiverMenuItem(s);

//	 			ImageMenuItem item = new ImageMenuItem (s.UserName + "@" + s.MachineName + 
//									" (" + s.Address + ":" + s.Port.ToString() + ")");
	 			//quit.Activated += OnQuit;
	 			popupMenu.Add (item);
			}

			
			if(!foundItems) {
				ImageMenuItem item = new ImageMenuItem("No giver targets found!");
				popupMenu.Add(item);
			}
 			
			popupMenu.ShowAll(); // shows everything
 			//popupMenu.Popup(null, null, null, IntPtr.Zero, args.Event.Button, args.Event.Time);
 			popupMenu.Popup(null, null, null, args.Event.Button, args.Event.Time);
		}
		
		
		#endregion		


		#region Public Static Methods	
		public static void Main(string[] args)
		{
			try 
			{
				Utilities.SetProcessName ("Giver");
				application = GetApplicationWithArgs(args);
/*				Banter.Application.RegisterSessionManagerRestart (
					Environment.GetEnvironmentVariable ("RTC_PATH"),
					args);
*/
				application.StartMainLoop ();
			} 
			catch (Exception e)
			{
				//TODO log
				Console.WriteLine (e.Message);
				Exit (-1);
			}
		}

		public static Application GetApplicationWithArgs(string[] args)
		{
			lock(locker)
			{
				if(application == null)
				{
					lock(locker)
					{
						application = new Application(args);
					}
				}
				return application;
			}
		}

		public static void OnExitSignal (int signal)
		{
			if (ExitingEvent != null) ExitingEvent (null, EventArgs.Empty);
			if (signal >= 0) System.Environment.Exit (0);
		}
		
		public static event EventHandler ExitingEvent = null;
		
		public static void Exit (int exitcode)
		{
			OnExitSignal (-1);
			System.Environment.Exit (exitcode);
		}
			
		#endregion
		
		#region Public Methods			
		public void StartMainLoop ()
		{
			program.Run ();
		}

		public void QuitMainLoop ()
		{
//			actionManager ["QuitAction"].Activate ();
		}
		#endregion
		
	}
}
