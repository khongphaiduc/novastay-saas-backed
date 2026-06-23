pipeline {
    agent any

    stages {
        stage('Run Unit Tests') {
            steps {
                dir('NovaStay') {
                    sh '''
                        docker run --rm \
                          -v "$PWD":/src \
                          -w /src \
                          mcr.microsoft.com/dotnet/sdk:8.0 \
                          dotnet test NovaStay.UnitTests/NovaStay.UnitTests.csproj --configuration Release
                    '''
                }
            }
        }

        stage('Build and Push Image') {
            steps {
                withDockerRegistry(credentialsId: 'docker', url: 'https://index.docker.io/v1/') {
                    dir('NovaStay') {
                        sh 'docker build -t ptrungduc1011/benovastay:v1 .'
                        sh 'docker push ptrungduc1011/benovastay:v1'
                    }
                }
            }
        }

        stage('Deploy') {
            steps {
                sh '''
                    docker stop benovastay || true
                    docker rm benovastay || true
                    docker rmi ptrungduc1011/benovastay:v1 || true

                    docker run -d \
                      --name benovastay \
                      -p 8888:8080 \
                      --env-file /opt/novastay/.env \
                      --restart unless-stopped \
                      ptrungduc1011/benovastay:v1
                '''
            }
        }
    }
}