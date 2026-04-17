kubectl delete gs --all
make build-image
kind load docker-image planet-hunter/simple-server/unity-simple-server:0.2
kubectl create -f gameserver.yaml

