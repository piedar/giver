//***********************************************************************
// *  $RCSfile$ - TargetService.cs
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
using System.Collections;
using System.IO;
using Gtk;
using Mono.Unix;

namespace Giver
{
	///<summary>
	///	TargetWindow
	/// Window holding all drop targets for giver
	///</summary>
	public enum DragTargetType
	{
		UriList,
		RootWindow
	};

	/// <summary>
	/// Widget used to show Avahi Services for Giver
	/// </summary>
	public class TargetService : Gtk.Button
	{
		private ServiceInfo serviceInfo;
		private bool isManual;
		
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

	        HBox hbox = new HBox(false, 10);
			Gtk.Image image;
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
	        Add(hbox);

			TargetEntry[] targets = new TargetEntry[] {
	                		new TargetEntry ("text/uri-list", 0, (uint) DragTargetType.UriList) };

			this.DragDataReceived += DragDataReceivedHandler;

			Gtk.Drag.DestSet(this,
						 DestDefaults.All | DestDefaults.Highlight,
						 targets,
						 Gdk.DragAction.Copy );
		}


		private void DragDataReceivedHandler (object o, DragDataReceivedArgs args)
		{
			//args.Context.
			switch(args.Info) {
				case (uint) DragTargetType.UriList:
				{
                    UriList uriList = new UriList(args.SelectionData);
					if(!isManual) {
						Application.EnqueueFileSend(serviceInfo, uriList.ToLocalPaths());
					} else {
						// Prompt for the info to send here
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
							Catalog.GetString("File to Give"),
							null,
							FileChooserAction.Open
							);

			chooser.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			chooser.AddButton(Stock.Cancel, ResponseType.Cancel);
			chooser.AddButton(Catalog.GetString("Give"), ResponseType.Ok);
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
							Catalog.GetString("Folder to Give"),
							null,
							FileChooserAction.SelectFolder
							);

			chooser.SetCurrentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			chooser.AddButton(Stock.Cancel, ResponseType.Cancel);
			chooser.AddButton(Catalog.GetString("Give"), ResponseType.Ok);
			chooser.DefaultResponse = ResponseType.Ok;
			chooser.LocalOnly = true;

			if(chooser.Run() == (int)ResponseType.Ok) {
				if(!isManual) {
	//				Giver.Application.EnqueueFileSend(serviceInfo, chooser.Filename);
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
		#endregion

		#region Public Properties
		#endregion
	}
}