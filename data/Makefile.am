
SUBDIRS = images sounds

@INTLTOOL_DESKTOP_RULE@

desktop_in_files = giver.desktop.in
desktop_files    = $(desktop_in_files:.desktop.in=.desktop)

desktopdir   = $(datadir)/applications
desktop_DATA = $(desktop_files)

noinst_DATA = 

EXTRA_DIST = 					\
	$(desktop_in_files)     		\
	$(noinst_DATA)

DISTCLEANFILES = 				\
	$(desktop_files)   
