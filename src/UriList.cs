/***************************************************************************
 *  UriList.cs
 *
 *  Copyright (C) 2007 Novell, Inc.
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
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Gtk;

namespace Giver
{	
	public class UriList : ArrayList
	{
		private void LoadFromString (string data) {
			//string [] items = System.Text.RegularExpressions.Regex.Split ("\n", data);
			string [] items = data.Split ('\n');
			
			foreach (String i in items) {
				if (!i.StartsWith ("#")) {
					Uri uri;
					String s = i;

					if (i.EndsWith ("\r")) {
						s = i.Substring (0, i.Length - 1);
	//					Debug.PrintLine ("uri = {0}", s);
					}
					
					try {
						uri = new Uri (s);
					} catch {
						continue;
					}
					Add (uri);
				}
			}
		}

		static char[] CharsToQuote = { ';', '?', ':', '@', '&', '=', '$', ',', '#' };

		public static Uri PathToFileUri (string path)
		{
			path = Path.GetFullPath (path);

			StringBuilder builder = new StringBuilder ();
			builder.Append (Uri.UriSchemeFile);
			builder.Append (Uri.SchemeDelimiter);

			int i;
			while ((i = path.IndexOfAny (CharsToQuote)) != -1) {
				if (i > 0)
					builder.Append (path.Substring (0, i));
				builder.Append (Uri.HexEscape (path [i]));
				path = path.Substring (i+1);
			}
			builder.Append (path);

			return new Uri (builder.ToString (), true);
		}

		public UriList (string [] uris)
		{	
			// FIXME this is so lame do real chacking at some point
			foreach (string str in uris) {
				Uri uri;

				if (File.Exists (str) || Directory.Exists (str))
					uri = PathToFileUri (str);
				else 
					uri = new Uri (str);
				
				Add (uri);
			}
		}

		public UriList (string data) {
			LoadFromString (data);
		}
		
		public UriList (Gtk.SelectionData selection) 
		{
			// FIXME this should check the atom etc.
			LoadFromString (System.Text.Encoding.UTF8.GetString (selection.Data));
		}

		public override string ToString () {
			StringBuilder list = new StringBuilder ();

			foreach (Uri uri in this) {
				if (uri == null)
					break;

				list.Append (uri.ToString () + "\r\n");
			}

			return list.ToString ();
		}

		public string [] ToLocalPaths () {
			int count = 0;
			foreach (Uri uri in this) {
				if (uri.IsFile)
					count++;
			}
			
			String [] paths = new String [count];
			count = 0;
			foreach (Uri uri in this) {
				if (uri.IsFile)
					paths[count++] = uri.LocalPath;
			}
			return paths;
		}
	}
}
