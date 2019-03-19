# Azure DevOps BuildId Incrementer
The BuildId in Azure DevOps / VisualStudioServices (VSS) is initialized and autoincremented by the service. There is no possibility to define a seed or change that value.
In some situations it is necessary to be greater or equal a already existent Id, i.e. deploying mobile apps to Google Play Store or Apple Connect /where the BuildId is used as version code

## Usage
* Clone or download the project
* Compile the solution with Visual Studio

Currently i do not provide an executable exe (unless somebody asks for it ;-)

## Disclaimer
You use this tool at your own risk. I will make changes to your Azure DevOps projects, which can't be undone.

