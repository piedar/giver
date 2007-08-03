/***************************************************************************
 *  TransferStatusArgs.cs
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
