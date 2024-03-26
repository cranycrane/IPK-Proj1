PROJECT=IPK-Proj1.csproj
OUTPUT_BIN=ipk24chat-client

# Default target for building the project
all: build

# Target to build the ipk24chat-client executable
build:
	dotnet publish -c Release -r linux-x64 --self-contained true -o ./ -p:PublishSingleFile=true -p:AssemblyName=$(OUTPUT_BIN) $(PROJECT)

# Clean up the build artifacts
clean:
	rm -f $(OUTPUT_BIN)
	dotnet clean

.PHONY: all build clean
