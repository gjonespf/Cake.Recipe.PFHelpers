#!/usr/bin/groovy
def executeXplat(commandString) {
    if (isUnix()) {
        sh commandString
    } else {
        bat commandString
    }
}

pipeline {
    agent { label 'win-cake' } 
    environment { 
        GITCREDENTIALSID = 'f2fd441c-118c-40e8-bc6a-bd8b9826c095'
        LOCALOCTOCREDID = 'e162c9e0-0792-472d-a01e-51f2b7427f2b'
        CLOUDOCTOCREDID = '74207640-6946-44f4-8175-171e9d807193'
        DEFAULTOCTOCREDID = 'e162c9e0-0792-472d-a01e-51f2b7427f2b'
        NUGETCREDID = '74d7f630-d422-4324-89f2-6ebccc3b3687'
        CHOCOCREDID = '74a39230-d94d-4660-b686-daf40f89e462'
    }

    stages {
        stage('Pre') {
            steps {
                // TODO: Pull vars from properties.json
                // script{ 
                //     def props = readJSON file: 'properties.json'
                //     for (int i = 0; i < props.size(); i++) {
                //         echo "json ${props[i].key} = ${props[i].value}"
                //     }
                // }

                echo "Build setup:"
                echo "============"
                echo "Proxy: ${env.HTTP_PROXY}"
                echo "Git Cred ID: ${env.GITCREDENTIALSID}"
                
                withCredentials([
                    usernamePassword(credentialsId: "${env.GITCREDENTIALSID}", passwordVariable: 'GITKEY', usernameVariable: 'GITUSER')
                ])
                {
                    echo 'Preparing...'
                    executeXplat "pwsh -NonInteractive -NoProfile -ExecutionPolicy Bypass ./pre.ps1 "
                }
            }
        }
        stage('Init') {
            steps {
                echo 'Initializing...'
                executeXplat "pwsh -NonInteractive -NoProfile -ExecutionPolicy Bypass ./build.ps1 -Target \"Init\" "
            }
        }
        stage('Build') {
            steps {
                echo "Running #${env.BUILD_ID} on ${env.JENKINS_URL}"
                echo 'Building...'
                executeXplat "pwsh -NonInteractive -NoProfile -ExecutionPolicy Bypass ./build.ps1 -Target \"Build\" "
            }
        }
        stage('Package') {
            steps {
                echo 'Packaging...'
                executeXplat "pwsh -NonInteractive -NoProfile -ExecutionPolicy Bypass ./build.ps1 -Target \"Package\" "
            }
        }
        stage('Test'){
            steps {
                echo 'Testing...'
                executeXplat "pwsh -NonInteractive -NoProfile -ExecutionPolicy Bypass ./build.ps1 -Target \"Test\" "
            }
        }
        stage('Publish') {
            steps {
                withCredentials([
                    usernamePassword(credentialsId: "${env.GITCREDENTIALSID}", passwordVariable: 'GITKEY', usernameVariable: 'GITUSER'),
                    usernamePassword(credentialsId: "${env.LOCALOCTOCREDID}", passwordVariable: 'OCTOAPIKEY', usernameVariable: 'OCTOSERVER'),
                    usernamePassword(credentialsId: "${env.CLOUDOCTOCREDID}", passwordVariable: 'OCTOCLOUDAPIKEY', usernameVariable: 'OCTOCLOUDSERVER'),
                    usernamePassword(credentialsId: "${env.DEFAULTOCTOCREDID}", passwordVariable: 'OCTOPUS_CLI_API_KEY', usernameVariable: 'OCTOPUS_CLI_SERVER'),
                    usernamePassword(credentialsId: "${env.NUGETCREDID}", passwordVariable: 'LocalNugetApiKey', usernameVariable: 'LocalNugetServerUrl'),
                    usernamePassword(credentialsId: "${env.CHOCOCREDID}", passwordVariable: 'LocalChocolateyApiKey', usernameVariable: 'LocalChocolateyServerUrl')
                    ]) 
                {
                    echo 'Publishing...'
                    executeXplat "pwsh -NonInteractive -NoProfile -ExecutionPolicy Bypass ./build.ps1 -Target \"Publish\" "
                }
            }
        }
        // TODO: Update release deets?
    }

    post {
        success {
            archiveArtifacts allowEmptyArchive: true, artifacts: 'BuildArtifacts/**', fingerprint: true
        }
    }
}

