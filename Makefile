
.PHONY: all build docs clean distclean

all: build docs

build:
	dotnet build

clean:
	dotnet clean

distclean:
	rm -rf obj
	rm -rf bin
	rm -rf user_manual

docs:
	doxygen doxygen_user_manual.conf
	cp -ru images user_manual/html/


