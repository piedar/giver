/***************************************************************************
 *  GnomePlatformService.cs
 *
 *  Copyright (C) 2007 Novell, Inc.
 *  Written by Calvin Gaisford <calvinrg@gmail.com>
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
using System.Text;
using System.Runtime.InteropServices;

namespace Giver
{
	public class GnomePlatformService : PlatformService
	{
		public override IDesktopApplication CreateDesktopApplication (string app_name, string version, string [] args)
		{
			return new GnomeApplication (app_name, version, args);
		}

		public override void SetProcessName (string name)
		{
			// 15 = PR_SET_NAME
			if (prctl (15, Encoding.ASCII.GetBytes (name + "\0"), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0)
			{
				throw new ApplicationException("Error setting process name: " +
					Mono.Unix.Native.Stdlib.GetLastError());
			}
		}

		public override void PlaySoundFile (string filename)
		{
			Gnome.Sound.Play (filename);
		}

		public override string GetString (string format, params object [] args)
		{
			return String.Format (Mono.Unix.Catalog.GetString (format), args);
		}

		[DllImport("libc")]
		private static extern int prctl (int option, byte [] arg2, IntPtr arg3,
			IntPtr arg4, IntPtr arg5);
	}
}
