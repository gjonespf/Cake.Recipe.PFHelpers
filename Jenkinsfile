#!/usr/bin/groovy
def executeXplat(commandString) {
    if (isUnix()) {
        sh commandString
    } else {
        bat commandString
    }
}

pipeline {
    agent { label 'xplat-cake' } 
    environment { 
        CC = 'clang'
    }

    stages {
        stage('Pre') {
            steps
            {
                echo 'Preparing...'
                script {
                    if (isUnix()) {
                        echo 'Running on Unix...'
                        sh "pwsh ./pre.ps1 -t \"Init\"" 
                    } else  {
                        echo 'Running on Windows...'
                        bat "powershell -ExecutionPolicy Bypass -Command \"& './pre.ps1' -Target \"Init\"\""
                    }
                }
            }
        }
        stage('Init') {
            steps
            {
                echo 'Initializing...'
                script {
                    if (isUnix()) {
                        echo 'Running on Unix...'
                        sh "./build.sh -t \"Init\"" 
                    } else  {
                        echo 'Running on Windows...'
                        bat "powershell -ExecutionPolicy Bypass -Command \"& './build.ps1' -Target \"Init\"\""
                    }
                }
            }
        }
        stage('Build') {
            steps {
                echo "Running #${env.BUILD_ID} on ${env.JENKINS_URL}"
                echo 'Building...'
                script {
                    if (isUnix()) {
                        sh "./build.sh -t \"Build\"" 
                    } else  {
                        bat "powershell -ExecutionPolicy Bypass -Command \"& './build.ps1' -Target \"Build\"\""
                    }
                }
            }
        }
        stage('Package') {
            steps {
                echo 'Packaging...'
                script {
                    if (isUnix()) {
                        sh "./build.sh -t \"Package\"" 
                    } else  {
                        bat "powershell -ExecutionPolicy Bypass -Command \"& './build.ps1' -Target \"Package\"\""
                    }
                }
            }
        }
        stage('Test'){
            steps {
                echo 'Testing...'
                script {
                    if (isUnix()) {
                        sh "./build.sh -t \"Test\"" 
                    } else  {
                        bat "powershell -ExecutionPolicy Bypass -Command \"& './build.ps1' -Target \"Test\"\""
                    }
                }
            }
        }
        stage('Publish') {
            steps {
                withCredentials([
                    usernamePassword(credentialsId: 'e162c9e0-0792-472d-a01e-51f2b7427f2b', passwordVariable: 'OCTOAPIKEY', usernameVariable: 'OCTOSERVER'),
                    usernamePassword(credentialsId: '74207640-6946-44f4-8175-171e9d807193', passwordVariable: 'OCTOCLOUDAPIKEY', usernameVariable: 'OCTOCLOUDSERVER'),
                    usernamePassword(credentialsId: '74d7f630-d422-4324-89f2-6ebccc3b3687', passwordVariable: 'LocalNugetApiKey', usernameVariable: 'LocalNugetServerUrl'),
                    usernamePassword(credentialsId: '74a39230-d94d-4660-b686-daf40f89e462', passwordVariable: 'LocalChocolateyApiKey', usernameVariable: 'LocalChocolateyServerUrl')
                    ]) 
                {
                    echo 'Publishing...'
                    script {
                        if (isUnix()) {
                            sh "./build.sh -t \"Publish\"" 
                        } else  {
                            bat "powershell -ExecutionPolicy Bypass -Command \"& './build.ps1' -Target \"Publish\"\""
                        }
                    }
                }
            }
        }
    }

    post {
        always {
            archiveArtifacts allowEmptyArchive: true, artifacts: 'BuildArtifacts/**', fingerprint: true
        }
    }
}