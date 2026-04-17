podman machine reset
podman machine init
podman machine start

kind delete cluster
kind create cluster

kubectl create namespace agones-system
kubectl apply --server-side -f agones-install.yaml
sleep 30
kubectl create -f sample.yaml