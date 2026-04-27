kubectl delete fleet --all
make build-image
minikube image load planet-hunter/simple-server/unity-simple-server:0.3
kubectl create -f fleet.yaml
POD_NAME=$(kubectl get pods -o name --no-headers=true)
sleep 10
POD_PORT=$(kubectl get pods -o json --no-headers=true | jq ".items[0].spec.containers[0].ports[0].hostPort")
kubectl port-forward "$POD_NAME" $POD_PORT:$POD_PORT

