/***************************************************************************
 *  ServiceInfo.cs
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
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Giver
{
    public class ServiceInfo 
	{
        private IPAddress address;
        private ushort port;
        private string machineName;
		private string userName;
		private string name;
		private string version;
		private string photoType;
		private string photoLocation;
		private string id;
		private Gdk.Pixbuf photo;

        public IPAddress Address 
		{
            get { return address; }
			set { this.address = value; }
        }

        public ushort Port 
		{
            get { return port; }
			set { this.port = value; }
        }

        public string MachineName 
		{
            get { return machineName; }
			set { this.machineName = value; }
        }

		public string Name
		{
			get { return name; }
			set { this.name = value; }
		}

		public string UserName
		{
			get { return userName; }
			set { this.userName = value; }
		}

		public string Version
		{
			get { return version; }
			set { this.version = value; }
		}

		public string PhotoType
		{
			get { return photoType; }
			set { this.photoType = value; }
		}

		public string PhotoLocation
		{
			get { return photoLocation; }
			set { this.photoLocation = value; }
		}

		public Gdk.Pixbuf Photo
		{
			get { return photo; }
			set { this.photo = value; }
		}

		public string ID
		{
			get { return id; }
			set { this.id = value; }
		}

        public ServiceInfo (string name, IPAddress address, ushort port)
		{
			this.name = name;
            this.address = address;
            this.port = port;
            this.machineName = address.ToString();
			this.userName = "";
			this.version = "";
			this.photoType = Preferences.None;
			this.photoLocation = "";
			this.photo = null;
			this.id = String.Format("{0}@{1}:{2}", name, address.ToString(), port);
        }

        public override string ToString()
        {
            return String.Format("{0}@{1}:{2} ({3}) - {4}", UserName, Address, Port, MachineName, Version);
        }
    }
}
