////***********************************************************************
//// *   /home/calvin/code/giver/src/ServiceInfo.cs
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
