pipeline {
    agent any

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Debug Project Structure') {
            steps {
                sh '''
                    echo "=== WORKSPACE ==="
                    echo "$WORKSPACE"

                    echo "=== LIST ROOT ==="
                    ls -la "$WORKSPACE"

                    echo "=== FIND SOLUTION ==="
                    find "$WORKSPACE" -name "*.sln"

                    echo "=== FIND CSPROJ ==="
                    find "$WORKSPACE" -name "*.csproj"
                '''
            }
        }

        stage('Run Unit Tests') {
            steps {
                echo '=== Running Unit Tests inside Docker SDK ==='
                sh '''
                    docker run --rm \
                        -v "$WORKSPACE:/src" \
                        -w /src/NovaStay \
                        mcr.microsoft.com/dotnet/sdk:8.0 \
                        dotnet test NovaStay.sln --configuration Release --logger "console;verbosity=detailed"
                '''
            }
        }

        stage('Build and Push Image') {
            steps {
                withDockerRegistry(credentialsId: 'docker', url: 'https://index.docker.io/v1/') {
                    echo '=== Building and Pushing Docker Image ==='
                    sh '''
                        docker build -t ptrungduc1011/benovastay:v1 "$WORKSPACE/NovaStay"
                        docker push ptrungduc1011/benovastay:v1
                    '''
                }
            }
        }

        stage('Deploy') {
            steps {
                echo '=== Deploying Application ==='
                sh '''
                    docker stop benovastay || true
                    docker rm benovastay || true

                    docker pull ptrungduc1011/benovastay:v1

                    docker run -d \
                        --name benovastay \
                        --restart unless-stopped \
                        -p 8888:8080 \
                        ptrungduc1011/benovastay:v1
                '''
            }
        }
    }
}
