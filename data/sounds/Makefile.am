SOUND_FILES = \
	$(srcdir)/notify.wav 

SOUNDSDIR = $(DESTDIR)$(datadir)/giver/sounds

install-exec-local: all
	$(mkinstalldirs) $(SOUNDSDIR)
	$(INSTALL_PROGRAM) $(SOUND_FILES) $(SOUNDSDIR)

uninstall-local:
	rm -rf $(SOUNDSDIR)

EXTRA_DIST = \
	$(SOUND_FILES) 

