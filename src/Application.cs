//***********************************************************************
// *  $RCSfile$ - Application.cs
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
        private Gnome.Program program = null;
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
			Logger.Debug ("Giver::Application::Init - called");
			Gtk.Application.Init ();
			program = 
				new Gnome.Program (
						"giver",
						Defines.Version,
						Gnome.Modules.UI,
						args);

			//tray = new NotificationArea("RtcApplication");
			SetupTrayIcon();
		}
	
		private void SetupTrayIcon ()
		{
/*			Logger.Debug ("Creating TrayIcon");
			
			EventBox eb = new EventBox();
			appPixBuf = GetIcon ("banter-22", 22);
			eb.Add(new Image (appPixBuf)); 
			//new Image(Gtk.Stock.DialogWarning, IconSize.Menu)); // using stock icon

			// hooking event
			eb.ButtonPressEvent += new ButtonPressEventHandler (this.OnImageClick);
			trayIcon = new Egg.TrayIcon("RtcApplication");
			trayIcon.Add(eb);
			// showing the trayicon
			trayIcon.ShowAll();			
*/
		}
		

		private void OnQuitAction (object sender, EventArgs args)
		{
			Logger.Info ("OnQuitAction called - terminating application");

			Gtk.Main.Quit ();
			//program.Quit (); // Should this be called instead?
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
