using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureDevopsBuildIdIncrementer
{
    class AzureDevOpsClient
    { 

        public Uri Endpoint { get; private set; }
        public TeamProjectReference SelectedProject { get; private set; }
        public BuildDefinitionReference DummyBuildDef { get; private set; }

        private VssConnection azureDevopsConnection;
        private BuildHttpClient buildClient;
        private ProjectHttpClient projectClient;
        private int maxBuildId; 

        public AzureDevOpsClient(string organizationName)
        {
            organizationName = organizationName.Trim();
            Endpoint = new Uri(@"https://dev.azure.com/" + organizationName + "/");
            var creds = new VssClientCredentials(new WindowsCredential(false), new VssFederatedCredential(true), CredentialPromptType.PromptIfNeeded);
            azureDevopsConnection = new VssConnection(Endpoint, creds);
            azureDevopsConnection.ConnectAsync().Wait();

            buildClient = azureDevopsConnection.GetClient<BuildHttpClient>();
            projectClient = azureDevopsConnection.GetClient<ProjectHttpClient>();
        }

        public List<TeamProjectReference> Projects
        {
            get
            {    
                return projectClient.GetProjects().Result.ToList();
            }           
        }

        public void Select(int index)
        {
            SelectedProject = Projects[index];
        }

        public void SetMaxBuildId(int maxBuildId)
        {
            this.maxBuildId = maxBuildId;
        }
        
        public static string DummyDefName = "Dummy Build for Incrementing BuildId";
        public void CloneBuildDefinitionIfNeeded()
        {
            var definitions = buildClient.GetDefinitionsAsync(project: SelectedProject.Name).Result;
            var firstDef = definitions.First();
            DummyBuildDef = definitions.FirstOrDefault(b => b.Name== DummyDefName) ;
            
            if (DummyBuildDef == null) // clone needs to be created
            {
                var cloneDef = buildClient.GetDefinitionAsync(SelectedProject.Id, firstDef.Id).Result;
                cloneDef.Name = DummyDefName;
                var cloneResult = buildClient.CreateDefinitionAsync(cloneDef, SelectedProject.Id, firstDef.Id).Result;

                var newDefinitions = buildClient.GetDefinitionsAsync(project: SelectedProject.Name).Result;
                DummyBuildDef = newDefinitions.First(b => b.Name == DummyDefName);      
            }       
        }

        public void DeleteBuildDefinitinIfPossible()
        {
            var definitions = buildClient.GetDefinitionsAsync(project: SelectedProject.Name).Result;
            DummyBuildDef = definitions.FirstOrDefault(b => b.Name == DummyDefName);

            if (DummyBuildDef != null) // clone needs to be deleted
            {             
                var cloneResult = buildClient.DeleteDefinitionAsync(SelectedProject.Id, DummyBuildDef.Id);
            }
        }

        public int LastBuildIdWithOutDeleted
        {
            get
            {              
                var builds = buildClient.GetBuildsAsync(project: SelectedProject.Name).Result;
                return builds.Max(b => b.Id);
            }
        }

        public List<int> MakeDummyIncrementBuildsUntilMax()
        {
            var idsIncremented = new List<int>();
            int lastBuildId = LastBuildIdWithOutDeleted;
            if (maxBuildId <= LastBuildIdWithOutDeleted) throw new Exception("max buildid not higher then last possible id");
            do
            {
                lastBuildId = MakeNewDummyIncrementBuild();
                idsIncremented.Add(lastBuildId);

            } while (lastBuildId < maxBuildId);
            return idsIncremented;
        }

        public int MakeNewDummyIncrementBuild()
        {          
            //queue
            var queueResult = buildClient.QueueBuildAsync(new Build
            {
                Definition = new DefinitionReference
                {
                    Id = DummyBuildDef.Id
                },
                //SourceBranch = "master_android",
                Project = DummyBuildDef.Project,
                
            }).Result;
                      
            // cancel
            queueResult.Status = BuildStatus.Cancelling;
            var cancelResult = buildClient.UpdateBuildAsync(queueResult, queueResult.Id).Result;

            // deleted
            buildClient.DeleteBuildAsync(SelectedProject.Id,cancelResult.Id).Wait();

            return cancelResult.Id;
        }
    }
}
