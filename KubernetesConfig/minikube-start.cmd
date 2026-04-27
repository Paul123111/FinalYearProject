minikube delete
minikube start --ports 7000-7100:7000-7100/udp

kubectl create namespace agones-system
kubectl apply --server-side -f agones-install.yaml
