kubectl delete gs --all
make build-image
kind load docker-image planet-hunter/simple-server/unity-simple-server:0.2
kubectl create -f gameserver.yaml
POD_NAME=$(kubectl get pods -o name --no-headers=true)
sleep 10
kubectl port-forward "$POD_NAME"
kubectl port-forward "$POD_NAME" 7000:7000

