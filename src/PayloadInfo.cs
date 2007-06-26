//***********************************************************************
// *  PayloadInfo.cs
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
	/// <summary>
	/// Type of Payload being sent
	/// </summary>
	public enum PayloadType : uint
	{
		File = 1,
		Folder
	}

	/// <summary>
	/// Sent as the first bit of data in requesting a file be sent to Giver
	/// </summary>
	public class PayloadInfo
	{
		#region Private Types
		private string name;
        private PayloadType type;
        private string senderName;
		private string senderMachineName;
		private string senderPhoto;
		private string senderVersion;
		private string senderPublicKey;
		#endregion		

		public string Name
		{
			get { return name; }
		}

		public PayloadInfo(string name, PayloadType type)
		{
			this.name = name;
			this.type = type;
		}
	}
}
