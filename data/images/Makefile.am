
pixmapdir = $(datadir)/pixmaps
pixmap_DATA = 	giver-16.png \
				giver-24.png \
				giveroff-24.png \
				giver-32.png \
				giver-48.png \
				giver-256.png \
				gravatar.png \
				blankphoto.png 

hicolordir = $(datadir)/icons/hicolor

tango_icons = 				\
				giver-16.png \
				giver-24.png \
				giveroff-24.png \
				giver-32.png \
				giver-48.png \
				giver-256.png \
				gravatar.png \
				blankphoto.png 

install-data-local:
	@-$(mkinstalldirs) $(DESTDIR)$(hicolordir)/16x16/apps $(DESTDIR)$(hicolordir)/24x24/apps $(DESTDIR)$(hicolordir)/32x32/apps $(DESTDIR)$(hicolordir)/48x48/apps $(DESTDIR)$(hicolordir)/256x256/apps
	$(INSTALL_DATA) $(srcdir)/giver-16.png $(DESTDIR)$(hicolordir)/16x16/apps/giver.png
	$(INSTALL_DATA) $(srcdir)/giver-24.png $(DESTDIR)$(hicolordir)/24x24/apps/giver.png
	$(INSTALL_DATA) $(srcdir)/giveroff-24.png $(DESTDIR)$(hicolordir)/24x24/apps/giveroff.png
	$(INSTALL_DATA) $(srcdir)/giver-32.png $(DESTDIR)$(hicolordir)/32x32/apps/giver.png
	$(INSTALL_DATA) $(srcdir)/giver-48.png $(DESTDIR)$(hicolordir)/48x48/apps/giver.png
	$(INSTALL_DATA) $(srcdir)/blankphoto.png $(DESTDIR)$(hicolordir)/48x48/apps/blankphoto.png
	$(INSTALL_DATA) $(srcdir)/gravatar.png $(DESTDIR)$(hicolordir)/48x48/apps/gravatar.png
	$(INSTALL_DATA) $(srcdir)/giver-256.png $(DESTDIR)$(hicolordir)/256x256/apps/giver.png

gtk_update_icon_cache = gtk-update-icon-cache -f -t $(datadir)/icons/hicolor

install-data-hook:
	@-if test -z "$(DESTDIR)"; then \
		echo "Updating Gtk icon cache."; \
		$(gtk_update_icon_cache); \
	else \
		echo "*** Icon cache not updated.  After install, run this:"; \
		echo "***   $(gtk_update_icon_cache)"; \
	fi

uninstall-hook:
	rm -f $(DESTDIR)$(hicolordir)/16x16/apps/giver.png
	rm -f $(DESTDIR)$(hicolordir)/24x24/apps/giver.png
	rm -f $(DESTDIR)$(hicolordir)/24x24/apps/giveroff.png
	rm -f $(DESTDIR)$(hicolordir)/32x32/apps/giver.png
	rm -f $(DESTDIR)$(hicolordir)/48x48/apps/giver.png
	rm -f $(DESTDIR)$(hicolordir)/48x48/apps/gravatar.png
	rm -f $(DESTDIR)$(hicolordir)/48x48/apps/blankphoto.png
	rm -f $(DESTDIR)$(hicolordir)/256x256/apps/giver.png

noinst_DATA =

EXTRA_DIST = $(noinst_DATA) $(pixmap_DATA) $(tango_icons)
