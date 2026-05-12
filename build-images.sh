#!/bin/bash

##############################
# build-images.sh
##############################
# Script to be used in root directory of local repository
# /PlanetHunter/Build/Server and /ServerApi need to be present
#
# This script runs the makefile in /PlanetHunter and /ServerApi to build
# and push a container image
#
# This could be done automatically using a GitHub Action if you had a Unity
# licence to build the server on GitHub or used Git LFS to push a server binary
# to the repo
##############################
# Arguments
##############################
# --server-version - the version for the server image
# --api-version - the version for the api image
##############################

show_help() {
  echo "Usage: $0 [--server-version] [0.1] [--api-version] [0.2]"
  echo "       Use at root of repository (directory this script is in)"
  exit 1
}

if [[ $(basename $(pwd)) != "FinalYearProject" ]]; then
    echo "Error: Please move to root of repository (directory this script is in)"
fi

if [[ $# -eq 0 ]]; then
    show_help
fi

# arguments
while [[ $# -gt 0 ]]; do
  case "$1" in
    --server-version)
      shift
      if [[ "$1" == "" || "$1" == "--api-version" || "$1" == "--server-version" ]]; then 
        show_help
      fi
      export SERVER_VERSION="$1"
      shift
      ;;
    --api-version)
      shift
      if [[ "$1" == "" || "$1" == "--api-version" || "$1" == "--server-version" ]]; then 
        show_help
      fi
      export API_VERSION="$1"
      shift
      ;;
    *)
      show_help
      ;;
  esac
done

if [[ -n "$SERVER_VERSION" ]]; then
  cd PlanetHunter
  echo "Building Server..."
  make build-image-script
  cd ..
fi

if [[ -n "$API_VERSION" ]]; then
  cd ServerApi
  echo "Building API..."
  make build-image-script
  cd ..
fi

