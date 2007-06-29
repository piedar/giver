////***********************************************************************
//// *  SendingHandler.cs
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
using System.Collections.Generic;
using Notifications;
using Mono.Unix;
using System.Xml;

namespace Giver
{
	public class SendFilePair
	{
		public string file;
		public string relativePath;

		public SendFilePair(string file, string relativePath)
		{
			this.file = file;
			this.relativePath = relativePath;
		}
	}

	public class SendingHolder
	{
		public ServiceInfo serviceInfo;
		public string[] files;
		public List<SendFilePair> filePairs;
		public int fileCount;
		public long totalSize;

		public SendingHolder()
		{
			filePairs = new List<Giver.SendFilePair> ();
		}
	}

	public class SendingHandler
	{
		private System.Object locker;
		private Queue<SendingHolder> queue;
		private bool running;
		private Thread sendingThread;
		private AutoResetEvent resetEvent;

    	public TransferStartedHandler TransferStarted;
    	public FileTransferStartedHandler FileTransferStarted;
    	public TransferProgressHandler TransferProgress;
    	public TransferEndedHandler TransferEnded;

		public SendingHandler()
		{
			resetEvent = new AutoResetEvent(false);
			running = false;
			locker = new System.Object();
			queue = new Queue<SendingHolder> ();
		}

		public void Start()
		{
			running = true;
			sendingThread  = new Thread(SendFileThread);
			sendingThread.Start();
		}

		public void Stop()
		{
			running = false;
			resetEvent.Set();
		}

		public void QueueFileSend(ServiceInfo serviceInfo, string[] files)
		{
			lock(locker) {
				Logger.Debug("SEND: Queueing up {0} files to send", files.Length);
				SendingHolder sh = new SendingHolder();
				sh.files = files;
				sh.serviceInfo = serviceInfo;

				queue.Enqueue(sh);
			}
			resetEvent.Set();
		}

		private void CalculateSendingHolderData(SendingHolder sh)
		{
			sh.fileCount = 0;
			sh.totalSize = 0;
			sh.filePairs.Clear();

			foreach(string file in sh.files) {
				if(File.Exists(file)) {
					//Logger.Debug("SEND: About to send file: {0}", file);
					// this is a file, figure it out
					FileInfo fi = new FileInfo(file);
					sh.totalSize += fi.Length;
					sh.filePairs.Add(new SendFilePair(file, ""));
					sh.fileCount++;
				} else if(Directory.Exists(file)) {
					DirectoryInfo di = new DirectoryInfo(file);
					CalculateFolderData(sh, di, di.Name);
				}
			}
		}

		private void CalculateFolderData(SendingHolder sh, DirectoryInfo dirInfo, string relativePath)
		{
			foreach(FileInfo file in dirInfo.GetFiles())
			{
				//Logger.Debug("SEND: About to send file: {0}", file);
				sh.totalSize += file.Length;
				sh.filePairs.Add(new SendFilePair(file.FullName, relativePath));
				sh.fileCount++;
			}

			foreach(DirectoryInfo di in dirInfo.GetDirectories())
			{
				CalculateFolderData(sh, di, Path.Combine(relativePath, di.Name));
			}
		}

		private void SendFileThread()
		{
			while(running) {

				resetEvent.WaitOne();

				SendingHolder sh = null;
				resetEvent.Reset();

				lock(locker) {
					if(queue.Count > 0)
						sh = queue.Dequeue();
				}

				// if there was nothing to do, re-loop
				if(sh == null)
					continue;

				ServiceInfo serviceInfo = sh.serviceInfo;

				// Calculate how many files to send and how big they are
				CalculateSendingHolderData(sh);
				//Logger.Debug("SEND: About to request send for {0} files at {1} bytes", sh.fileCount, sh.totalSize);

				UriBuilder urib = new UriBuilder("http", serviceInfo.Address.ToString(), (int)serviceInfo.Port);
				//Logger.Debug("SEND: Sending request to URI: {0}", urib.Uri.ToString());

				System.Net.HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(urib.Uri);

				// Send request to send the file
				request.Method = "GET";
				request.Headers.Set(Protocol.Request, Protocol.Send);
				request.Headers.Set(Protocol.UserName, Application.Preferences.UserName);
				request.Headers.Set(Protocol.Type, Protocol.ProtocolTypeFiles);
				request.Headers.Set(Protocol.Count, sh.fileCount.ToString());
//				*** START OF TOMBOY HACK ***

				if(sh.fileCount == 1) {
					// Complete and total hack for TomBoy
					Logger.Debug("The extension is: {0}", Path.GetExtension(sh.files[0]));
					if(Path.GetExtension(sh.files[0]).CompareTo(".note") == 0) {
						Logger.Debug("I got a Note file");
						try {
							StreamReader reader = new StreamReader (sh.files[0]);
							string noteXml = reader.ReadToEnd ();
							string title = null;
							reader.Close ();
							XmlTextReader xml = new XmlTextReader (new StringReader (noteXml));
							xml.Namespaces = false;
							while (xml.Read ()) {
								switch (xml.NodeType) {
									case XmlNodeType.Element:
										switch (xml.Name) {
											case "title":
												title = xml.ReadString ();
												break;
										}
										break;
								}
							}
							if(title != null) {
								request.Headers.Set(Protocol.Name, title);
								request.Headers.Set(Protocol.Type, Protocol.ProtocolTypeTomboy);
							} else {
								Logger.Debug("The node is null");
								request.Headers.Set(Protocol.Name, Path.GetFileName(sh.files[0]));
							}
						} catch (Exception e) {
							Logger.Debug("Exception getting note {0}", e);
							request.Headers.Set(Protocol.Name, Path.GetFileName(sh.files[0]));
						}	
					} else {
						request.Headers.Set(Protocol.Name, Path.GetFileName(sh.files[0]));
					}
				}
//				*** END OF TOMBOY HACK ***

//				if(sh.fileCount == 1)
//					request.Headers.Set(Protocol.Name, Path.GetFileName(sh.files[0]));
				else
					request.Headers.Set(Protocol.Name, "many");

				request.Headers.Set(Protocol.Size, sh.totalSize.ToString());

				//Logger.Debug("SEND: about to perform a GET for the file send request");
				HttpWebResponse response;

				// Read the response to the request
				try {
					response = (HttpWebResponse)request.GetResponse();
				} catch(System.Net.WebException we) {
					if(we.Response != null)
						response = (HttpWebResponse)we.Response;
					else {
						Logger.Debug("SEND: Exception in getting response {0}", we.Message);
						continue;
					}
				} catch (Exception e) {
					Logger.Debug("SEND: Exception in request.GetResponse(): {0}", e);
					continue;
				}
			
				Logger.Debug("SEND: Response Status code {0}", response.StatusCode);

				if( response.StatusCode == HttpStatusCode.OK ) {
					//Logger.Debug("SEND: Response was OK");

					string sessionID = response.Headers[Protocol.SessionID];
					response.Close();
					int counter = 0;
					long entireSize = 0;

					if(TransferStarted != null) {
						TransferStarted(new TransferStatusArgs(sh.fileCount, 0, "",
											Protocol.ProtocolTypeFile, sh.totalSize,
											0, request.ContentLength, 0, serviceInfo ));
					}


					foreach(SendFilePair filePair in sh.filePairs) {
						request = (HttpWebRequest) HttpWebRequest.Create(urib.Uri);
						request.Method = "POST";
						request.Headers.Set(Protocol.SessionID, sessionID);
						request.Headers.Set(Protocol.Request, Protocol.Payload);
						request.Headers.Set(Protocol.Type, Protocol.ProtocolTypeFile);
						request.Headers.Set(Protocol.Count, counter.ToString());
						request.Headers.Set(Protocol.Name, Path.GetFileName(filePair.file));
						request.Headers.Set(Protocol.RelativePath, filePair.relativePath);

						try {
							System.IO.FileStream filestream = File.Open(filePair.file, FileMode.Open, FileAccess.Read);
							request.ContentLength = filestream.Length;
							Stream stream = request.GetRequestStream();
							
							int sizeRead = 0;
							int totalRead = 0;
							byte[] buffer = new byte[2048];

							if(FileTransferStarted != null) {
								FileTransferStarted(new TransferStatusArgs(sh.fileCount, counter, filePair.file,
													Protocol.ProtocolTypeFile, sh.totalSize,
													entireSize, request.ContentLength, totalRead, serviceInfo ));
							}


							do {
								sizeRead = filestream.Read(buffer, 0, 2048);
								totalRead += sizeRead;
								entireSize += sizeRead;
								if(sizeRead > 0) {
									stream.Write(buffer, 0, sizeRead);
								}

								if(TransferProgress != null) {
									TransferProgress(new TransferStatusArgs(sh.fileCount, counter, filePair.file,
														Protocol.ProtocolTypeFile, sh.totalSize,
														entireSize, request.ContentLength, totalRead, serviceInfo ));
								}


							} while(sizeRead == 2048);
							//Logger.Debug("SEND: We Read from the file {0} bytes", totalRead);
							//Logger.Debug("SEND: The content length is {0} bytes", filestream.Length);

							stream.Close();
							filestream.Close();

							// Read the response to the request
							try {
								response = (HttpWebResponse)request.GetResponse();
							} catch(System.Net.WebException we) {
								if(we.Response != null)
									response = (HttpWebResponse)we.Response;
								else {
									Logger.Debug("SEND: Exception in getting response {0}", we.Message);
									continue;
								}
							} catch (Exception e) {
								Logger.Debug("SEND: Exception in request.GetResponse(): {0}", e);
								continue;
							}

							response.Close();

						} catch (Exception e) {
							Logger.Debug("SEND: Exception when sending file: {0}", e.Message);
							Logger.Debug("SEND: Exception {0}", e);
						}
						counter++;		
					}

					if(TransferEnded != null) {
						TransferEnded(new TransferStatusArgs(sh.fileCount, counter, "",
											Protocol.ProtocolTypeFile, sh.totalSize,
											entireSize, request.ContentLength, 0, serviceInfo ));
					}



					//Logger.Debug("RECEIVE: About to do a Gtk.Application.Invoke for the notify dude.");
					Gtk.Application.Invoke( delegate {
						string body = String.Format(Catalog.GetString("{0} has received the file(s)."), serviceInfo.UserName);
						//Logger.Debug("RECEIVE: Inside the Gtk.Application.Invoke dude");
						Notification notify = new Notification(	Catalog.GetString("Done Giving Files"), 
																body,
																serviceInfo.Photo);

						Application.ShowAppNotification(notify);
						Gnome.Sound.Play(Path.Combine(Giver.Defines.SoundDir, "notify.wav"));
					} );

				} else {
					//Logger.Debug("RECEIVE: About to do a Gtk.Application.Invoke for the notify dude.");
					Gtk.Application.Invoke( delegate {
						string body = String.Format(Catalog.GetString("{0} declined your request to give files."), serviceInfo.UserName);
						//Logger.Debug("RECEIVE: Inside the Gtk.Application.Invoke dude");
						Notification notify = new Notification(	Catalog.GetString("Giving Was Declined"), 
																body,
																serviceInfo.Photo);

						Application.ShowAppNotification(notify);
						Gnome.Sound.Play(Path.Combine(Giver.Defines.SoundDir, "notify.wav"));
					} );
				}
			
				Logger.Debug("SEND: Done with Sending file");
			}
		}


		public static void GetPhoto(ServiceInfo serviceInfo)
		{
			UriBuilder urib = new UriBuilder("http", serviceInfo.Address.ToString(), (int)serviceInfo.Port);
			Logger.Debug("Sending request to URI: {0}", urib.Uri.ToString());
			System.Net.HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(urib.Uri);

			// Send request to send the file
			request.Method = "POST";
			request.Headers.Set(Protocol.Request, Protocol.Photo);
			request.ContentLength = 0;
			request.GetRequestStream().Close();

			// Read the response to the request
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			if( response.StatusCode == HttpStatusCode.OK ) {
				Logger.Debug("Response was OK");

				byte[] buffer = new byte[response.ContentLength];
				int sizeRead = 0;
				int totalRead = 0;
				Stream stream = response.GetResponseStream();
				Logger.Debug("About to read photo of size {0}", response.ContentLength);

				try {

					do {
						sizeRead = stream.Read(buffer, totalRead, (int)(response.ContentLength - totalRead));
						totalRead += sizeRead;
						Logger.Debug("SizeRead = {0}, totalRead = {1}", sizeRead, totalRead);
					} while( (sizeRead > 0) && (totalRead < response.ContentLength) );

					Logger.Debug("We Read the photo and it's {0} bytes", totalRead);
					Logger.Debug("The content length is {0} bytes", response.ContentLength);

					stream.Close();
				} catch (Exception e) {
					Logger.Debug("Exception when reading file from stream: {0}", e.Message);
					Logger.Debug("Exception {0}", e);
				}

				serviceInfo.Photo = new Gdk.Pixbuf(buffer);

			} else {
				Logger.Debug("Unable to get the photo because {0}", response.StatusDescription);
			}
		}


	}
}
