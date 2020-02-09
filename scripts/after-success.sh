#!/bin/bash
openssl aes-256-cbc -K $encrypted_3aae70881186_key -iv $encrypted_3aae70881186_iv -in deploy_key.enc -out ./deploy_key -d
eval "$(ssh-agent -s)"
chmod 600 ./deploy_key
echo -e "Host $SERVER_IP_ADDRESS\n\tStrictHostKeyChecking no\n" >> ~/.ssh/config
ssh-add ./deploy_key
# test ssh connection for: https://github.com/dwyl/learn-travis/issues/42
ssh -i ./deploy_key -t $SERVER_USER@$SERVER_IP_ADDRESS 'bash -s' < ./scripts/vps_deployment.sh