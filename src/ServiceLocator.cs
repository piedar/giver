/***************************************************************************
 *  ServiceLocator.cs
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
		private System.Object resolverQueueLocker;
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
			resolverQueueLocker = new Object();
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

			foreach(Avahi.ServiceInfo si in browser.Services) {
				ServiceResolver resolver = new ServiceResolver (client, si);
				lock(resolverLocker) {
	            	resolvers.Add (resolver);
				}
	            resolver.Found += OnServiceResolved;
	            resolver.Timeout += OnServiceTimeout;
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
			lock(resolverLocker) {
            	resolvers.Add (resolver);
			}
            resolver.Found += OnServiceResolved;
            resolver.Timeout += OnServiceTimeout;

			//Logger.Debug("ServiceLocator:OnServiceAdded : {0}", args.Service.Name);
        }

        private void OnServiceResolved (object o, ServiceInfoArgs args) 
		{
			ServiceResolver sr = (ServiceResolver) o;

			lock(resolverLocker) {
            	resolvers.Remove (sr);
			}

			lock(resolverQueueLocker) {
				resolverQueue.Enqueue(sr);
			}
			resetResolverEvent.Set();
        }

        private void OnServiceTimeout (object o, EventArgs args) 
		{
			Logger.Debug("Service timed out");
			ServiceResolver sr = (ServiceResolver) o;
			
			lock(resolverLocker) {
            	resolvers.Remove (sr);
			}

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

				lock(resolverQueueLocker) {
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
