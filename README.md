# Azure Functions, Pipelines, Terraform

The goal of this excercise is to familiarize yourself with Azure Functions, Azure Pipelines and Terraform.
Write a simple Azure Function that reacts to an incoming HTTP request, does some processing, and returns data in the HTTP response.

The infrastructure (Function App, Storage Account) as well as the function's code should be deployed using a CI/CD-Pipeline
in Azure Pipelines.

## Azure Functions

[Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview) are small functions written in
one of many programming languages that run in a serverless environment, can respond to many
[different triggers](https://docs.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings?tabs=csharp), and
make it easy to interact with other Azure Services like Storage Accounts, CosmosDB, or Event Grid.

You can use the [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=macos%2Ccsharp%2Cbash)
to develop and test your function locally, as well as deploy your code to an Azure Functions instance.

Create an Azure Function in C# that handles addition of arbitrarily many numbers. It receives an HTTP POST request of the from

``` json
{
    "addends": [ 1, 3, 42, 23, 1337 ]
}
```

and should return a response of the from

``` json
{
    "result": 1406
}
```

If the list is empty, it should return 0.

Write unit tests to validate the behaviour your function.

## Terraform

Terraform is used to automatically and reproducibly create infrastructure in cloud environments. Familiarize yourself with
[Terraform on Azure](https://learn.hashicorp.com/tutorials/terraform/infrastructure-as-code?in=terraform/azure-get-started)
and create a Terraform configuration file that describes your Function App. The Function App will also need a Storage Account
to store its Code/Data.

## Azure Pipelines

[Azure Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/get-started/what-is-azure-pipelines?view=azure-devops)
comes as part of Azure DevOps and makes it easy to create CI/CD pipelines to automatically validate and deploy your code to a
target infrastructure. Pipelines are defined in a YAML format and live in the same repository as the code that they deploy.

Write a YAML pipeline that builds your function, runs tests on it, deploys the infrastructure using Terraform, and deploys your
function to the created Function App.