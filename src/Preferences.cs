//***********************************************************************
// *  $RCSfile$ - Preferences.cs
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

namespace Giver
{
	// <summary>
	// Class used to store Giver preferences
	// </summary>
	public class Preferences
	{
		// public static event PreferenceChangedEventHandler PreferenceChanged;
		
//	   	public const string CustomAvailableMessages = "/apps/banter/custom_available_messages";
//	   	public const string CustomBusyMessages = "/apps/banter/custom_busy_messages";


		public bool HasPhoto
		{
			get { return true; }
		}

		public string PhotoLocation
		{
			get { return "local"; }
		}

		public string LocalPhotoLocation
		{
			get { return "/home/calvin/calvin.png"; }
		}

		public Preferences ()
		{
			// Initialize the preferences
		}
	}
}
