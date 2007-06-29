////***********************************************************************
//// *  Protocol.cs
//// *
//// *  Copyright (C) 2007 Novell, Inc.
//// *
//// *  This program is free software; you can redistribute it and/or
//// *  modify it under the terms of the GNU General Public
//// *  License as published by the Free Software Foundation; either
//// *  version 2 of the License, or (at your option) any later version.
//// *
//// *  This program is distributed in the hope that it will be useful,
//// *  but WITHOUT ANY WARRANTY; without even the implied warranty of
//// *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//// *  General Public License for more details.
//// *
//// *  You should have received a copy of the GNU General Public
//// *  License along with this program; if not, write to the Free
//// *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
//// *
//// **********************************************************************

using System;

namespace Giver
{
	public class Protocol
	{
		// Requests
		public const string Request = "Request";
		public const string Send = "Send";
		public const string Payload = "Payload";
		public const string Photo = "Photo";


		// Request Responses
		public const string ResponseUnknown = "Unknown Request";
		public const string ResponseOKToSend = "OK to send Payload";
		public const string ResponseMissingSession = "SessionID missing";
		public const string ResponseInvalidSession = "Invalid Session";
		public const string ResponsePayloadReceived = "Payload Received";
		public const string ResponseNoPhoto = "Photograph not available";
		public const string ResponsePhotoSent = "Photo was sent";
		public const string ResponseDeclined = "Request was declined";


		// Headers
		public const string SessionID = "SessionID";
		public const string Count = "Count";
		public const string Name = "Name";
		public const string Type = "Type";
		public const string Size = "Size";
		public const string Response = "Response";
		public const string UserName = "UserName";
		public const string RelativePath = "RelativePath";

		// Protocol Types
		public const string ProtocolTypeFiles = "Files";
		public const string ProtocolTypeFile = "File";
		public const string ProtocolTypeTomboy = "Tomboy";
		
		public Protocol()
		{
		}
	}
}
