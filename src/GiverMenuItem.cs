//***********************************************************************
// *  GiverMenuItem.cs
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
using Gtk;

namespace Giver
{
	/// <summary>
	/// Used to show Giver targets in the target menu
	/// </summary>
	public class GiverMenuItem : ComplexMenuItem
	{   
	    public GiverMenuItem(Service service) : base(false)
	    {
	        HBox hbox = new HBox(false, 10);
			Gtk.Image image = new Gtk.Image(Utilities.GetIcon("giver-44", 44));
			hbox.PackStart(image, false, false, 0);
			VBox vbox = new VBox();
			hbox.PackStart(vbox, false, false, 0);
			Label label = new Label();
			label.Justify = Gtk.Justification.Left;
            label.SetAlignment (0.0f, 0.5f);
			label.LineWrap = false;
			label.UseMarkup = true;
			label.UseUnderline = false;
			label.Markup = string.Format ("<span weight=\"bold\" size=\"large\">{0}</span>",
                    						service.UserName);
			vbox.PackStart(label, true, true, 0);

			label = new Label();
			label.Justify = Gtk.Justification.Left;
            label.SetAlignment (0.0f, 0.5f);
			label.LineWrap = false;
			label.UseMarkup = true;
			label.UseUnderline = false;
			label.Markup = string.Format ("<span size=\"small\">{0}</span>",
                    						service.MachineName);

			vbox.PackStart(label, true, true, 0);

			label = new Label();
			label.Justify = Gtk.Justification.Left;
            label.SetAlignment (0.0f, 0.5f);
			label.LineWrap = false;
			label.UseMarkup = true;
			label.UseUnderline = false;
			label.Markup = string.Format ("<span style=\"italic\" size=\"small\">{0}:{1}</span>",
                    						service.Address, service.Port);

			vbox.PackStart(label, true, true, 0);
	        hbox.ShowAll();
	        Add(hbox);
	    }
	}


}
