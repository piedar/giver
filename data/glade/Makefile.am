GLADE_FILES = \
	$(srcdir)/giver-prefs.glade \
	$(srcdir)/gravatar.gif

GLADEDIR = $(DESTDIR)$(datadir)/giver/glade

install-exec-local: all
	$(mkinstalldirs) $(GLADEDIR)
	$(INSTALL_PROGRAM) $(GLADE_FILES) $(GLADEDIR)

uninstall-local:
	rm -rf $(GLADEDIR)

EXTRA_DIST = \
	$(GLADE_FILES) 

