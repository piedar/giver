//***********************************************************************
// *  $RCSfile$ - TargetWindow.cs
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
using Gtk;
using Mono.Unix;

namespace Giver
{
	///<summary>
	///	TargetWindow
	/// Window holding all drop targets for giver
	///</summary>
	public class TargetWindow : Gtk.Window 
	{
		#region Private Types
		private static TargetWindow targetWindow = null;

		// Widgets
		private ScrolledWindow scrolledWindow;
		private VBox targetVBox;
		private Button manualEntryButton;
		private Dictionary<string, TargetService> targets;

		private ServiceLocator serviceLocator;
		#endregion
		

		#region Constructors
		///<summary>
		///	Constructor
		/// Creates a ChatWindow and a conversation according to the type requested
		///</summary>		
		public TargetWindow (ServiceLocator serviceLocator) :
			base (WindowType.Toplevel)
		{
			this.serviceLocator = serviceLocator;
			targets = new Dictionary<string,Giver.TargetService> ();
			InitWindow();
			SetupLocatorEvents();
		}
		#endregion


		#region Private Methods
		///<summary>
		///	SetupLocatorEvents
		/// Connects all serviceLocator event handlers
		///</summary>			
		void SetupLocatorEvents()
		{
			PopulateInitialTargets();
			serviceLocator.ServiceAdded += OnServiceAdded;
			serviceLocator.ServiceRemoved += OnServiceRemoved;
		}


		///<summary>
		///	TearDownLocatorEvents
		/// Tear down all service locator Events
		///</summary>			
		void TearDownLocatorEvents()
		{
			serviceLocator.ServiceAdded -= OnServiceAdded;
			serviceLocator.ServiceRemoved -= OnServiceRemoved;
		}
		
		
		///<summary>
		///	InitWindow
		/// Sets up the widgets and events in the chat window
		///</summary>	
		void InitWindow()
		{
			this.Icon = Utilities.GetIcon ("giver-48", 48);
			// Update the window title
			Title = string.Format ("Giver drop targets");	
			
			this.DefaultSize = new Gdk.Size (300, 500); 			

			VBox mainVBox = new VBox();
			mainVBox.BorderWidth = 0;
			mainVBox.Show ();
			this.Add (mainVBox);

			scrolledWindow = new ScrolledWindow ();
			scrolledWindow.VscrollbarPolicy = PolicyType.Automatic;
			scrolledWindow.HscrollbarPolicy = PolicyType.Automatic;
			scrolledWindow.ShadowType = ShadowType.EtchedIn;
			scrolledWindow.CanFocus = true;
			scrolledWindow.Show ();
			mainVBox.PackStart (scrolledWindow, true, true, 0);

			targetVBox = new VBox();
			targetVBox.BorderWidth = 0;
			targetVBox.Show ();
			scrolledWindow.AddWithViewport(targetVBox);

			manualEntryButton = new Button ();
			manualEntryButton.CanFocus = true;
			manualEntryButton.Label = Catalog.GetString ("Enter Manual _Address");
			manualEntryButton.UseUnderline = true;
			manualEntryButton.Image = new Image (Stock.GoBack, IconSize.Menu);
			manualEntryButton.Show ();
			mainVBox.PackStart (manualEntryButton, false, false, 0);
						
			Shown += OnWindowShown;
			DeleteEvent += WindowDeleted;
		}

		private void PopulateInitialTargets()
		{
			foreach(Giver.ServiceInfo serviceInfo in serviceLocator.Services) {
				TargetService target = new TargetService(serviceInfo);
				targetVBox.PackStart (target, false, false, 0);
				targets.Add(serviceInfo.ID, target);
//	 			item.Activated += OnSelectedService;
			}
		}

    	public void OnServiceAdded (object o, ServiceArgs args)
		{
			Gtk.Application.Invoke( delegate {
				Logger.Debug("Adding the service {0}", args.ServiceInfo.ID);
				TargetService target = new TargetService(args.ServiceInfo);
				target.ShowAll();
				targetVBox.PackStart (target, false, false, 0);
				targets.Add(args.ServiceInfo.ID, target);
			} );
		}

    	public void OnServiceRemoved (object o, ServiceArgs args)
		{
			Logger.Debug("TargetWindow:OnServiceRemoved called for {0}", args.ServiceInfo.Name);
			Gtk.Application.Invoke( delegate {
				Logger.Debug("Remove the service {0}", args.ServiceInfo.Name);
				if(targets.ContainsKey(args.ServiceInfo.ID)) {
					targetVBox.Remove(targets[args.ServiceInfo.ID]);
					targets.Remove(args.ServiceInfo.ID);
				}
			} );
		}

		#endregion


		#region EventHandlers
		///<summary>
		///	WindowDeleted
		/// Cleans up the conversation object with the ConversationManager
		///</summary>	
		private void WindowDeleted (object sender, DeleteEventArgs args)
		{
			Logger.Debug("WindowDeleted was called");
			TearDownLocatorEvents();
			targetWindow = null;
		}


		///<summary>
		///	OnVideoViewRealized
		/// Handles all setup of the window after it's been realized on the screen
		///</summary>			
		private void OnWindowShown (object sender, EventArgs args)
		{
			Logger.Debug("OnWindowShown was called");
		}


		#endregion


		#region Public Methods
		///<summary>
		///	Present
		/// Presents the window
		///</summary>			
/*		public new void Present ()
		{
			if (everShown == false) {
				Show ();
				everShown = true;
			} else {
				base.Present ();
			}
		}
*/
		#endregion


		#region Public Properties
		#endregion

		public static void ShowWindow(ServiceLocator serviceLocator)
		{
			if(TargetWindow.targetWindow != null) {
				targetWindow.Show();
			} else {
				TargetWindow.targetWindow = new TargetWindow(serviceLocator);
				targetWindow.ShowAll();
			}
		}
	}
}
