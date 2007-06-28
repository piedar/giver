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
using Gtk;
using Mono.Unix;

namespace Giver
{
	/// <summary>
	/// Widget used to show Avahi Services for Giver
	/// </summary>
	public class TargetService : Gtk.Button
	{
		private ServiceInfo serviceInfo;
		
		public TargetService(ServiceInfo serviceInfo)
		{
			this.serviceInfo = serviceInfo;

			this.BorderWidth = 0;
			this.Relief = Gtk.ReliefStyle.None;

			Init();
		}

		private void Init()
		{
	        HBox hbox = new HBox(false, 10);
			Gtk.Image image;			
			if(serviceInfo.Photo != null)
				image = new Gtk.Image(serviceInfo.Photo);
			else
			 	image = new Gtk.Image(Utilities.GetIcon("giver-48", 48));
			hbox.PackStart(image, false, false, 0);
			VBox vbox = new VBox();
			hbox.PackStart(vbox, false, false, 0);
			Label label = new Label();
			label.Justify = Gtk.Justification.Left;
            label.SetAlignment (0.0f, 0.5f);
			label.LineWrap = false;
			label.UseMarkup = true;
			label.UseUnderline = false;
			label.Markup = string.Format ("<span weight=\"bold\" size=\"large\">{0}</span>",
                    						serviceInfo.UserName);
			vbox.PackStart(label, true, true, 0);

			label = new Label();
			label.Justify = Gtk.Justification.Left;
            label.SetAlignment (0.0f, 0.5f);
			label.LineWrap = false;
			label.UseMarkup = true;
			label.UseUnderline = false;
			label.Markup = string.Format ("<span size=\"small\">{0}</span>",
                    						serviceInfo.MachineName);

			vbox.PackStart(label, true, true, 0);

			label = new Label();
			label.Justify = Gtk.Justification.Left;
            label.SetAlignment (0.0f, 0.5f);
			label.LineWrap = false;
			label.UseMarkup = true;
			label.UseUnderline = false;
			label.Markup = string.Format ("<span style=\"italic\" size=\"small\">{0}:{1}</span>",
                    						serviceInfo.Address, serviceInfo.Port);

			vbox.PackStart(label, true, true, 0);
	        hbox.ShowAll();
	        Add(hbox);
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
				Giver.Application.EnqueueFileSend(serviceInfo, chooser.Filename);
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
				Logger.Debug("Sending folder {0}", chooser.Filename);
//				Giver.Application.EnqueueFileSend(serviceInfo, chooser.Filename);
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