# Works with bash and powershell
PROJDIR:=src/Ara
ASMPDIR:=src/Asm
DIAGDIR:=src/Diagnostics

NETVER:=net6.0
SYSTEM:=win-x64
SLN:=Ara.sln
CP=cp
RM=rm

all: build

build: debugbuild debugcopy resources

debugbuild:
	dotnet build $(SLN) -t:rebuild

debugcopy:
	$(CP) $(PROJDIR)/bin/Debug/$(NETVER)/Ara.dll Ara.dll
	$(CP) $(ASMPDIR)/bin/Debug/$(NETVER)/Asm.dll Asm.dll
	$(CP) $(DIAGDIR)/bin/Debug/$(NETVER)/Diagnostics.dll Diagnostics.dll
	-$(CP) $(PROJDIR)/bin/Debug/$(NETVER)/Ara.exe ara.exe
	-$(CP) $(PROJDIR)/bin/Debug/$(NETVER)/Ara ara.exe

setup:
	$(CP) $(PROJDIR)/bin/Debug/$(NETVER)/Ara.deps.json Ara.deps.json
	$(CP) $(PROJDIR)/bin/Debug/$(NETVER)/Ara.runtimeconfig.json Ara.runtimeconfig.json

.PHONY: resources
resources:
	$(RM) -f -r Resources
	mkdir Resources
	-$(CP) -a $(PROJDIR)/Resources/. Resources
	-$(CP) -a $(ASMPDIR)/Resources/. Resources

release: releasebuild resources

releasebuild:
	dotnet publish $(PROJDIR)/Ara.csproj -r $(SYSTEM) -p:PublishSingleFile=true --self-contained true \
		-p:PublishReadyToRunShowWarnings=true -p:IncludeNativeLibrariesForSelfExtract=true --configuration Release
	$(CP) $(PROJDIR)/bin/Release/$(NETVER)/$(SYSTEM)/publish/Ara.exe ara.exe

clean:
	$(RM) *.dll
	$(RM) *.exe
	$(RM) *.json
