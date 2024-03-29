PROJECT=./IPK-Proj1/IPK-Proj1.csproj
PROJECT_DIR=./IPK-Proj1
TESTS_DIR=./IPK-Proj1-Tests
OUTPUT_BIN=ipk24chat-client

all: build

build:
	dotnet publish -c Release -r linux-x64 --self-contained true -o ./ -p:PublishSingleFile=true -p:AssemblyName=$(OUTPUT_BIN) $(PROJECT)

clean:
	rm -f $(OUTPUT_BIN)
	rm -rf $(PROJECT_DIR)/obj
	rm -rf $(PROJECT_DIR)/bin
	rm -rf $(PROJECT_DIR)/packages
	rm -rf $(TESTS_DIR)/obj
	rm -rf $(TESTS_DIR)/bin

.PHONY: all build clean
