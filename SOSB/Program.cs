using System.Data;
using LibSOSB;
using LibSOSB.Types;
using Newtonsoft.Json;

namespace SOSB;

public class Program
{
    static void Version()
    {
        Console.WriteLine("SOSB - The Sipaa Operating System Builder");
        Console.WriteLine("Version 0.1 (BETA)");
        Console.WriteLine();
        Console.WriteLine("Copyright (C) 2023 The SipaaOS Project");
    }

    static void Help()
    {
        Console.WriteLine("SOSB - The Sipaa Operating System Builder");
        Console.WriteLine();
        Console.WriteLine("Commands :");
        Console.WriteLine("\tversion : Show the builder version.");
        Console.WriteLine("\tnew <template> : Create a new project.");
        Console.WriteLine("\tdelete : Delete a project");
        Console.WriteLine("\tbuild : Build the project.");
        Console.WriteLine("\thelp : Show this message.");
    }

    static void NewProject(string type)
    {
        DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);

        Console.WriteLine($"Creating new project in {di.Name}...");

        if (type == "limine-amd64")
        {
            Console.WriteLine($"The project will be created with Limine BareBones AMD64 template.");

            Project p = LimineAMD64ProjectTemplate.GetProject();
            p.Name = di.Name;

            // Create project JSON file
            string json = JsonConvert.SerializeObject(p, Formatting.Indented);
            File.WriteAllText(Path.Combine(di.FullName, "project.json").ToString(), json);

            // Create kernel directories
            Directory.CreateDirectory(p.KernelSourceDir);
            Directory.CreateDirectory(Path.Combine(p.KernelSourceDir, "limine"));

            // Get files
            File.WriteAllText(Path.Combine(Path.Combine(p.KernelSourceDir, "limine"), "limine.h"), LimineAMD64ProjectTemplate.GetFile(LimineAMD64ProjectTemplate.FileType.LimineBootProtocolHeader));
            File.WriteAllText(Path.Combine(p.KernelSourceDir, "kernel.c"), LimineAMD64ProjectTemplate.GetFile(LimineAMD64ProjectTemplate.FileType.Kernel));
            File.WriteAllText(p.KernelLinkFile, LimineAMD64ProjectTemplate.GetFile(LimineAMD64ProjectTemplate.FileType.LinkFile));
        }
        else
        {
            Console.WriteLine($"The provided template isn't recognized. Making a basic project...");

            Project p = new();
            p.Name = di.Name;

            // Create project JSON file
            string json = JsonConvert.SerializeObject(p, Formatting.Indented);
            File.WriteAllText(Path.Combine(di.FullName, "project.json").ToString(), json);

            // Create kernel directories
            Directory.CreateDirectory(p.KernelSourceDir);
        }
    }

    static void Delete()
    {
        DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
        Project p = JsonConvert.DeserializeObject<Project>(File.ReadAllText(Path.Combine(di.FullName, "project.json")));

        Console.WriteLine($"Deleting project in {di.Name}...");

        // Delete project JSON file
        if (File.Exists(Path.Combine(di.FullName, "project.json").ToString()))
            File.Delete(Path.Combine(di.FullName, "project.json").ToString());

        // Delete kernel directories
        Clean();
        if (File.Exists(p.KernelSourceDir))
            Directory.Delete(p.KernelSourceDir, true);
    }

    static void Build()
    {
        DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
        Project p = JsonConvert.DeserializeObject<Project>(File.ReadAllText(Path.Combine(di.FullName, "project.json")));
        ProjectTarget pt = ProjectTarget.FromProject(p);

        Console.WriteLine($"Compiling {p.Name}");
        pt.Build();
    }

    static void Clean()
    {
        DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
        Project p = JsonConvert.DeserializeObject<Project>(File.ReadAllText(Path.Combine(di.FullName, "project.json")));

        if (Directory.Exists(p.KernelBinDir))
            Directory.Delete(p.KernelBinDir, true);

        if (Directory.Exists(p.KernelObjectDir))
            Directory.Delete(p.KernelObjectDir, true);
    }

    static void ListTemplates()
    {
        Console.WriteLine("Available templates: ");
        Console.WriteLine("\tlimine-amd64: AMD64 Limine BareBones");
        Console.WriteLine("");
        Console.WriteLine("More templates will come soon!");
    }

    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("error: invalid arguments");
            return;
        }

        if (args[0] == "help")
            Help();

        if (args[0] == "version")
            Version();

        if (args[0] == "delete")
            Delete();

        if (args[0] == "new")
            if (args.Length > 1)
                if (args[1] == "list")
                    ListTemplates();
                else
                    NewProject(args[1]);
            else
                Console.WriteLine("error: invalid arguments");

        if (args[0] == "build")
            Build();

        if (args[0] == "cleanup")
            Clean();
    }
}