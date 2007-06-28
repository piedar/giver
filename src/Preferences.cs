//***********************************************************************
// *  $RCSfile$ - Preferences.cs
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
					return "none";
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
