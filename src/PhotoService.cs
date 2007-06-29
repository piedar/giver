// created on 6/29/2007 at 10:17 AM
using System;
using System.Collections.Generic;
using System.Threading;

namespace Giver
{
    public delegate void PhotoResolvedHandler (ServiceInfo serviceInfo);

	/// <summary>
	/// Class for retrieving Giver user photos
	/// </summary>
    public class PhotoService 
	{
		//private System.Object locker;
		static private System.Object serviceLocker;
		static private Queue <ServiceInfo> outstanding;
		private Thread photoServiceThread;
		static private AutoResetEvent photoServiceEvent;
		private bool running;
		
		public event PhotoResolvedHandler PhotoResolved;

		public int Outstanding
		{
			get { return outstanding.Count; }
		}
        
        static PhotoService ()
        {
        	Logger.Debug ("PhotoService static constructor called");
			serviceLocker = new Object ();
			outstanding = new Queue<ServiceInfo> ();
			photoServiceEvent = new AutoResetEvent (false);
        }
        
        public PhotoService () 
		{
			//locker = new Object ();
			//photoServiceEvent = new AutoResetEvent (false);
        }

		private void PhotoServiceThread ()
		{
			running = true;
			
			while (running) {

				PhotoService.photoServiceEvent.WaitOne ();
				if (running == false) continue;

				ServiceInfo serviceInfo = null;
				//resetResolverEvent.Reset();

				lock (PhotoService.serviceLocker) {
					if (PhotoService.outstanding.Count > 0)
						serviceInfo = PhotoService.outstanding.Dequeue ();
				}

				// if there was nothing to do, re-loop
				if (serviceInfo == null)
					continue;

				Logger.Debug ("Resolving photo for: {0}", serviceInfo.UserName);
				try {
					if (serviceInfo.PhotoType.CompareTo(Preferences.Local) == 0 ) {
						SendingHandler.GetPhoto (serviceInfo);
						serviceInfo.Photo = serviceInfo.Photo.ScaleSimple(48, 48, Gdk.InterpType.Bilinear);
					} else if (serviceInfo.PhotoType.CompareTo (Preferences.Gravatar) == 0 ){
						string uri = Utilities.GetGravatarUri (serviceInfo.PhotoLocation);
						serviceInfo.Photo = Utilities.GetPhotoFromUri (uri);
					} else if (serviceInfo.PhotoType.CompareTo (Preferences.Uri) == 0) {
						serviceInfo.Photo = Utilities.GetPhotoFromUri (serviceInfo.PhotoLocation);
						serviceInfo.Photo = serviceInfo.Photo.ScaleSimple (48, 48, Gdk.InterpType.Bilinear);
					} else {
						serviceInfo.Photo = Utilities.GetIcon ("computer", 48);
					}
					
					// Call registered listeners
					if (PhotoResolved != null)
						PhotoResolved (serviceInfo);
						
				} catch (Exception e) {
				
					// FIXME:: Requeue and try again if the photo is
					// coming from the network
					Logger.Debug("Exception getting photo {0}", e);
					//photoInfo.Photo = Utilities.GetIcon ("computer", 48);
				}
			}
		}

		#region Public Methods		
		static public void QueueResolve (ServiceInfo serviceInfo)
		{
			lock (serviceLocker) {
				PhotoService.outstanding.Enqueue (serviceInfo);		
				PhotoService.photoServiceEvent.Set ();
			}
		}
		
        public void Start () 
		{
			photoServiceThread  = new Thread (PhotoServiceThread);
			photoServiceThread.Start ();
        }

        public void Stop () 
		{
			running = false;
			PhotoService.photoServiceEvent.Set ();
        }
		#endregion
    }
}