pipeline {
    agent any

    stages {
        stage('Run Unit Tests') {
            steps {
               
                dir('NovaStay') {
                    echo '=== Running Unit Tests ==='
                
                    sh 'dotnet test --configuration Release --logger "console;verbosity=detailed"'
                }
            }
        }
        
        stage('Build and Push Image') {
            steps {
                withDockerRegistry(credentialsId: 'docker', url: 'https://index.docker.io/v1/') {
                    dir('NovaStay') {
                        echo '=== Building and Pushing Docker Image ==='
                        sh 'docker build -t ptrungduc1011/benovastay:v1 .' 
                        sh 'docker push ptrungduc1011/benovastay:v1'                     
                    }
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
