
CSC = gmcs

TARGET = Giver.exe
WRAPPER = giver

CSFLAGS=

if ENABLE_DEBUG
CSFLAGS +=  -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
endif
if ENABLE_RELEASE
CSFLAGS +=  -noconfig -codepage:utf8 -warn:4
endif

CSFILES = \
	$(srcdir)/Application.cs \
	$(srcdir)/ComplexMenuItem.cs \
	$(srcdir)/GiverMenuItem.cs \
	$(srcdir)/GiverService.cs \
	$(srcdir)/IDesktopApplication.cs \
	$(srcdir)/Logger.cs \
	$(srcdir)/PhotoService.cs \
	$(srcdir)/PlatformService.cs \
	$(srcdir)/Preferences.cs \
	$(srcdir)/PreferencesDialog.cs \
	$(srcdir)/Protocol.cs \
	$(srcdir)/RequestHandler.cs \
	$(srcdir)/SendingHandler.cs \
	$(srcdir)/Services.cs \
	$(srcdir)/ServiceInfo.cs \
	$(srcdir)/ServiceLocator.cs \
	$(srcdir)/TargetService.cs \
	$(srcdir)/TargetWindow.cs \
	$(srcdir)/TransferStatusArgs.cs \
	$(srcdir)/TrayLib.cs \
	$(srcdir)/TrayPopupWindow.cs \
	$(srcdir)/UriList.cs \
	$(srcdir)/Utilities.cs

RESOURCES = \
	-resource:$(top_srcdir)/data/images/giver-16.png \
	-resource:$(top_srcdir)/data/images/giver-24.png \
	-resource:$(top_srcdir)/data/images/giveroff-24.png \
	-resource:$(top_srcdir)/data/images/giver-32.png \
	-resource:$(top_srcdir)/data/images/giver-48.png \
	-resource:$(top_srcdir)/data/images/gravatar.png \
	-resource:$(top_srcdir)/data/images/blankphoto.png \
	-resource:$(top_srcdir)/data/images/giver-256.png 

ASSEMBLIES =  \
	-r:System \
	-r:System.Xml \
	$(GLIB_SHARP_20_LIBS) \
	$(GTK_DOTNET_20_LIBS) \
	$(NOTIFY_SHARP_LIBS) \
	$(MONO_ZEROCONF_LIBS)

if ENABLE_WIN
CSFILES += $(srcdir)/GtkApplication.cs \
	$(srcdir)/WindowsPlatformService.cs

CSFLAGS += -t:winexe -d:WIN32
else
CSFILES += $(srcdir)/GnomeApplication.cs \
	$(srcdir)/GnomePlatformService.cs

ASSEMBLIES += -r:Mono.Posix $(GNOME_SHARP_20_LIBS)
endif

$(TARGET): $(CSFILES) Defines.cs
	$(CSC) -unsafe -out:$@ $(CSFLAGS) $^ $(ASSEMBLIES) $(RESOURCES)

banterlibdir = $(pkglibdir)
banterlib_DATA = $(TARGET)	

bin_SCRIPTS = $(WRAPPER)

$(WRAPPER): $(srcdir)/$(WRAPPER).in Makefile
	sed -e "s|\@prefix\@|$(prefix)|g"               \
		-e "s|\@exec_prefix\@|$(bindir)|g"			\
		-e "s|\@libdir\@|$(libdir)|g"				\
		-e "s|\@pkglibdir\@|$(pkglibdir)|g"         \
		-e "s|\@bindir\@|$(bindir)|g"               \
		-e "s|\@target\@|$(TARGET)|g"               \
		-e "s|\@wrapper\@|$(WRAPPER)|g"             \
		-e "s|\@srcdir\@|$(PWD)|g"  \
		< $< > $@
	chmod +x $(WRAPPER)

Defines.cs: $(srcdir)/Defines.cs.in Makefile
	sed -e "s|\@version\@|$(VERSION)|"     \
		-e "s|\@datadir\@|$(datadir)|"     \
		-e "s|\@pkglibdir\@|$(pkglibdir)|" \
		< $< > $@

EXTRA_DIST = \
	$(CSFILES)					\
	$(WRAPPER).in				\
	$(srcdir)/Defines.cs.in		

CLEANFILES = \
	$(TARGET)					\
	$(TARGET).mdb				\
	$(WRAPPER)					\
	Defines.cs

DISTCLEANFILES =                        \
        $(WRAPPER)			\
		$(TARGET)			\
		$(TARGET).mdb		\
		Defines.cs			
