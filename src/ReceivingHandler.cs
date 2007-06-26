////***********************************************************************
//// *  ReceivingHandler.cs
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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Giver
{
	/// <summary>
	/// Receives and handles all incoming giver requests
	/// </summary>	
	public class ReceivingHandler
	{
		private HttpListenerContext context;
		private Stream stream;
		private PayloadInfo payloadInfo;

		public ReceivingHandler(HttpListenerContext context)
		{
			this.context = context;
			this.stream = context.Request.InputStream;
			ReadPayloadInfo();
			Logger.Debug("We are being send the file: {0}", payloadInfo.Name);
		}

		private void ReadPayloadInfo()
		{
			try {
				int size;

				size = ReadInt(stream);
				string name = ReadString(stream, size);
				PayloadType type = (PayloadType) ReadInt(stream);

				payloadInfo = new PayloadInfo(name, type);
			} catch (Exception e) {
				Logger.Debug("Errror reading the PayloadInfo : {0}", e.Message);
				payloadInfo = null;
			}
		}

		private int ReadInt(Stream stream)
		{
			int bytesRead;
			Byte[] buffer = new Byte[4];

			bytesRead = stream.Read(buffer, 0, 4);
			if(bytesRead != 4)
				throw new Exception("Unable to read 4 bytes");

			return (int)buffer[0];
		}

		private string ReadString(Stream stream, int size)
		{
			int bytesRead;

			// Check for a very large string size here and blow if it's too big
			if(size > 32768)
				throw new Exception("String too big to read");

			Byte[] buffer = new Byte[size];

			bytesRead = stream.Read(buffer, 0, size);
			if(bytesRead < size)
				throw new Exception("Unable to read all bytes from stream");

			return Encoding.UTF8.GetString(buffer);
		}
	}
}


