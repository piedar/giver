/***************************************************************************
 *  PlatformService.cs
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

namespace Giver
{
	public abstract class PlatformService
	{
		public abstract IDesktopApplication CreateDesktopApplication (string app_name, string version, string [] args);

		public abstract void SetProcessName (string name);
		public abstract void PlaySoundFile (string filename);
		public abstract string GetString (string format, params object [] args);

		public abstract void ShowMessage (string title, string message, Gdk.Pixbuf icon);
		public abstract void AskYesNoQuestion (string title, string message, Gdk.Pixbuf icon,
				string ok_string, string cancel_string,
				EventHandler ok_handler, EventHandler cancel_handler);
	}
}
