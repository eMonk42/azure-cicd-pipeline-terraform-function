# C#

## Azure Function Logic

Install VisualStudo (current version: 2019) and dotnet. Verify, that the installation is valid. If everything worked correctly there should be a bunch of different Project-templates visible for you when creating a new project. Choose the Azure Function App - template and define http as the trigger.
You are given a complete Function all ready to go that returns just a string or so. As far as the packages goes you'll need the Microsoft.NET.sdk.Functions one at least for this task. Because we only have a POST-request trigger you can delete the 'get' from the template. Now you can write the logic that handels a request with a json-body and adds integers given as 'addends': [x, y, z].
You can test the Function locally by hitting the run-button. It will run on localhost and you will be able to interact with it via Postman, Insimnia or any other Software that can send a POST-request to any source.

## Unit Tests

I choose nunit for testing. Rightclick on your main solution and choose 'add'. Then in the progeress create a new project and this time navigate to test and choose a nunit-template. I suggest also using the FluentAssertion-package since it gives you more readability when checking if an error is thrown or if some variable is empty for example.
When it comes to naming your tests it is good style to write them as a sentence. Don't shy away from using snake-case since it provides even more readability. You are more than anything communicating to other human beings here so this is actually a key concept of writing solid tests.
A good talk about test-naming: https://www.youtube.com/watch?v=tWn8RA_DEic

# Pipeline

## Repo, Trigger, Structure

Go to the Azure DevOps front-end and if not already done create a repo there where you put your C#-solution with the Azure_function-logic and the Tests in it. Now you are ready to create the CI/CD-Pipeline. By creating you can choose between configuring everything through the frontand or by creating YAML file. Choose the second one. You'll still be able to do a lot of the configuration through the GUI since all it does is writing to a .yml-file too. When you have the .yml-file ready you can name the trigger after a branch in your repo. So whenever you push this branch the pipeline will be started. Make yourself familiar with the YAML-syntax and just try to add more stages, jobs and steps. For the task you are given you will have to consider these steps:
stage 1: build and test your C#-code. After success you'll need do save the build as an artifact for the next phase of the pipeline.
stage 2: Get the artifact, run terraform to initiate the infrastructure and finally deploy your build to it.
I'll talk about it in more detail in the YAML-section
noteworthy:

- jobs clean up after they are done. So when you for example download the artifact and then start a new job the downloaded files will be gone
- jobs also have no sequential order. They are not even necessarily performed on the same machine. So apply dependsOn: <previousjobName> to basically everything
- tasks don't have to be always named that way. You'll come across scripts and many other presets like that.

## Azure Portal - the Blob

This is a step you can do later on or now, based on when you are starting to add terraform to the project. The idea here is that you store the .tfstate-file in a blob-storage on Azure so terraform will always have the current state available no matter if you run it locally or in the pipeline. Therefore you have to create a new storage-container but I suggest not using the same resource group you are using for the Azure-Function since that one should be only maintained by terraform itself. For details of how to configure the job for terraform in the .yml to use this container please look into the stage 2 - part of the YAML-section of this file.

# terraform

## resources

That part is easy, since terraform is excellently documented. Just look int he documentation and you'll find a code example that just suit your needs. The more tricky part is setting up your credentials and define the backend (both is covered later on). The only thing I had to change there was the service-plan-resources kind-property which was set to Linux (which was fine at first). Due to it gets changed in the step when the code is deployed without terraform it led to confusions. So just give it the kind: FunctionApp.

## credentials

The idea here is to not use your account credentials or login through the azure-CLI (it wont be available in the pipeline anyways) but to create a service principle that has the rights to run the terraform script.
Therfore go to the portal.azure.com front-end. Navigate to the Azure Active Directory page. Look for app-registration -> create a new one -> give it a role-assignement. The last step is done by going to the subscriptions page, select your subscription, navigate to the IAM, click 'Add' and select 'Add role assignement'.
In the upcoming menu choose as role 'contributor' (that should be enough to run terraform) and for the point 'Select' search for your service principle that you created.
In the newly created service principal create a new secret. You'll need the client_id of the principal, the just createrd client_secret (which is called value in the GUI) and also the tenant_id and the subscription_id (your user-ID). You have to pass theswe four credentials to terraform. You can check if it works by running terraform locally and putting them into the providers-part of the terraform{}-section. A more advanced way to do it is to create a variables.tf and a secert.tfvars-file. You should add both to your .gitignore file since in the pipeline we want to use environment variables instead. Check stage 2 - section for that, too.

# YAML

## stage 1

This is pretty straight foreward. When you go to the DevOps-portal and choose your pipleine you can click on edit. All the steps here can also be performed locally in the .yml file but here you have the advantage that you can use some templates which is quiet usefull if you are new to the topic. The YAML should have a similar structure as stated above when I talked about the pipeline creation. The first task should be to use the 'dotnet restore' - command. You can search in the section on the right for the equivalent template and configure it as you wish (which is not that much at this point :)). Go ahead and create a dotnet build and test - task likewise. The next thing is to publish the dotnet-artifact. There is also a template for that. Next we need the terraform files to be published too and therefore we need a copy-files task that put everything from the terrafrom directory in the directory of the artifact as well (which is $(build.artifactstagingdirectory)). The very last thing for the first stage of the pipeline is to publish this directory. Similar to the other steps so far there is a template for that ready to be used.

## stage 2

This is the more advanced part of the pipeline. Here you want to invoke terraform with your credetnials and deploy your build to the created AzureFunction. First thing is to download the published Artifact from stage 1. Use the template as usual. Now we need to install terraform on this container (at least I had to do this). Then you there are two more terraform tasks to be done: init and apply. Since we are using a seperate container for the .tfstate-file you have to specifiy this container when creating the init-task.
It should look like that:

- task: TerraformTaskV2@2
  inputs:
  provider: "azurerm"
  command: "init"
  workingDirectory: "$(System.ArtifactsDirectory)/drop/Terraform"
  backendServiceArm: "operations"
  backendAzureRmResourceGroupName: "azure-menti-training"
  backendAzureRmStorageAccountName: "thinkportlukasfirstcicd"
  backendAzureRmContainerName: "tfstate"
  backendAzureRmKey: "AzureFunctions.tfstate"

where operations is the subscription, azure-menti-training is the resource-group, thinkportlukasfirstcicd is the name of the Blob-storage and tfstate the container-name. You also need

backend "azurerm" {
}

this snippet in your terrafrom{}-section in the main.tf. Without that it wont create the .tfstate - file in the container.
Now we need to set the credentials as environment-variables for the terraform-apply-task. When you are editing the .yml in the fron-end there is a 'Variables'-button right above the template-section on the right. Here you can specify the variables and please mark the client_serect as secret key.
There is a specific syntax for using these fpr your terraform-invokation in you .yml:

          - task: TerraformTaskV2@2
            inputs:
              provider: "azurerm"
              command: "apply"
              workingDirectory: "$(System.ArtifactsDirectory)/drop/Terraform"
              environmentServiceNameAzureRM: "operations"
            env:
              ARM_CLIENT_SECRET: $(terraform_client_secret)
              ARM_CLIENT_ID: "d64d6a46-977d-44b5-a180-80b9b43b83ab"
              ARM_SUBSCRIPTION_ID: "09a38f01-eb6d-4b29-8759-eaac6c6e0933"
              ARM_TENANT_ID: "12e29f7c-8633-4490-ab9d-95ba84981681"

In this case only the client_secret is a env-var and the other three (since they are not that critical) are defined on the spot.
I hope this was at least a little bit helpful. i wish you good loúck and all the best. Never stop believing in yourself! You can do it!
