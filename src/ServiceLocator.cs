//***********************************************************************
// *  ServiceLocator.cs
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
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using Avahi;


namespace Giver {

    public delegate void ServiceHandler (object o, ServiceArgs args);

    public class ServiceArgs : EventArgs 
	{

        private Service service;
        
        public Service Service 
		{
            get { return service; }
        }
        
        public ServiceArgs (Service service) 
		{
            this.service = service;
        }
    }

    public class Service 
	{
        private IPAddress address;
        private ushort port;
        private string machineName;
		private string userName;
		private string name;
		private string version;
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

		public Gdk.Pixbuf Photo
		{
			get { return photo; }
			set { this.photo = value; }
		}

        public Service (string name, IPAddress address, ushort port)
		{
			this.name = name;
            this.address = address;
            this.port = port;
            this.machineName = "";
			this.userName = "";
			this.version = "";
			this.photo = null;
        }

        public override string ToString()
        {
            return String.Format("{0}@{1}:{2} ({3}) - {4}", UserName, Address, Port, MachineName, Version);
        }
    }
    
    
    public class ServiceLocator 
	{
        private Avahi.Client client;
        private ServiceBrowser browser;
        private Dictionary <string, Service> services;
        private List <ServiceResolver> resolvers;
        private bool showLocals = false;

        public event ServiceHandler Found;
        public event ServiceHandler Removed;

        public bool ShowLocalServices 
		{
            get { return showLocals; }
            set { showLocals = value; }
        }
        
        public IEnumerable Services 
		{
            get { return services.Values; }
        }
        
        public ServiceLocator () 
		{
			services = new Dictionary <string, Service> ();
        	resolvers = new List <ServiceResolver> ();
			Start();
        }

        public void Start () 
		{
            if (client == null) {
                client = new Avahi.Client ();
                browser = new ServiceBrowser (client, "_giver._tcp");
                browser.ServiceAdded += OnServiceAdded;
                browser.ServiceRemoved += OnServiceRemoved;
            }
        }

        public void Stop () 
		{
            if (client != null) {
                services.Clear ();
                browser.Dispose ();
                client.Dispose ();
                client = null;
                browser = null;
            }
        }

        private void OnServiceAdded (object o, ServiceInfoArgs args) 
		{
            if ((args.Service.Flags & LookupResultFlags.Local) > 0 && !showLocals)
                return;
            
            ServiceResolver resolver = new ServiceResolver (client, args.Service);
            resolvers.Add (resolver);
            resolver.Found += OnServiceResolved;
            resolver.Timeout += OnServiceTimeout;

			Logger.Debug("ServiceLocator:OnServiceAdded : {0}", args.Service.Name);
        }

        private void OnServiceResolved (object o, ServiceInfoArgs args) 
		{
			ServiceResolver sr = (ServiceResolver) o;

            resolvers.Remove (sr);
            sr.Dispose ();

            string name = args.Service.Name;

			if (services.ContainsKey(args.Service.Name)) {
                return; // we already have it somehow
            }

            Service svc = new Service (name, args.Service.Address, args.Service.Port);

            foreach (byte[] txt in args.Service.Text) {
                string txtstr = Encoding.UTF8.GetString (txt);
                string[] splitstr = txtstr.Split('=');

                if (splitstr.Length < 2)
                    continue;

				if(splitstr[0].CompareTo("User Name") == 0)
					svc.UserName = splitstr[1];
				if(splitstr[0].CompareTo("Machine Name") == 0)
					svc.MachineName = splitstr[1];
				if(splitstr[0].CompareTo("Version") == 0)
					svc.Version = splitstr[1];
				if(splitstr[0].CompareTo("Photo") == 0) {
					// convert the string to a photo here and store it
					// svc.Photo = ConvertToPhoto(splitstr[1];					
				}
            }

            services[svc.MachineName] = svc;

            if (Found != null)
                Found (this, new ServiceArgs (svc));
        }

        private void OnServiceTimeout (object o, EventArgs args) 
		{
            Console.Error.WriteLine ("Failed to resolve");
        }

        private void OnServiceRemoved (object o, ServiceInfoArgs args) 
		{
			if(services.ContainsKey(args.Service.Name)) {
				Service svc = services[args.Service.Name];
            	if (svc != null)
                	services.Remove (svc.Name);

                if (Removed != null)
                    Removed (this, new ServiceArgs (svc));
            }
        }

    }

}
