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
using System.Threading;

using Avahi;


namespace Giver {

    public delegate void ServiceHandler (object o, ServiceArgs args);

    public class ServiceArgs : EventArgs 
	{
        private ServiceInfo serviceInfo;
        
        public ServiceInfo ServiceInfo 
		{
            get { return serviceInfo; }
        }
        
        public ServiceArgs (ServiceInfo serviceInfo) 
		{
            this.serviceInfo = serviceInfo;
        }
    }


    public class ServiceLocator 
	{
		private System.Object locker;
		private System.Object resolverLocker;
        private Avahi.Client client;
        private ServiceBrowser browser;
        private Dictionary <string, ServiceInfo> services;
        private List <ServiceResolver> resolvers;
		private Queue <ServiceResolver> resolverQueue;
		private Thread resolverThread;
		private AutoResetEvent resetResolverEvent;
		private bool runningResolverThread;
        private bool showLocals = true;

        public event ServiceHandler ServiceAdded;
        public event ServiceHandler ServiceRemoved;

        public bool ShowLocalServices 
		{
            get { return showLocals; }
            set { showLocals = value; }
        }

		public int Count
		{
			get 
			{
				int count;
				lock(locker) {
					count = services.Count;
				}
				return count;
			}
		}
        
        public ServiceInfo[] Services 
		{
            get 
			{
				List<ServiceInfo> serviceList;

				lock(locker) { 
					serviceList = new List<ServiceInfo> (services.Values);
				}
				return serviceList.ToArray();
			}
        }
        
        public ServiceLocator () 
		{
			locker = new Object();
			resolverLocker = new Object();
			services = new Dictionary <string, ServiceInfo> ();
        	resolvers = new List <ServiceResolver> ();
			resolverQueue = new Queue<ServiceResolver> ();
			resetResolverEvent = new AutoResetEvent(false);
			Start();
        }

        public void Start () 
		{
            if (client == null) {
                client = new Avahi.Client ();
                
                browser = 
                	new ServiceBrowser (client, -1, Avahi.Protocol.IPv4, "_giver._tcp", "local", Avahi.LookupFlags.UseMulticast);
                //browser = new ServiceBrowser ((client, "_giver._tcp");
               
                browser.ServiceAdded += OnServiceAdded;
                browser.ServiceRemoved += OnServiceRemoved;
            }

			runningResolverThread = true;
			resolverThread  = new Thread(ResolverThreadLoop);
			resolverThread.Start();
        }

        public void Stop () 
		{
			runningResolverThread = false;
			resetResolverEvent.Set();

            if (client != null) {
				lock(locker) {
                	services.Clear ();
				}
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

			//Logger.Debug("ServiceLocator:OnServiceAdded : {0}", args.Service.Name);
        }

        private void OnServiceResolved (object o, ServiceInfoArgs args) 
		{
			ServiceResolver sr = (ServiceResolver) o;

            resolvers.Remove (sr);

			lock(resolverLocker) {
				resolverQueue.Enqueue(sr);
			}
			resetResolverEvent.Set();
        }

        private void OnServiceTimeout (object o, EventArgs args) 
		{
			Logger.Debug("Service timed out");
			ServiceResolver sr = (ServiceResolver) o;

            resolvers.Remove (sr);
            sr.Dispose ();
        }

        private void OnServiceRemoved (object o, ServiceInfoArgs args) 
		{
			Logger.Debug("A Service was removed: {0}", args.Service.Name);

			lock(locker) {
				if(services.ContainsKey(args.Service.Name)) {
					ServiceInfo serviceInfo = services[args.Service.Name];
	            	if (serviceInfo != null)
	                	services.Remove (serviceInfo.Name);

	                if (ServiceRemoved != null)
	                    ServiceRemoved (this, new ServiceArgs (serviceInfo));
	            }
			}
        }


		private void ResolverThreadLoop()
		{
			while(runningResolverThread) {

				resetResolverEvent.WaitOne();

				ServiceResolver sr = null;
				resetResolverEvent.Reset();

				lock(resolverLocker) {
					if(resolverQueue.Count > 0)
						sr = resolverQueue.Dequeue();
				}

				// if there was nothing to do, re-loop
				if(sr == null)
					continue;

	            string name = sr.Service.Name;

				lock(locker) {
					if (services.ContainsKey(sr.Service.Name)) {
						continue; // we already have it somehow
	            	}
				}

	            ServiceInfo serviceInfo = new ServiceInfo (name, sr.Service.Address, sr.Service.Port);

	            foreach (byte[] txt in sr.Service.Text) {
	                string txtstr = Encoding.UTF8.GetString (txt);
	                string[] splitstr = txtstr.Split('=');

	                if (splitstr.Length < 2)
	                    continue;

					if(splitstr[0].CompareTo("User Name") == 0)
						serviceInfo.UserName = splitstr[1];
					if(splitstr[0].CompareTo("Machine Name") == 0)
						serviceInfo.MachineName = splitstr[1];
					if(splitstr[0].CompareTo("Version") == 0)
						serviceInfo.Version = splitstr[1];
					if(splitstr[0].CompareTo("PhotoType") == 0)
						serviceInfo.PhotoType = splitstr[1];
					if(splitstr[0].CompareTo("Photo") == 0)
						serviceInfo.PhotoLocation = splitstr[1];
	            }

				serviceInfo.Photo = Utilities.GetIcon("blankphoto", 48);
				lock(locker) {
					services[serviceInfo.Name] = serviceInfo;
					
					if(serviceInfo.PhotoType.CompareTo(Preferences.Local) == 0 ||
						serviceInfo.PhotoType.CompareTo (Preferences.Gravatar) == 0 ||
						serviceInfo.PhotoType.CompareTo (Preferences.Uri) == 0) {
						// Queue the resolution of the photo
						PhotoService.QueueResolve (serviceInfo);
					}		
				}

	            if (ServiceAdded != null)
				{
					Logger.Debug("About to call ServiceAdded");
	                ServiceAdded (this, new ServiceArgs (serviceInfo));
					Logger.Debug("ServiceAdded was just called");
				} else {
					Logger.Debug("ServiceAdded was null and not called");
				}

			}
		}

    }

}
