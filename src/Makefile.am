
CSC = gmcs

TARGET = Giver.exe
WRAPPER = giver

if ENABLE_DEBUG
CSFLAGS =  -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
endif
if ENABLE_RELEASE
CSFLAGS =  -noconfig -codepage:utf8 -warn:4
endif

CSFILES = \
	$(srcdir)/Application.cs \
	$(srcdir)/GiverService.cs \
	$(srcdir)/Logger.cs \
	$(srcdir)/ServiceLocator.cs \
	$(srcdir)/TrayLib.cs \
	$(srcdir)/Utilities.cs 

RESOURCES = \
	-resource:$(top_srcdir)/data/images/giver-16.png \
	-resource:$(top_srcdir)/data/images/giver-22.png \
	-resource:$(top_srcdir)/data/images/giver-24.png \
	-resource:$(top_srcdir)/data/images/giver-32.png \
	-resource:$(top_srcdir)/data/images/giver-44.png 

ASSEMBLIES =  \
	-r:System \
	-r:Mono.Posix \
	-r:System.Xml \
	$(GLIB_SHARP_20_LIBS) \
	$(GNOME_SHARP_20_LIBS) \
	$(GTK_DOTNET_20_LIBS) \
	$(EVOLUTION_SHARP_LIBS) \
	$(NOTIFY_SHARP_LIBS) \
	$(AVAHI_SHARP_LIBS) 

$(TARGET): $(CSFILES) Defines.cs
	$(CSC) -out:$@ $(CSFLAGS) $^ $(ASSEMBLIES) $(RESOURCES)

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
