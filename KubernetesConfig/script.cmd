kind delete cluster
kind create cluster --config ports.yaml

kubectl create namespace agones-system
kubectl apply --server-side -f agones-install.yaml
