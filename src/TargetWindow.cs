/***************************************************************************
 *  TargetWindow.cs
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
using Gtk;

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
		private static int lastXPos;
		private static int lastYPos;


		// Widgets
		private ScrolledWindow scrolledWindow;
		private VBox targetVBox;
		private TargetService manualTarget;
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
			SetupApplicationEvents ();
		}
		#endregion


		#region Private Methods
		private void SetupApplicationEvents ()
		{
			Application.Instance.AvatarUpdated += OnAvatarUpdated;
		}

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
			Title = string.Format ("Giver Recipients");	
			
			this.DefaultSize = new Gdk.Size (300, 500); 			

			// Start with an event box to paint the background white
			EventBox eb = new EventBox();
			eb.BorderWidth = 0;
            eb.ModifyBg(StateType.Normal, new Gdk.Color(255,255,255));
            eb.ModifyBase(StateType.Normal, new Gdk.Color(255,255,255));

			VBox mainVBox = new VBox();
			mainVBox.BorderWidth = 0;
			mainVBox.Show ();
			eb.Add(mainVBox);
			this.Add (eb);

			scrolledWindow = new ScrolledWindow ();
			scrolledWindow.VscrollbarPolicy = PolicyType.Automatic;
			scrolledWindow.HscrollbarPolicy = PolicyType.Never;
			//scrolledWindow.ShadowType = ShadowType.None;
			scrolledWindow.BorderWidth = 0;
			scrolledWindow.CanFocus = true;
			scrolledWindow.Show ();
			mainVBox.PackStart (scrolledWindow, true, true, 0);

			// Add a second Event box in the scrolled window so it will also be white
			EventBox innerEb = new EventBox();
			innerEb.BorderWidth = 0;
            innerEb.ModifyBg(StateType.Normal, new Gdk.Color(255,255,255));
            innerEb.ModifyBase(StateType.Normal, new Gdk.Color(255,255,255));

			targetVBox = new VBox();
			targetVBox.BorderWidth = 0;
			targetVBox.Show ();
			innerEb.Add(targetVBox);

			scrolledWindow.AddWithViewport(innerEb);

			//mainVBox.PackStart (targetVBox, false, false, 0);
			manualTarget = new TargetService();
			manualTarget.Show ();
			mainVBox.PackStart(manualTarget, false, false, 0);

			Shown += OnWindowShown;
			DeleteEvent += WindowDeleted;

			Application.Instance.TransferStarted += TransferStartedHandler;
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


    	private void TransferStartedHandler (TransferStatusArgs args) 
		{
			if(targets.ContainsKey(args.TargetServiceInfo.ID)) {
				TargetService ts = targets[args.TargetServiceInfo.ID];
				ts.SetupTransferEventHandlers();
			}		
		}

		
		public void OnAvatarUpdated (ServiceInfo serviceInfo)
		{
			Logger.Debug ("TargetWindow::OnAvatarUpdated - called");
			try {
				TargetService target = targets[serviceInfo.ID];
				target.UpdateImage (serviceInfo.Photo);
			} catch (Exception opu) {
				Logger.Debug (opu.Message);
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
            int x;
            int y;

            this.GetPosition(out x, out y);
            
            lastXPos = x;
            lastYPos = y;

			Application.Instance.TransferStarted -= TransferStartedHandler;
            
			Logger.Debug("WindowDeleted was called");
			TearDownLocatorEvents();
			targetWindow = null;
			Application.Instance.Quit ();
		}


		///<summary>
		///	OnVideoViewRealized
		/// Handles all setup of the window after it's been realized on the screen
		///</summary>			
		private void OnWindowShown (object sender, EventArgs args)
		{

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
			if(targetWindow != null) {
				if(targetWindow.IsActive) {
		            int x;
		            int y;

		            targetWindow.GetPosition(out x, out y);
		            
		            lastXPos = x;
		            lastYPos = y;

					targetWindow.Hide();
				} else {
					if(!targetWindow.Visible) {
			        	int x = lastXPos;
						int y = lastYPos;

						if (x >= 0 && y >= 0)
							targetWindow.Move(x, y);						
					}
					targetWindow.Present();
				}
			} else {
				TargetWindow.targetWindow = new TargetWindow(serviceLocator);
	        	int x = lastXPos;
				int y = lastYPos;

				if (x >= 0 && y >= 0)
					targetWindow.Move(x, y);						

				targetWindow.ShowAll();
			}
		}
	}
}
