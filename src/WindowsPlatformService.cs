/***************************************************************************
 *  WindowsPlatformService.cs
 *
 *  Written by Ankit Jain <radical@gmail.com>
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
using Gtk;

namespace Giver
{
	public class WindowsPlatformService : PlatformService
	{
		public override IDesktopApplication CreateDesktopApplication (
				string app_name, string version, string [] args)
		{
			return new GtkApplication (app_name, args);
		}

		//TODO
		public override void SetProcessName (string name)
		{
		}

		//TODO
		public override void PlaySoundFile (string filename)
		{
		}

		//TODO: Use resources?
		public override string GetString (string format, params object [] args)
		{
			return String.Format (format, args);
		}

		public override void ShowMessage (string title, string message, Gdk.Pixbuf icon)
		{
			MessageDialog dialog = new MessageDialog (null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
					message);
			if (icon != null) {
				dialog.Image = new Image (icon);
				dialog.Image.Show ();
			}

			dialog.Run ();
			dialog.Destroy ();
		}

		public override void AskYesNoQuestion (string title, string message, Gdk.Pixbuf icon,
				string ok_string, string cancel_string,
				EventHandler ok_handler, EventHandler cancel_handler)
		{
			MessageDialog dialog = new MessageDialog (null, DialogFlags.DestroyWithParent | DialogFlags.Modal,
					MessageType.Other, ButtonsType.None, true, null);

			dialog.UseMarkup = true;
			dialog.Title = "Giver";
			if (!String.IsNullOrEmpty (title))
				dialog.Text = "<b>" + title + "</b>";
			dialog.SecondaryText = message;
			if (icon != null) {
				dialog.Image = new Image (icon);
				dialog.Image.Show ();
			}

			dialog.AddButton (cancel_string, ResponseType.No);
			dialog.AddButton (ok_string, ResponseType.Yes);
			dialog.DefaultResponse = ResponseType.Yes;

			dialog.Response += delegate (object o, ResponseArgs args) {
				if (args.ResponseId == ResponseType.Yes && ok_handler != null)
					ok_handler (null, null);
				else if (args.ResponseId == ResponseType.No && cancel_handler != null)
					cancel_handler (null, null);
			};

			dialog.Close += delegate {
				if (cancel_handler != null)
					cancel_handler (null, null);
			};

			dialog.Run ();
			dialog.Destroy ();
		}
	}
}
