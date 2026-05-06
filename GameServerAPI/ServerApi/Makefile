REPOSITORY = localhost:5000

mkfile_path := $(abspath $(lastword $(MAKEFILE_LIST)))
project_path := $(dir $(mkfile_path))
version := 0.3
server_tag = $(REPOSITORY)/planet-hunter-api:$(version)

# build docker image
build-image:
	docker build $(project_path) --tag=$(server_tag)

# output the server_tag
echo-image-tag:
	@echo $(server_tag)

