/***************************************************************************
 *  Protocol.cs
 *
 *  Copyright (C) 2007 Novell, Inc.
 *  Written by Calvin Gaisford <calvinrg@gmail.com>
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
		public const string ProtocolTypeTasque = "Tasque";
		
		public Protocol()
		{
		}
	}
}
