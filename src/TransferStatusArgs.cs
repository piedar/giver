////***********************************************************************
//// *  TransferStatus.cs
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
    public delegate void TransferStartedHandler (TransferStatusArgs args);
    public delegate void FileTransferStartedHandler (TransferStatusArgs args);
    public delegate void TransferProgressHandler (TransferStatusArgs args);
    public delegate void TransferEndedHandler (TransferStatusArgs args);

	public class TransferStatusArgs
	{
		private int totalCount;
		private int currentCount;
		private string name;
		private string type;
		private long totalBytes;
		private long totalBytesTransferred;
		private long currentBytes;
		private long currentBytesTransferred;
		private ServiceInfo serviceInfo;

		///<summary>
		///	TotalCount
		/// The total count of files to be sent
		///</summary>
		public int TotalCount
		{
			get { return this.totalCount; }
		}

		///<summary>
		///	CurrentCount
		/// The count of files that have been sent
		///</summary>
		public int CurrentCount
		{
			get { return this.currentCount; }
		}

		///<summary>
		///	Name
		/// The name of the item being sent
		///</summary>
		public string Name
		{
			get { return this.name; }
		}

		///<summary>
		///	Type
		/// The type of the item being sent
		///</summary>
		public string Type
		{
			get { return this.type; }
		}

		///<summary>
		///	TotalBytes
		/// The total bytes that will be transfered
		///</summary>
		public long TotalBytes
		{
			get { return this.totalBytes; }
		}

		///<summary>
		///	TotalBytes
		/// The total bytes that have been transfered
		///</summary>
		public long TotalBytesTransferred
		{
			get { return this.totalBytesTransferred; }
		}


		///<summary>
		///	CurrentBytes
		/// The number of bytes in the current file
		///</summary>
		public long CurrentBytes
		{
			get { return this.currentBytes; }
		}


		///<summary>
		///	ServiceInfo
		/// The serviceInfo for this send.. This will be null on the receiving end
		///</summary>
		public ServiceInfo TargetServiceInfo
		{
			get { return this.serviceInfo; }
		}


		///<summary>
		///	CurrentBytesTransferred
		/// The number of bytes that have been transfered in the current file
		///</summary>
		public long CurrentBytesTransferred
		{
			get { return this.currentBytesTransferred; }
		}

		public TransferStatusArgs(int totalCount, int currentCount, string name, string type,
									long totalBytes, long totalBytesTransferred, long currentBytes,
									long currentBytesTransferred, ServiceInfo serviceInfo )
		{
			this.totalCount = totalCount;
			this.currentCount = currentCount;
			this.name = name;
			this.type = type;
			this.totalBytes = totalBytes;
			this.totalBytesTransferred = totalBytesTransferred;
			this.currentBytes = currentBytes;
			this.currentBytesTransferred = currentBytesTransferred;
			this.serviceInfo = serviceInfo;
		}
	}
}
