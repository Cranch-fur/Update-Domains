using CranchyLib.Networking; // Source: https://github.com/Cranch-fur/CranchyLib.Networking
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace UpdateDomains
{
    internal class Program
    {
        internal const string gDPILocalDomainsFilePath   = "russia-blacklist.txt";
        internal const string zapretLocalDomainsFilePath = "list-general.txt";
        internal const string defaultDomainsFilePath     = "Default Domains.txt";




        static void Exit()
        {
            Console.Write("\nPress ENTER to continue...");
            Console.ReadLine();


            Environment.Exit(0);
        }
        static void CheckDefaultDomains()
        {
            if (File.Exists(defaultDomainsFilePath) == false)
            {
                Console.WriteLine($"[ERROR] {defaultDomainsFilePath} File is missing!");
                Exit();
            }
        }
        static void PopulateFile(string filePath, List<string> domainsList)
        {
            Console.WriteLine("\n\n");
            List<string> newDomainsList = new List<string>(domainsList); // Further in the code we will be making changes to List we've received on function execution. Variables from function execution are references to already existing ones, since we don't want to ruin our domainsList, we're making up a new one.


            if (File.Exists(filePath) == true)
            {
                Console.WriteLine($"[~] \"{filePath}\" File was found!");
                Console.WriteLine("[~] Attempting to update the file...");
                Thread.Sleep(3000); // Intentional slow-down to let user read LOG we've just printed.


                HashSet<string> localDomainsHashSet = new HashSet<string>(File.ReadAllLines(filePath)); // As soon as we read file lines, convert them to HashSet data type. HashSet allows us to effectively iterate through thousands of elements.
                List<string> uniqueDomains = new List<string>(); // List of domains we consider "unique" - those domains aren't present in file yet.

                foreach (string domain in newDomainsList)
                {
                    if (localDomainsHashSet.Contains(domain) == false)
                    {
                        uniqueDomains.Add(domain);
                    }
                }

                if (uniqueDomains.Count > 0) // See if we've found any unique domains in first place.
                {
                    foreach (string domain in uniqueDomains)
                    {
                        Console.WriteLine($"[+] {domain}");
                        localDomainsHashSet.Add(domain);
                    }

                    List<string> outDomainsList = localDomainsHashSet.OrderBy(item => item).ToList(); 
                    File.WriteAllLines(filePath, outDomainsList);


                    Console.WriteLine($"[~] \"{filePath}\" File successfully updated!");
                }
                else
                {
                    Console.WriteLine("[~] There's no new domains to add, your file is already up to date!");
                }
            }
            else
            {
                Console.WriteLine($"[~] \"{filePath}\" File wasn't found!");
                Console.WriteLine("[~] Attempting to create the file...");
                Thread.Sleep(3000); // Intentional slow-down to let user read LOG we've just printed.


                CheckDefaultDomains();


                List<string> defaultDomains = File.ReadAllLines(defaultDomainsFilePath).ToList();
                foreach (string defaultDomain in defaultDomains)
                {
                    newDomainsList.Add(defaultDomain);
                }

                List<string> outDomainsList = newDomainsList.OrderBy(item => item).ToList(); // Sort List entries.
                File.WriteAllLines(filePath, outDomainsList);


                Console.WriteLine($"[~] \"{filePath}\" File successfully created!");
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine(" ██████╗ ██████╗ ██████╗ ██╗        ██╗    ███████╗ █████╗ ██████╗ ██████╗ ███████╗████████╗");
            Console.WriteLine("██╔════╝ ██╔══██╗██╔══██╗██║       ██╔╝    ╚══███╔╝██╔══██╗██╔══██╗██╔══██╗██╔════╝╚══██╔══╝");
            Console.WriteLine("██║  ███╗██║  ██║██████╔╝██║      ██╔╝       ███╔╝ ███████║██████╔╝██████╔╝█████╗     ██║   ");
            Console.WriteLine("██║   ██║██║  ██║██╔═══╝ ██║     ██╔╝       ███╔╝  ██╔══██║██╔═══╝ ██╔══██╗██╔══╝     ██║   ");
            Console.WriteLine("╚██████╔╝██████╔╝██║     ██║    ██╔╝       ███████╗██║  ██║██║     ██║  ██║███████╗   ██║   ");
            Console.WriteLine(" ╚═════╝ ╚═════╝ ╚═╝     ╚═╝    ╚═╝        ╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝  ╚═╝╚══════╝   ╚═╝   ");
            Console.WriteLine("Domains list updating tool");
            Console.WriteLine("Originator: bropines");
            Console.WriteLine("Tool by: Cranch (Кранч) The Wolf");
            Console.WriteLine("\n\n");


            Console.WriteLine("[~] Attempting to fetch domains...");


            var getDomainsResponse = Networking.Get("https://zapret.cranchpalace.info/getDomains", 300);
            if (getDomainsResponse.statusCode == Networking.E_StatusCode.OK)
            {
                string getDomainsContent = getDomainsResponse.content;
                List<string> getDomainsList = new List<string>(getDomainsContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
                Console.WriteLine($"[~] Domains fetched: {getDomainsList.Count}");


                Console.WriteLine("[~] Scanning for Goodbye DPI...");
                if (Directory.Exists("x86")) // Directory included with: https://github.com/ValdikSS/GoodbyeDPI
                {
                    Console.WriteLine("[~] Goodbye DPI detected.");
                    PopulateFile(gDPILocalDomainsFilePath, getDomainsList);
                }
                else
                {
                    Console.WriteLine("[~] Goodbye DPI wasn't found.");
                }


                Console.WriteLine("[~] Scanning for Zapret...");
                if (Directory.Exists("bin")) // Directory included with: https://github.com/Flowseal/zapret-discord-youtube
                {
                    Console.WriteLine("[~] Zapret detected.");
                    PopulateFile(zapretLocalDomainsFilePath, getDomainsList);
                }
                else
                {
                    Console.WriteLine("[~] Zapret wasn't found.");
                    Console.WriteLine("");
                }
            }
            else
            {
                Console.WriteLine("[ERROR] Failed to fetch domains.");
            }


            Exit();
        }
    }
}
