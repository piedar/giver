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

using Mono.Zeroconf;


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
        private ServiceBrowser browser;
        private Dictionary <string, ServiceInfo> services;
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
			services = new Dictionary <string, ServiceInfo> ();
			Start();
        }

        public void Start () 
		{
            if (browser == null) {
                browser = new ServiceBrowser();
               
                browser.ServiceAdded += OnServiceAdded;
                browser.ServiceRemoved += OnServiceRemoved;
				
				browser.Browse("_giver._tcp", "local");
            }
        }

        public void Stop () 
		{
            if (browser != null) {
				lock(locker) {
                	services.Clear ();
				}
                browser.Dispose ();
                browser = null;
            }
        }

        private void OnServiceAdded (object o, ServiceBrowseEventArgs args) 
		{            
			// Mono.Zeroconf doesn't expose these flags?
			//if ((args.Service.Flags & LookupResultFlags.Local) > 0 && !showLocals)
            //    return;
			
            args.Service.Resolved += OnServiceResolved;
			args.Service.Resolve();

			//Logger.Debug("ServiceLocator:OnServiceAdded : {0}", args.Service.Name);
        }

        private void OnServiceResolved (object o, ServiceResolvedEventArgs args)
		{
			IResolvableService service = o as IResolvableService;

			lock(locker) {
				if (services.ContainsKey(service.Name)) {
					// TODO: When making changes (like name or photo) at runtime becomes possible
					// this should allow updates to this info
					return; // we already have it somehow
            	}
			}

            ServiceInfo serviceInfo = new ServiceInfo (service.Name, service.HostEntry.AddressList[0], (ushort)service.Port);

			ITxtRecord record = service.TxtRecord;
			serviceInfo.UserName = record["User Name"].ValueString;
			serviceInfo.MachineName = record["Machine Name"].ValueString;
			serviceInfo.Version = record["Version"].ValueString;
			serviceInfo.PhotoType = record["PhotoType"].ValueString;
			serviceInfo.PhotoLocation = record["Photo"].ValueString;
			
			Logger.Debug("Setting default photo");
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


        private void OnServiceRemoved (object o, ServiceBrowseEventArgs args) 
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

    }

}
