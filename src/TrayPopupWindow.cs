
using System;
using Gdk;
using Gtk;

namespace Giver
{
	///<summary>
	///	TrayPopupWindow
	/// Window holding all drop targets for giver
	///</summary>	
	public class TrayPopupWindow : Gtk.Window
	{
		
		private Label titleLabel;
		public TrayPopupWindow() : base(Gtk.WindowType.Popup)
		{
			//Stetic.Gui.Build(this, typeof(giver.TrayPopupWindow));
			
			VBox test = new VBox();
			titleLabel = new Label("User X has sent Y of Z files");
			test.Add (titleLabel);
			test.ShowAll();
			this.Add(test);
			
			
		}
	}
}
