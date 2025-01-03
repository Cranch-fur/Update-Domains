﻿// Update Domains
// https://github.com/Cranch-fur/Update-Domains

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
        const string gDPILocalDomainsFilePath                 = "russia-blacklist.txt";
        const string zapretLocalDomainsFilePath               = "lists\\other.txt";
        const string zapretWinBundleLocalDomainsFilePath      = "list-youtube.txt";
        const string zapretDiscordYouTubeLocalDomainsFilePath = "list-general.txt";
        const string defaultDomainsFilePath                   = "Default Domains.txt";

        const string logFilePath                              = "Update Domains.log";




        static void FlushLog() => File.WriteAllText(logFilePath, string.Empty);
        static void WriteLog(string message)
        {
            Console.WriteLine(message);
            File.AppendAllText(logFilePath, $"{message}\n");
        }


        static void Exit()
        {
            WriteLog("\nPress ENTER to continue...");
            Console.ReadLine();


            Environment.Exit(0);
        }


        static void CheckDefaultDomains()
        {
            if (File.Exists(defaultDomainsFilePath) == false)
            {
                WriteLog($"[ERROR] {defaultDomainsFilePath} File is missing!");
                Exit();
            }
        }
        static void PopulateFile(string filePath, HashSet<string> domainsList)
        {
            WriteLog("\n\n");
            

            if (File.Exists(filePath) == true)
            {
                WriteLog($"[~] \"{filePath}\" Detected!");
                WriteLog("[~] Attempting to update the file...");
                Thread.Sleep(3000); // Intentional slow-down to let user read LOG we've just printed.


                HashSet<string> localDomainsHashSet = new HashSet<string>(File.ReadAllLines(filePath));
                HashSet<string> uniqueDomains = new HashSet<string>(); // HashSet of domains we consider "unique" - those domains aren't present in file yet.

                foreach (string domain in domainsList)
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
                        WriteLog($"[+] {domain}");
                        localDomainsHashSet.Add(domain);
                    }


                    File.WriteAllLines(filePath, localDomainsHashSet.OrderBy(item => item));
                    WriteLog($"[~] \"{filePath}\" File successfully updated!");
                }
                else
                {
                    WriteLog("[~] There's no new domains to add, your file is already up to date!");
                }
            }
            else
            {
                WriteLog($"[~] \"{filePath}\" File wasn't found!");
                WriteLog("[~] Attempting to create the file...");
                Thread.Sleep(3000); // Intentional slow-down to let user read LOG we've just printed.


                CheckDefaultDomains();


                HashSet<string> newDomainsList = new HashSet<string>(domainsList); // Further in the code we will be making changes to HashSet we've received on function execution. Variables from function execution are references to already existing ones, since we don't want to ruin our domainsList, we're making up a new one.
                HashSet<string> defaultDomains = new HashSet<string>(File.ReadAllLines(defaultDomainsFilePath));
                foreach (string domain in defaultDomains)
                {
                    if (newDomainsList.Contains(domain) == false)
                    {
                        newDomainsList.Add(domain);
                    }
                }

                
                File.WriteAllLines(filePath, newDomainsList.OrderBy(item => item));
                WriteLog($"[~] \"{filePath}\" File successfully created!");
            }


            WriteLog("\n");
        }


        static void Main(string[] args)
        {
            Console.BufferHeight = short.MaxValue - 1; // Set console height buffer size to maximum available.
            FlushLog();

            WriteLog(" ██████╗ ██████╗ ██████╗ ██╗        ██╗    ███████╗ █████╗ ██████╗ ██████╗ ███████╗████████╗");
            WriteLog("██╔════╝ ██╔══██╗██╔══██╗██║       ██╔╝    ╚══███╔╝██╔══██╗██╔══██╗██╔══██╗██╔════╝╚══██╔══╝");
            WriteLog("██║  ███╗██║  ██║██████╔╝██║      ██╔╝       ███╔╝ ███████║██████╔╝██████╔╝█████╗     ██║   ");
            WriteLog("██║   ██║██║  ██║██╔═══╝ ██║     ██╔╝       ███╔╝  ██╔══██║██╔═══╝ ██╔══██╗██╔══╝     ██║   ");
            WriteLog("╚██████╔╝██████╔╝██║     ██║    ██╔╝       ███████╗██║  ██║██║     ██║  ██║███████╗   ██║   ");
            WriteLog(" ╚═════╝ ╚═════╝ ╚═╝     ╚═╝    ╚═╝        ╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝  ╚═╝╚══════╝   ╚═╝   ");
            WriteLog("Domains list updating tool");
            WriteLog("Originator: bropines");
            WriteLog("Tool by: Cranch (Кранч) The Wolf");
            WriteLog("\n\n");


            WriteLog("[~] Attempting to fetch domains...");
            var getDomainsResponse = Networking.Get("https://zapret.cranchpalace.info/getDomains", 300);
            if (getDomainsResponse.statusCode != Networking.E_StatusCode.OK)
            {
                WriteLog("[ERROR] Failed to fetch domains.");
            }
            else
            {
                string getDomainsContent = getDomainsResponse.content;
                HashSet<string> getDomainsList = new HashSet<string>(getDomainsContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
                WriteLog($"[~] Domains fetched: {getDomainsList.Count}");


                WriteLog(string.Empty);


                WriteLog("[~] Scanning for Goodbye DPI...");
                if (Directory.Exists("x86")) // Directory included with: https://github.com/ValdikSS/GoodbyeDPI
                {
                    WriteLog("[~] Goodbye DPI detected.");
                    PopulateFile(gDPILocalDomainsFilePath, getDomainsList);
                }
                else
                {
                    WriteLog("[~] Goodbye DPI wasn't found.");
                }


                WriteLog(string.Empty);


                WriteLog("[~] Scanning for Zapret...");
                if (Directory.Exists("bin") && Directory.Exists("lists")) // Directories included with: https://github.com/censorliber/zapret
                {
                    WriteLog("[~] Zapret detected.");
                    PopulateFile(zapretLocalDomainsFilePath, getDomainsList);
                }
                else
                {
                    WriteLog("[~] Zapret wasn't found.");
                }


                WriteLog(string.Empty);


                WriteLog("[~] Scanning for Zapret Win Bundle...");
                if (File.Exists("killall.exe") && File.Exists("_CMD_ADMIN.cmd")) // Files included with: https://github.com/bol-van/zapret-win-bundle
                {
                    WriteLog("[~] Zapret Win Bundle detected.");
                    PopulateFile(zapretWinBundleLocalDomainsFilePath, getDomainsList);
                }
                else
                {
                    WriteLog("[~] Zapret Win Bundle wasn't found.");
                }


                WriteLog(string.Empty);


                WriteLog("[~] Scanning for Zapret - Disord - Youtube...");
                if (Directory.Exists("bin") && File.Exists("general.bat")) // Directory & File included with: https://github.com/Flowseal/zapret-discord-youtube
                {
                    WriteLog("[~] Zapret - Disord - Youtube detected.");
                    PopulateFile(zapretDiscordYouTubeLocalDomainsFilePath, getDomainsList);
                }
                else
                {
                    WriteLog("[~] Zapret - Disord - Youtube wasn't found.");
                }
            }


            Exit();
        }
    }
}
