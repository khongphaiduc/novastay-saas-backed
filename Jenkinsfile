pipeline {
    agent any

    stages {
        stage('Run Unit Tests') {
            steps {
                echo '=== Running Unit Tests inside Docker SDK ==='
               
                sh '''
                    docker run --rm \
                        -v "$(pwd)":/src \
                        -w /src \
                        mcr.microsoft.com/dotnet/sdk:8.0 \
                        dotnet test NovaStay.UnitTests/NovaStay.UnitTests.csproj --configuration Release --logger "console;verbosity=detailed"
                '''
            }
        }
        
        stage('Build and Push Image') {
            steps {
                withDockerRegistry(credentialsId: 'docker', url: 'https://index.docker.io/v1/') {
                    echo '=== Building and Pushing Docker Image ==='
                
                    sh 'docker build -t ptrungduc1011/benovastay:v1 .' 
                    sh 'docker push ptrungduc1011/benovastay:v1'                     
                }
            }
        }

        stage('Deploy') {
            steps {
                echo '=== Deploying Application ==='
                sh '''
                    docker stop benovastay || true
                    docker rm benovastay || true
                    docker rmi ptrungduc1011/benovastay:v1 || true
                    docker run -d --name benovastay -p 8888:8080 ptrungduc1011/benovastay:v1
                '''
            }
        }
    }
}
