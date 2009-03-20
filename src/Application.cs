/***************************************************************************
 *  Application.cs
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
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Net.Sockets;

using Gtk;
using Gdk;

namespace Giver
{

    public delegate void AvatarUpdatedHandler (ServiceInfo serviceInfo);

	class Application
	{
		#region Private Static Types
		private static Giver.Application application = null;
		private static System.Object locker = new System.Object();
		#endregion
		
		#region Private Types
		private IDesktopApplication desktop_app;
		private Gdk.Pixbuf onPixBuf;
		private Gdk.Pixbuf offPixBuf;
		private Gtk.StatusIcon tray_icon;
		private GiverService service;
		private ServiceLocator locator;
		private RequestHandler requestHandler;
		private SendingHandler sendingHandler;
		private Giver.PhotoService photoService;
		private Preferences preferences;
		private bool quiet;
		#endregion
	
    	public TransferStartedHandler TransferStarted;
    	public FileTransferStartedHandler FileTransferStarted;
    	public TransferProgressHandler TransferProgress;
    	public TransferEndedHandler TransferEnded;
    	public AvatarUpdatedHandler AvatarUpdated;

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

		public static Preferences Preferences
		{
			get { return Application.Instance.preferences; }
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
			desktop_app = Services.PlatformService.CreateDesktopApplication (
						"Giver",
						Defines.Version,
						args);

			preferences = new Preferences();
			foreach (string arg in args) {
				if (arg == "--quiet") {
					quiet = true;
					break;
				}
			}

			GLib.Idle.Add(InitializeIdle);
		}
	
		private bool InitializeIdle()
		{
			requestHandler = new RequestHandler();
			sendingHandler = new SendingHandler();
			sendingHandler.Start();

			sendingHandler.TransferStarted += TransferStartedHandler;
			sendingHandler.FileTransferStarted += FileTransferStartedHandler;
			sendingHandler.TransferProgress += TransferProgressHandler;
			sendingHandler.TransferEnded += TransferEndedHandler;

			try {
				photoService = new Giver.PhotoService ();
				photoService.PhotoResolved += OnPhotoResolved;
				photoService.Start ();
			} catch (Exception e) {
				Logger.Fatal ("Failed to start the Photo Service");
				throw e;
			}
			

			try {
				locator = new ServiceLocator();
			} catch (Exception e) {
				if(e.Message.CompareTo("Daemon not running") == 0) {
					Logger.Fatal("The Avahi Daemon is not running... start it before running Giver");
				}
				else
					Logger.Debug("Error starting ServiceLocator: {0}", e.Message);

				throw e;
			}
			try {
				service = new GiverService();
			} catch (Exception e) {
				if(e.Message.CompareTo("Daemon not running") == 0) {
					Logger.Fatal("The Avahi Daemon is not running... start it before running Giver");
				}
				else
					Logger.Debug("Error starting GiverService: {0}", e.Message);

				throw e;
			}

			locator.ServiceRemoved += OnServicesChanged;
			locator.ServiceAdded += OnServicesChanged;
			service.ClientConnected += OnClientConnected;

			//tray = new NotificationArea("RtcApplication");
			SetupTrayIcon();

			if (!quiet)
				TargetWindow.ShowWindow(locator);

			return false;
		}
  
		private void UpdateTrayIcon()
		{
			if(locator.Count > 0)
				tray_icon.Pixbuf = onPixBuf;
			else
				tray_icon.Pixbuf = offPixBuf;
		}
 
	 	private void OnServicesChanged (object o, ServiceArgs args)
		{
			Gtk.Application.Invoke( delegate {
				UpdateTrayIcon();
			} );
		}


		private void OnClientConnected (HttpListenerContext context)
		{
			requestHandler.HandleRequest(context);
		}
		
		private void OnPhotoResolved (ServiceInfo serviceInfo)
		{
			Logger.Debug ("OnPhotoResolved called");
			Gtk.Application.Invoke( delegate {
				UpdatePhoto (serviceInfo);
			} );
		}
		
		private void UpdatePhoto (ServiceInfo serviceInfo)
		{
			if (AvatarUpdated != null)
				AvatarUpdated (serviceInfo);
			else
				Logger.Debug ("No registered providers for AvatarUpdated");
		}
		
		private void SetupTrayIcon ()
		{
//			Logger.Debug ("Creating TrayIcon");
			
			onPixBuf = Utilities.GetIcon ("giver-24", 24);
			offPixBuf = Utilities.GetIcon ("giveroff-24", 24);

			tray_icon = new Gtk.StatusIcon ();
			tray_icon.Pixbuf = offPixBuf;
			tray_icon.Activate += delegate { TargetWindow.ShowWindow (locator); };
			tray_icon.PopupMenu += OnPopupMenu;
		}

		private void TransferStartedHandler (TransferStatusArgs args)
		{
			if(TransferStarted != null)
				TransferStarted(args);
		}

		private void FileTransferStartedHandler (TransferStatusArgs args)
		{
			if(FileTransferStarted != null)
				FileTransferStarted(args);
		}

		private void TransferProgressHandler (TransferStatusArgs args)
		{

			if(TransferProgress != null)
				TransferProgress(args);
		}

		private void TransferEndedHandler (TransferStatusArgs args)
		{
			if(TransferEnded != null)
				TransferEnded(args);
		}

		private void OnPreferences (object sender, EventArgs args)
		{
			Logger.Info ("OnPreferences called");
			Giver.PreferencesDialog dialog = new PreferencesDialog();
			dialog.Run();
			dialog.Hide();
			dialog.Destroy();
	
		}

		private void OnAbout (object sender, EventArgs args)
		{
            string [] authors = new string [] {
                "Calvin Gaisford <calvinrg@gmail.com>",
                "Scott Reeves <sreeves@gmail.com>"
            };

           /* string [] documenters = new string [] {
                "Calvin Gaisford <calvinrg@gmail.com>"
            };

			string translators = Services.PlatformService.GetString ("translator-credits");
            if (translators == "translator-credits")
                translators = null;
			*/

            Gtk.AboutDialog about = new Gtk.AboutDialog ();
            about.Name = "Giver";
            about.Version = Defines.Version;
            about.Logo = Utilities.GetIcon("giver-48", 48);
            about.Copyright =
                Services.PlatformService.GetString ("Copyright \xa9 2007 Novell, Inc.");
            about.Comments = Services.PlatformService.GetString ("Easy File Sharing");
            about.Website = "http://idea.opensuse.org/content/ideas/easy-file-sharing";
            about.WebsiteLabel = Services.PlatformService.GetString("Homepage");
            about.Authors = authors;
            //about.Documenters = documenters;
            //about.TranslatorCredits = translators;
            about.IconName = "giver";
            about.Run ();
            about.Destroy ();
		}


		private void OnShowTargets (object sender, EventArgs args)
		{
			TargetWindow.ShowWindow(locator);
		}
		
		private void OnQuit (object sender, EventArgs args)
		{
			Logger.Info ("OnQuitAction called - terminating application");
			Quit ();
		}
		
		private void OnPopupMenu (object o, PopupMenuArgs args)
		{
      			Menu popupMenu = new Menu();
      			
      			ImageMenuItem targets = new ImageMenuItem (
						Services.PlatformService.GetString ("Giver Recipients ..."));
				targets.Image = new Gtk.Image(Utilities.GetIcon ("giver-24", 24));
      			targets.Activated += OnShowTargets;
      			popupMenu.Add (targets);
      			
      			SeparatorMenuItem separator = new SeparatorMenuItem ();
      			popupMenu.Add (separator);
      			
      			ImageMenuItem preferences = new ImageMenuItem (Gtk.Stock.Preferences, null);
      			preferences.Activated += OnPreferences;
      			popupMenu.Add (preferences);

      			ImageMenuItem about = new ImageMenuItem (Gtk.Stock.About, null);
      			about.Activated += OnAbout;
      			popupMenu.Add (about);

      			separator = new SeparatorMenuItem ();
      			popupMenu.Add (separator);

      			ImageMenuItem quit = new ImageMenuItem ( Gtk.Stock.Quit, null);
      			quit.Activated += OnQuit;
      			popupMenu.Add (quit);
      			
			popupMenu.ShowAll(); // shows everything
			popupMenu.Popup(null, null, null, 3, Gtk.Global.CurrentEventTime);
		}		

		#endregion		


		#region Public Static Methods	
		public static void Main(string[] args)
		{
			try 
			{
				Services.PlatformService.SetProcessName ("Giver");
				application = GetApplicationWithArgs(args);
				application.StartMainLoop ();
			} 
			catch (Exception e)
			{
				Giver.Logger.Debug("Exception is: {0}", e);
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

		public static void EnqueueFileSend(ServiceInfo serviceInfo, string[] files)
		{
			Giver.Application.Instance.sendingHandler.QueueFileSend(serviceInfo, files);
		}

		#endregion
		
		#region Public Methods			
		public void StartMainLoop ()
		{
			desktop_app.StartMainLoop ();
		}

		public void Quit ()
		{
			sendingHandler.Stop();
			service.Stop();
			locator.Stop();
			photoService.Stop ();
			desktop_app.Quit ();
//			actionManager ["QuitAction"].Activate ();
		}
		#endregion
		
	}

}
