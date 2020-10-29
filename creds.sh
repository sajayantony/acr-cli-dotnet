#!/bin/sh

export REGISTRY=$(az configure -l --query "[?name == 'acr'].value" -o tsv)

echo "Loading Credentials for registry:" $REGISTRY
export REGISTRY_LOGIN=$(az acr show --query loginServer -o tsv)
export REGISTRY_USERNAME=$(az acr credential show --query 'username' -o tsv)
export REGISTRY_PASSWORD=$(az acr credential show --query 'passwords[0].value' -o tsv)


