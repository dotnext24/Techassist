## Dockerfiles:

* src/TechAssistPro.Ticketing/Dockerfile
* src/TechAssistPro.Scheduling/Dockerfile
* src/TechAssistPro.CustomerManagement/Dockerfile
* src/TechAssistPro.Gateway/Dockerfile

## Kubernetes YAMLs (under k8s/):

* k8s/namespace.yaml
* k8s/ticketing-deployment.yaml
* k8s/scheduling-deployment.yaml
* k8s/customermanagement-deployment.yaml
* k8s/gateway-deployment.yaml

These files provide a basic foundation for deploying your TechAssistPro microservices to a Kubernetes cluster.

## Important Considerations:

* Secrets Management: For production, never hardcode passwords (like for SQL Server SA_PASSWORD or RabbitMQ credentials) directly in your YAMLs. Use Kubernetes Secrets to manage sensitive information securely.
* Image Registry: Replace techassistpro/<service>:latest with your actual image names and tags from your container registry (e.g., Docker Hub, Azure Container Registry, Google Container Registry).
* Ingress: For exposing your gateway-service externally with a proper domain, you'll likely want to set up an Ingress resource (not generated here) to manage external access, TLS, and routing.
* Persistent Storage: The sqlserver-deployment.yaml includes a PersistentVolumeClaim (PVC) for SQL Server data. Ensure your Kubernetes cluster has a storage class configured to provision PersistentVolumes.
* Health Checks: Basic liveness and readiness probes are included. You may need to fine-tune these based on your application's specific health endpoints and startup times.
* Resource Limits: The CPU and memory requests/limits are examples. You should adjust these based on the actual performance characteristics and needs of your applications.