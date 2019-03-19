# Azure DevOps BuildId Incrementer
The BuildId in Azure DevOps / VisualStudioServices (VSS) is initialized and autoincremented by the service. There is no possibility to define a seed or change that value.
In some situations it is necessary to be greater or equal a already existent Id, i.e. deploying mobile apps to Google Play Store or Apple Connect /where the BuildId is used as version code

## How does it work
First you need a valid build defintion in your project. Then this tool queue as many builds needed to increment the BuildId to your expected value. Builds are cancelled and deleted immediately afterwards. This application clones the first existing definition so that the deleted dummy builds doesn't show up.

## Download 
[Release.1.0.0.zip (Windows, 64bit, created 19.03.2019)](https://github.com/FrankMue/AzureDevopsBuildIdIncrementer/raw/master/Compiled/Release.1.0.0.zip)

## Compile yourself
In the case that you need some changes. I.e. adding some values for the dummy builds (correct source branch...)

1. Clone or download the project
2. Compile and start the solution with Visual Studio

## Disclaimer
You use this tool at your own risk. I will make changes to your Azure DevOps projects, which can't be undone.

