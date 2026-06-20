pipeline {
    agent any

    stages {
        
        
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
                    # 1. Dừng và xóa container cũ mang tên 'benovastay'
                    docker stop benovastay || true
                    docker rm benovastay || true
                    
                    # 2. Xóa image cũ để giải phóng dung lượng và cập nhật bản mới nhất
                    docker rmi ptrungduc1011/benovastay:v1 || true

                    # 3. Chạy container mới trên port 9999 của VPS
                    docker run -d --name benovastay -p 9999:80 ptrungduc1011/benovastay:v1
                '''
            }
        }
    }
}