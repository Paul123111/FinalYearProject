REPOSITORY = .dkr.ecr.eu-west-1.amazonaws.com

mkfile_path := $(abspath $(lastword $(MAKEFILE_LIST)))
project_path := $(dir $(mkfile_path))

name := planet-hunter-server
version := 0.5

server_tag = $(REPOSITORY)/$(name):$(version)

# build docker image
build-image:
	docker build $(project_path) --tag=$(server_tag)

build-image-script:
	docker build $(project_path) --tag=$(AWS_ACCOUNT_ID)$(REPOSITORY)/$(name):$(SERVER_VERSION)
	docker push $(AWS_ACCOUNT_ID)$(REPOSITORY)/$(name):$(SERVER_VERSION)

# output the server_tag
echo-image-tag:
	@echo $(server_tag)

