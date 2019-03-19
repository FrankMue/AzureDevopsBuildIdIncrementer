using System;

namespace AzureDevopsBuildIdIncrementer
{
    class Program
    {

        private static AzureDevOpsClient azureDevOps;

        static void Main(string[] args)
        {
            Console.WriteLine("=======================================");
            Console.WriteLine("==  Azure DevOps BuidID Incrementer  ==");
            Console.WriteLine("=======================================");

            AskForOrgName();
            ChooseProject();
            AskForMaxIncrement();
            AskForExecution();
                    
            Console.WriteLine("finished - press any key to exit");
            Console.ReadKey();
        }
      
        private static void AskForOrgName()
        {
            Console.WriteLine();
            Console.WriteLine(@"Please enter organisation name: (i.e. https://dev.azure.com/NAME/");

            var name = Console.ReadLine();

            azureDevOps = new AzureDevOpsClient(name);

            Console.WriteLine();
            Console.WriteLine("Try to connect to " + azureDevOps.Endpoint);
        }
        private static void ChooseProject()
        {
            Console.WriteLine();
            var projects = azureDevOps.Projects;
            int choosenProject = 0;
            string line = null;
            do
            {
                Console.WriteLine("Please choose a project:");
                var i = 1;
                foreach (var pro in projects)
                {
                    Console.WriteLine(i + " " + pro.Name);
                    i++;
                }
                line = Console.ReadLine();
            } while (String.IsNullOrWhiteSpace(line) || int.TryParse(line, out choosenProject) == false || choosenProject <= 0 || choosenProject > projects.Count);


            azureDevOps.Select(choosenProject - 1);
            Console.WriteLine("Choosen project is " + azureDevOps.SelectedProject.Name);
        }
        private static void AskForMaxIncrement()
        {
            Console.WriteLine();

            var lastId = azureDevOps.LastBuildIdWithOutDeleted;
            string line;
            int maxId = lastId;
            Console.WriteLine($"Last BuildId is currently {lastId}");
            do
            {
                Console.WriteLine("Increment until:");
                line = Console.ReadLine();
            } while (String.IsNullOrWhiteSpace(line) || int.TryParse(line, out maxId) == false || maxId <= lastId);
            Console.WriteLine($"You choose to increment until " + maxId);
            azureDevOps.SetMaxBuildId(maxId);
        }
        private static void AskForExecution()
        {
            Console.WriteLine();
            Console.WriteLine("Do you really want to execute");
            Console.WriteLine("press Y to continue");
            var key = Console.ReadKey(true);

            if (key.KeyChar == 'y' || key.KeyChar == 'Y')
            {

                //Console.WriteLine("DONE");
                Console.WriteLine();

                azureDevOps.CloneBuildDefinitionIfNeeded();
                Console.WriteLine("Cloned first definition found");

                var ids = azureDevOps.MakeDummyIncrementBuildsUntilMax();

                foreach (int id in ids)
                {
                    Console.WriteLine($"Created dummy build with id {id} (Queue -> Cancelled -> Deleted)");
                }

                Console.WriteLine();
                Console.WriteLine("Do you want to delete the dummy build definition");
                Console.WriteLine("press Y to continue");
                if (key.KeyChar == 'y' || key.KeyChar == 'Y')
                {
                    azureDevOps.DeleteBuildDefinitinIfPossible();
                    Console.WriteLine("Cloned build definition deleled");
                }
            }
        }


    }
}
