/***************************************************************************
 *  Preferences.cs
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
using System.Xml;
using System.IO;

namespace Giver
{
	// <summary>
	// Class used to store Giver preferences
	// </summary>
	public class Preferences
	{
		public const string None = "none";
		public const string Local = "local";
		public const string Gravatar = "gravatar";
		public const string Uri = "uri";
		// public static event PreferenceChangedEventHandler PreferenceChanged;
		
//	   	public const string CustomAvailableMessages = "/apps/banter/custom_available_messages";
//	   	public const string CustomBusyMessages = "/apps/banter/custom_busy_messages";
		private System.Xml.XmlDocument document;
		private string location;

		public string PhotoType
		{
			get
			{ 
				XmlNodeList list = document.GetElementsByTagName("PhotoType");
				XmlElement element = (XmlElement) list[0];
				if(element == null)
					return Preferences.None;
				else
					return element.InnerText;
			}
			
			set
			{
				XmlNodeList list = document.GetElementsByTagName("PhotoType");
				XmlElement element = (XmlElement) list[0];
				if(element == null) {
					element = document.CreateElement("PhotoType");
					document.DocumentElement.AppendChild(element);
				}
				element.InnerText = value; 
				SavePrefs();
			}
		}

		public string PhotoLocation
		{
			get
			{ 
				XmlNodeList list = document.GetElementsByTagName("PhotoLocation");
				XmlElement element = (XmlElement) list[0];
				if(element == null)
					return "";
				else
					return element.InnerText;
			}
			
			set
			{
				XmlNodeList list = document.GetElementsByTagName("PhotoLocation");
				XmlElement element = (XmlElement) list[0];
				if(element == null) {
					element = document.CreateElement("PhotoLocation");
					document.DocumentElement.AppendChild(element);
				}
				element.InnerText = value; 
				SavePrefs();
			}
		}


		public string UserName
		{
			get
			{ 
				XmlNodeList list = document.GetElementsByTagName("UserName");
				XmlElement element = (XmlElement) list[0];
				if( (element == null) || (element.InnerText.Length < 1) )
					return Environment.UserName;
				else
					return element.InnerText;
			}
			
			set
			{
				XmlNodeList list = document.GetElementsByTagName("UserName");
				XmlElement element = (XmlElement) list[0];
				if(element == null) {
					element = document.CreateElement("UserName");
					document.DocumentElement.AppendChild(element);
				}
				if(value == null)
					element.InnerText = Environment.UserName; 
				else
					element.InnerText = value; 
				SavePrefs();
			}
		}


		public string ReceiveFileLocation
		{
			get
			{ 
				XmlNodeList list = document.GetElementsByTagName("ReceiveFileLocation");
				XmlElement element = (XmlElement) list[0];
				if( (element == null) || (element.InnerText.Length < 1) )
					return System.Environment.GetFolderPath (System.Environment.SpecialFolder.Desktop);
				else
					return element.InnerText;
			}
			
			set
			{
				XmlNodeList list = document.GetElementsByTagName("ReceiveFileLocation");
				XmlElement element = (XmlElement) list[0];
				if(element == null) {
					element = document.CreateElement("ReceiveFileLocation");
					document.DocumentElement.AppendChild(element);
				}
				if(value == null)
					element.InnerText = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Desktop);
				else
					element.InnerText = value; 
				SavePrefs();
			}
		}

		public int PortNumber
		{
			get
			{ 
				XmlNodeList list = document.GetElementsByTagName("PortNumber");
				XmlElement element = (XmlElement) list[0];
				if( (element == null) || (element.InnerText.Length < 1) )
					// any port
					return -1;
				int port_num;
				if (Int32.TryParse (element.InnerText, out port_num))
					return port_num;
				else
					return -1;
			}

			set
			{
				XmlNodeList list = document.GetElementsByTagName("PortNumber");
				XmlElement element = (XmlElement) list[0];
				if(element == null) {
					element = document.CreateElement("PortNumber");
					document.DocumentElement.AppendChild(element);
				}
				element.InnerText = value.ToString ();
				SavePrefs();
			}
		}

		public Preferences ()
		{
			document = new XmlDocument();
			location = Path.Combine(Environment.GetFolderPath(
			Environment.SpecialFolder.ApplicationData), "giver/preferences");
			if(!File.Exists(location)) {
				CreateDefaultPrefs();
			} else {
				document.Load(location);
			}
		}


		private void SavePrefs()
		{
			XmlTextWriter writer = new XmlTextWriter(location, System.Text.Encoding.UTF8);
			writer.Formatting = Formatting.Indented;
			document.WriteTo( writer );
			writer.Flush();
			writer.Close();
		}


		private void CreateDefaultPrefs()
		{
			try {
				Directory.CreateDirectory(Path.GetDirectoryName(location));

       			document.LoadXml("<giverprefs>" +
                   				"  <PhotoType>none</PhotoType>" +
                   				"  <PhotoLocation></PhotoLocation>" +
								"  <UserName></UserName>" +
								"  <ReceiveFileLocation></ReceiveFileLocation>" +
								"  <PortNumber></PortNumber>" +
                  			 "</giverprefs>");
				SavePrefs();
/* 
		       // Create a new element node.
		       XmlNode newElem = doc.CreateNode("element", "pages", "");  
		       newElem.InnerText = "290";
		     
		       Console.WriteLine("Add the new element to the document...");
		       XmlElement root = doc.DocumentElement;
		       root.AppendChild(newElem);
		     
		       Console.WriteLine("Display the modified XML document...");
		       Console.WriteLine(doc.OuterXml);
*/
			} catch (Exception e) {
				Logger.Debug("Exception thrown in Preferences {0}", e);
				return;
			}

		}
	}
}
