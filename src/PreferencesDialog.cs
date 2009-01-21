/***************************************************************************
 *  PreferencesDialog.cs
 *
 *  Copyright (C) 2007 Novell, Inc.
 *  Written by Scott Reeves <sreeves@gmail.com>
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
using System.IO;
using Gtk;

namespace Giver
{
	public class PreferencesDialog : Gtk.Dialog
	{
		private FileChooserButton	fileLocationButton;
		private Gtk.Entry 			nameEntry;
		private Gtk.RadioButton		noneButton;
		private Gtk.RadioButton		localButton;
		private Gtk.RadioButton		webButton;
		private Gtk.RadioButton		gravatarButton;
		private Image 				localImage;
		private Button 				photoButton;
		private Entry				webEntry;
		private Entry				gravatarEntry;

		public PreferencesDialog() : base ()
		{
			Init();
			LoadPreferences();
			ConnectEvents();
		}


		private void Init()
		{
			Logger.Debug("Called Init");
			this.Icon = Utilities.GetIcon ("giver-48", 48);
			// Update the window title
			this.Title = string.Format ("Giver Preferences");	
			
			//this.DefaultSize = new Gdk.Size (300, 500); 	
			this.VBox.Spacing = 0;
			this.VBox.BorderWidth = 0;
			this.SetDefaultSize (450, 100);


			this.AddButton(Stock.Close, Gtk.ResponseType.Ok);
            this.DefaultResponse = ResponseType.Ok;


			// Start with an event box to paint the background white
			EventBox eb = new EventBox();
			eb.Show();
			eb.BorderWidth = 0;
            eb.ModifyBg(StateType.Normal, new Gdk.Color(255,255,255));
            eb.ModifyBase(StateType.Normal, new Gdk.Color(255,255,255));

			VBox mainVBox = new VBox();
			mainVBox.BorderWidth = 10;
			mainVBox.Spacing = 5;
			mainVBox.Show ();
			eb.Add(mainVBox);
			this.VBox.PackStart(eb);

			Label label = new Label();
			label.Show();
			label.Justify = Gtk.Justification.Left;
            label.SetAlignment (0.0f, 0.5f);
			label.LineWrap = false;
			label.UseMarkup = true;
			label.UseUnderline = false;
			label.Markup = "<span weight=\"bold\" size=\"large\">Your Name</span>";
			mainVBox.PackStart(label, true, true, 0);

			// Name Box at the top of the Widget
			HBox nameBox = new HBox();
			nameBox.Show();
			nameEntry = new Entry();
			nameEntry.Show();
			nameBox.PackStart(nameEntry, true, true, 0);
			nameBox.Spacing = 10;
			mainVBox.PackStart(nameBox, false, false, 0);
	
			label = new Label();
			label.Show();
			label.Justify = Gtk.Justification.Left;
            label.SetAlignment (0.0f, 0.5f);
			label.LineWrap = false;
			label.UseMarkup = true;
			label.UseUnderline = false;
			label.Markup = "<span weight=\"bold\" size=\"large\">Your Picture</span>";
			mainVBox.PackStart(label, true, true, 0);
		
			Gtk.Table table = new Table(4, 3, false);
			table.Show();
			// None Entry
			noneButton = new RadioButton((Gtk.RadioButton)null);
			noneButton.Show();
			table.Attach(noneButton, 0, 1, 0, 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			VBox vbox = new VBox();
			vbox.Show();
			Gtk.Image image = new Image(Utilities.GetIcon("computer", 48));
			image.Show();
			vbox.PackStart(image, false, false, 0);
			label = new Label("None");
			label.Show();
			vbox.PackStart(label, false, false, 0);
			table.Attach(vbox, 1, 2, 0 ,1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			vbox = new VBox();
			vbox.Show();
			table.Attach(vbox, 2,3,1,2, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Expand | AttachOptions.Fill, 0, 0);

			// Local Entry
			localButton = new RadioButton(noneButton);
			localButton.Show();
			table.Attach(localButton, 0, 1, 1, 2, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			vbox = new VBox();
			vbox.Show();
			localImage = new Image(Utilities.GetIcon("stock_person", 48));
			localImage.Show();
			vbox.PackStart(localImage, false, false, 0);
			label = new Label("File");
			label.Show();
			vbox.PackStart(label, false, false, 0);
			table.Attach(vbox, 1, 2, 1 ,2, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			photoButton = new Button("Change Photo");
			photoButton.Show();
			table.Attach(photoButton, 2,3,1,2, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Shrink, 0, 0);

			// Web Entry
			webButton = new RadioButton(noneButton);
			webButton.Show();
			table.Attach(webButton, 0, 1, 2, 3, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			vbox = new VBox();
			vbox.Show();
			image = new Image(Utilities.GetIcon("web-browser", 48));
			image.Show();
			vbox.PackStart(image, false, false, 0);
			label = new Label("Web Link");
			label.Show();
			vbox.PackStart(label, false, false, 0);
			table.Attach(vbox, 1, 2, 2 ,3, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			webEntry = new Entry();
			webEntry.Show();
			table.Attach(webEntry, 2,3,2,3, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Expand | AttachOptions.Fill, 0, 0);

			// Gravatar Entry
			gravatarButton = new RadioButton(noneButton);
			gravatarButton.Show();
			table.Attach(gravatarButton, 0, 1, 3, 4, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			vbox = new VBox();
			vbox.Show();
			image = new Image(Utilities.GetIcon("gravatar", 48));
			image.Show();
			vbox.PackStart(image, false, false, 0);
			label = new Label("Gravatar");
			label.Show();
			vbox.PackStart(label, false, false, 0);
			table.Attach(vbox, 1, 2, 3 ,4, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);
			gravatarEntry = new Entry();
			gravatarEntry.Show();
			table.Attach(gravatarEntry, 2,3,3,4, AttachOptions.Expand | AttachOptions.Fill, AttachOptions.Expand | AttachOptions.Fill, 0, 0);

			mainVBox.PackStart(table, true, true, 0);


			label = new Label();
			label.Show();
			label.Justify = Gtk.Justification.Left;
            label.SetAlignment (0.0f, 0.5f);
			label.LineWrap = false;
			label.UseMarkup = true;
			label.UseUnderline = false;
			label.Markup = "<span weight=\"bold\" size=\"large\">Your File Location</span>";
			mainVBox.PackStart(label, true, true, 0);
	
			fileLocationButton = new FileChooserButton("Select storage location",
			    FileChooserAction.SelectFolder);
			fileLocationButton.Show();

			mainVBox.PackStart(fileLocationButton, true, true, 0);

			DeleteEvent += WindowDeleted;
		}

		///<summary>
		///	WindowDeleted
		/// Cleans up the conversation object with the ConversationManager
		///</summary>	
		private void WindowDeleted (object sender, DeleteEventArgs args)
		{
			// Save preferences

		}


		private void LoadPreferences()
		{
			nameEntry.Text = Giver.Application.Preferences.UserName;
			fileLocationButton.SetFilename(Giver.Application.Preferences.ReceiveFileLocation);

			photoButton.Sensitive = false;
			webEntry.Sensitive = false;

			if(Giver.Application.Preferences.PhotoType.CompareTo(Giver.Preferences.Local) == 0) {
				localButton.Active = true;
				photoButton.Sensitive = true;
/*
				Logger.Debug("photo type is local");
			   (Glade["local_radiobutton"] as RadioButton).Active = true;
				Image photo = new Image(photoLocation);
			   (Glade["local_radiobutton"] as RadioButton).Image = photo;
			   //(Glade["photo_local_image"] as Image).SetFromIconName(photoLocation, IconSize.Button);
				photoButton
				localImage
*/
			} else if(Giver.Application.Preferences.PhotoType.CompareTo(Giver.Preferences.Uri) == 0) {
				webButton.Active = true;
				webEntry.Text = Giver.Application.Preferences.PhotoLocation;
				webEntry.Sensitive = true;
			} else if(Giver.Application.Preferences.PhotoType.CompareTo(Giver.Preferences.Gravatar) == 0) {
				gravatarButton.Active = true;
				gravatarEntry.Text = Giver.Application.Preferences.PhotoLocation;
			} else {
				// make this none
				noneButton.Active = true;
			}
		}

		private void ConnectEvents()
		{
			nameEntry.Changed += delegate {
				Application.Preferences.UserName = nameEntry.Text;
			};
			
			fileLocationButton.SelectionChanged += delegate {
				Application.Preferences.ReceiveFileLocation = fileLocationButton.Filename;
			};

			noneButton.Toggled += delegate {
				Logger.Debug("nonebutton was toggled");
				if(noneButton.Active)
				{
					Application.Preferences.PhotoType = Preferences.None;
					photoButton.Sensitive = false;
					webEntry.Sensitive = false;
					gravatarEntry.Sensitive = false;
				}
			};

			localButton.Toggled += delegate {
				if(localButton.Active)
				{
					photoButton.Sensitive = true;
					webEntry.Sensitive = false;
					gravatarEntry.Sensitive = false;
				}
			};

			webButton.Toggled += delegate {
				if(webButton.Active)
				{
					photoButton.Sensitive = false;
					webEntry.Sensitive = true;
					gravatarEntry.Sensitive = false;
					Application.Preferences.PhotoType = Preferences.Uri;
					Application.Preferences.PhotoLocation = webEntry.Text;
				}
			};

			webEntry.Changed += delegate {
				Application.Preferences.PhotoLocation = webEntry.Text;
			};

			gravatarButton.Toggled += delegate {
				if(gravatarButton.Active)
				{
					photoButton.Sensitive = false;
					webEntry.Sensitive = false;
					gravatarEntry.Sensitive = true;
					Application.Preferences.PhotoType = Preferences.Gravatar;
					Application.Preferences.PhotoLocation = gravatarEntry.Text;
				}
			};

			gravatarEntry.Changed += delegate {
				Application.Preferences.PhotoLocation = gravatarEntry.Text;
			};

		}

	}
}
