//***********************************************************************
// *  Preferences.cs
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
using System.IO;
using Gtk;
using Glade;
using Mono.Unix;

namespace Giver
{
	public class PreferencesDialog
	{
		private string window_name;
		private Glade.XML glade;
		private Window window;

		private Tooltips tips = new Tooltips();
		private FileChooserButton storage_location_chooser;
		private FileChooserButton photo_local_location;

		public PreferencesDialog()
		{
			//Glade.XML glade = new Glade.XML(Path.Combine(Defines.GladeDir, "giver-prefs.glade"), "GiverPrefsDialog", "giver");
			Glade.XML glade = new Glade.XML(Path.Combine(Defines.GladeDir, "giver-prefs.glade"), null, null);
			window_name = "GiverPrefsDialog";        
			this.glade = glade; 
			this.glade.Autoconnect(this);
			BuildWindow();
			LoadPreferences();
			ConnectEvents();
		}

		public virtual void Destroy()
		{
			Window.Destroy();
		}

		protected Glade.XML Glade
		{
			get { return glade; }
		}

		public string Name
		{
			get { return window_name; }
		}

		public Window Window
		{
			get {
			if(window == null) {
				window = (Window)glade.GetWidget(window_name);
			}

			return window;
			}
		}
		
		public ResponseType Run()
		{
		  return (ResponseType)Dialog.Run();
		}

		public Dialog Dialog
		{
			get { return (Dialog)Window; }
		}

		private void BuildWindow()
		{
			storage_location_chooser = new FileChooserButton("Select storage location",
			    FileChooserAction.SelectFolder);
			(Glade["storage_location_container"] as Container).Add(storage_location_chooser);
			(Glade["storage_location_label"] as Label).MnemonicWidget = storage_location_chooser;
			storage_location_chooser.Show();

			photo_local_location = new FileChooserButton("Select photo location",
			    FileChooserAction.Open);
			(Glade["photo_local_location"] as Container).Add(photo_local_location);
			photo_local_location.Show();

			tips.SetTip(Glade["storage_location_label"], "Location to store incoming files", "storage_location");
		}

		private void LoadPreferences()
		{
			//string location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "giver/preferences");
			string photoLocation = Application.Preferences.PhotoLocation;
			string photoType = Application.Preferences.PhotoType;
			string displayName = Application.Preferences.UserName;
			string storage_location = Application.Preferences.ReceiveFileLocation;
			
			(Glade["display_name"] as Entry).Text = displayName;

			if(photoType.Equals(Preferences.None))
			{
				//Logger.Debug("photo type is none");
			   (Glade["none_radiobutton"] as RadioButton).Active = true;
			}
			else if(photoType.Equals(Preferences.Local))
			{
				Logger.Debug("photo type is local");
			   (Glade["local_radiobutton"] as RadioButton).Active = true;
				Image photo = new Image(photoLocation);
			   (Glade["local_radiobutton"] as RadioButton).Image = photo;
			   //(Glade["photo_local_image"] as Image).SetFromIconName(photoLocation, IconSize.Button);
			}
			else if(photoType.Equals(Preferences.Gravatar))
			{
				//Logger.Debug("photo type is gravatar");
			   (Glade["gravatar_radiobutton"] as RadioButton).Active = true;
			   (Glade["gravatar_email"] as Entry).Text = photoLocation;
			}
			else if(photoType.Equals(Preferences.Uri))
			{
			   (Glade["uri_radiobutton"] as RadioButton).Active = true;
				//Logger.Debug("bloody photo location is {0}", photoLocation);
			   (Glade["photo_uri_location"] as Entry).Text = photoLocation;
			}
			           
			storage_location_chooser.SetFilename(storage_location);
		}

		private void ConnectEvents()
		{
			Entry displayName = (Entry) glade.GetWidget("display_name");
			displayName.Changed += delegate {
				Application.Preferences.UserName = displayName.Text;
			};
			
			storage_location_chooser.SelectionChanged += delegate {
				Application.Preferences.ReceiveFileLocation = storage_location_chooser.Filename;
			};

			RadioButton button = (RadioButton) glade.GetWidget("none_radiobutton");
			button.Toggled += delegate {
				Logger.Debug("nonebutton was toggled");
				if(button.Active)
				{
					Application.Preferences.PhotoType = Preferences.None;
				}
			};

			button = (RadioButton) glade.GetWidget("local_radiobutton");
			button.Toggled += delegate {
				if(button.Active)
				{
					Application.Preferences.PhotoType = Preferences.Local;
					Application.Preferences.PhotoLocation = photo_local_location.Filename; 
				}
			};
			//photo.Changed += OnPhotoFileChanged;
		}

		private void OnPhotoFileChanged(object o, EventArgs args)
		{
			(Glade["example_path"] as Label).Markup = String.Format("<small><i>{0}</i></small>",
			GLib.Markup.EscapeText("giver"));
		}
	}
}
