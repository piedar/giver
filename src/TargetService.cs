/***************************************************************************
 *  TargetService.cs
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
using System.Collections;
using System.IO;
using Gtk;

namespace Giver
{
	///<summary>
	///	TargetWindow
	/// Window holding all drop targets for giver
	///</summary>
	public enum DragTargetType
	{
		UriList,
		TomBoyNote
	};

	/// <summary>
	/// Widget used to show Avahi Services for Giver
	/// </summary>
	public class TargetService : Gtk.Button
	{
		private ServiceInfo serviceInfo;
		private bool isManual;
		private Gtk.Image image;
		private double progressFraction;
		private bool updateProgress;
		
		private ProgressBar progressBar;
		private Label progressLabel;
		
		public TargetService(ServiceInfo serviceInfo)
		{
			this.serviceInfo = serviceInfo;
			isManual = false;
			Init();
		}

		public TargetService()
		{
			isManual = true;
			Init();
		}

		private void Init()
		{
			this.BorderWidth = 0;
			this.Relief = Gtk.ReliefStyle.None;
			this.CanFocus = false;
			progressFraction = 0;
			updateProgress = false;
			
			VBox outerVBox = new VBox (false, 4);
	        HBox hbox = new HBox(false, 10);
			if(isManual) {
				image = new Gtk.Image(Utilities.GetIcon("computer", 48));
			} else {
				if(serviceInfo.Photo != null)
					image = new Gtk.Image(serviceInfo.Photo);
				else
				 	image = new Gtk.Image(Utilities.GetIcon("giver-48", 48));
			}

			hbox.PackStart(image, false, false, 0);
			VBox vbox = new VBox();
			hbox.PackStart(vbox, false, false, 0);
			Label label = new Label();
			label.Justify = Gtk.Justification.Left;
            label.SetAlignment (0.0f, 0.5f);
			label.LineWrap = false;
			label.UseMarkup = true;
			label.UseUnderline = false;
			if(isManual)
				label.Markup = "<span weight=\"bold\" size=\"large\">User Specified</span>";
			else
				label.Markup = string.Format ("<span weight=\"bold\" size=\"large\">{0}</span>",
                    						serviceInfo.UserName);
			vbox.PackStart(label, true, true, 0);

			label = new Label();
			label.Justify = Gtk.Justification.Left;
            label.SetAlignment (0.0f, 0.5f);
			label.UseMarkup = true;
			label.UseUnderline = false;

			if(isManual) {
				label.LineWrap = true;
				label.Markup = "<span style=\"italic\" size=\"small\">Use this recipient to enter\ninformation manually.</span>";
			} else {
				label.LineWrap = false;
				label.Markup = string.Format ("<span size=\"small\">{0}</span>",
	                    						serviceInfo.MachineName);
			}

			vbox.PackStart(label, true, true, 0);

			if(!isManual) {
				label = new Label();
				label.Justify = Gtk.Justification.Left;
	            label.SetAlignment (0.0f, 0.5f);
				label.LineWrap = false;
				label.UseMarkup = true;
				label.UseUnderline = false;
				label.Markup = string.Format ("<span style=\"italic\" size=\"small\">{0}:{1}</span>",
	                    						serviceInfo.Address, serviceInfo.Port);

				vbox.PackStart(label, true, true, 0);
			}

	        hbox.ShowAll();
	        outerVBox.PackStart (hbox, true, true, 0);
	        
	        progressBar = new ProgressBar ();
	        progressBar.Orientation = ProgressBarOrientation.LeftToRight;
	        progressBar.BarStyle = ProgressBarStyle.Continuous;
	        
	        progressBar.NoShowAll = true;
	        outerVBox.PackStart (progressBar, true, false, 0);
	        
			progressLabel = new Label ();
			progressLabel.UseMarkup = true;
			progressLabel.Xalign = 0;
			progressLabel.UseUnderline = false;
			progressLabel.LineWrap = true;
			progressLabel.Wrap = true;
			progressLabel.NoShowAll = true;
			progressLabel.Ellipsize = Pango.EllipsizeMode.End;
			outerVBox.PackStart (progressLabel, false, false, 0);
	        
	        outerVBox.ShowAll ();
	        Add(outerVBox);

			TargetEntry[] targets = new TargetEntry[] {
	                		new TargetEntry ("text/uri-list", 0, (uint) DragTargetType.UriList) };

			this.DragDataReceived += DragDataReceivedHandler;

			Gtk.Drag.DestSet(this,
						 DestDefaults.All | DestDefaults.Highlight,
						 targets,
						 Gdk.DragAction.Copy | Gdk.DragAction.Move );
		}


		private void DragDataReceivedHandler (object o, DragDataReceivedArgs args)
		{
			//args.Context.
			switch(args.Info) {
				case (uint) DragTargetType.UriList:
				{
                    UriList uriList = new UriList(args.SelectionData);
					string[] paths = uriList.ToLocalPaths();

					if(paths.Length > 0)
					{
						if(!isManual) {
							Application.EnqueueFileSend(serviceInfo, uriList.ToLocalPaths());
						} else {
							// Prompt for the info to send here
						}
					} else {
						// check for a tomboy notes
						foreach(Uri uri in uriList) {
							if( (uri.Scheme.CompareTo("note") == 0) &&
								(uri.Host.CompareTo("tomboy") == 0) ) {
								string[] files = new string[1];
								string tomboyID = uri.AbsolutePath.Substring(1);

								string homeFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
								string path = System.IO.Path.Combine(homeFolder, ".tomboy");
								path = System.IO.Path.Combine(path, tomboyID + ".note");
								files[0] = path;
								Logger.Debug("Go and get the tomboy note {0}", path);
								Application.EnqueueFileSend(serviceInfo, files);
							}
							else if(uri.Scheme.CompareTo("tasque") == 0) {
								string[] files = new string[1];
								string taskFile = uri.AbsolutePath.Substring(1);

								string path = System.IO.Path.Combine("/", taskFile);
								files[0] = path;
								Logger.Debug("Go and get the task file {0}", path);
								Application.EnqueueFileSend(serviceInfo, files);
							}
						}
					}
					break;
				}
				default:
					break;
			}

			//Logger.Debug("DragDataReceivedHandler called");
            Gtk.Drag.Finish (args.Context, true, false, args.Time);
		}


		private void OnSendFile (object sender, EventArgs args)
		{
			FileChooserDialog chooser = new FileChooserDialog(
							Services.PlatformService.GetString("File to Give"),
							null,
							FileChooserAction.Open
							);

			chooser.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			chooser.AddButton(Stock.Cancel, ResponseType.Cancel);
			chooser.AddButton(Services.PlatformService.GetString("Give"), ResponseType.Ok);
			chooser.DefaultResponse = ResponseType.Ok;
			chooser.LocalOnly = true;

			if(chooser.Run() == (int)ResponseType.Ok) {
				Logger.Debug("Giving file {0}", chooser.Filename);
				if(!isManual) {
					Application.EnqueueFileSend(serviceInfo, chooser.Filenames);
				} else {
					// Prompt for the info to send here
				}
			}

			chooser.Destroy();
		}


		private void OnSendFolder (object sender, EventArgs args)
		{
			FileChooserDialog chooser = new FileChooserDialog(
							Services.PlatformService.GetString("Folder to Give"),
							null,
							FileChooserAction.SelectFolder
							);

			chooser.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			chooser.AddButton(Stock.Cancel, ResponseType.Cancel);
			chooser.AddButton(Services.PlatformService.GetString("Give"), ResponseType.Ok);
			chooser.DefaultResponse = ResponseType.Ok;
			chooser.LocalOnly = true;

			if(chooser.Run() == (int)ResponseType.Ok) {
				if(!isManual) {
					Giver.Application.EnqueueFileSend(serviceInfo, chooser.Filenames);
				} else {
					// Prompt for the info to send here
				}
			}

			chooser.Destroy();
		}


		#region Overrides
		protected override void OnClicked ()
		{
			Menu popupMenu = new Menu ();
			ImageMenuItem item;
			
      		item = new ImageMenuItem ("Give a File...");
			item.Image = new Image(Gtk.Stock.File, IconSize.Button);
			item.Activated += OnSendFile;
			popupMenu.Add (item);
			
			item = new ImageMenuItem("Give a Folder...");
			item.Image = new Image(Gtk.Stock.Directory, IconSize.Button);
			item.Activated += OnSendFolder;
			popupMenu.Add (item);
			
			popupMenu.ShowAll();
			popupMenu.Popup ();
		}
		#endregion

		#region Event Handlers
		private void FileTransferStartedHandler (TransferStatusArgs args)
		{
			Gtk.Application.Invoke ( delegate {
				ProgressText = Services.PlatformService.GetString ("Giving: {0}",
					args.Name);
				progressBar.Text = Services.PlatformService.GetString ("{0} of {1}",
					args.CurrentCount,
					args.TotalCount);
			});

			if(!updateProgress) {
				updateProgress = true;
				GLib.Timeout.Add(50, UpdateProgressBar);
			}
		}
		
		private void TransferProgressHandler (TransferStatusArgs args)
		{
			progressFraction = ((double)args.TotalBytesTransferred) / ((double)args.TotalBytes);
		}

		private bool UpdateProgressBar()
		{
			if(updateProgress) {
				Gtk.Application.Invoke (delegate {
					progressBar.Fraction = progressFraction;
				});
			}
			return updateProgress;
		}


		private void TransferEndedHandler (TransferStatusArgs args)
		{
			updateProgress = false;
			Giver.Application.Instance.FileTransferStarted -= FileTransferStartedHandler;
			Giver.Application.Instance.TransferProgress -= TransferProgressHandler;
			Giver.Application.Instance.TransferEnded -= TransferEndedHandler;

			Gtk.Application.Invoke (delegate {
				progressBar.Hide ();
				ProgressText = string.Empty;
				progressLabel.Hide ();
			});
		}

		#endregion

		#region Public Properties
		public string ProgressText
		{
			set {
				progressLabel.Markup =
					string.Format ("<span size=\"small\" style=\"italic\">{0}</span>",
						value);
			}
		}
		#endregion
		
		#region Public Methods		
		public void UpdateImage (Gdk.Pixbuf newImage)
		{
			Logger.Debug ("TargetService::UpdateImage called");
			this.image.FromPixbuf = newImage;
		}

		public void SetupTransferEventHandlers ()
		{
			Gtk.Application.Invoke ( delegate {
				progressBar.Show ();
				progressLabel.Show ();
			});
			Giver.Application.Instance.FileTransferStarted += FileTransferStartedHandler;
			Giver.Application.Instance.TransferProgress += TransferProgressHandler;
			Giver.Application.Instance.TransferEnded += TransferEndedHandler;
		}

		#endregion
		
	}
}
