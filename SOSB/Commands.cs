using System.Data;
using LibSOSB;
using LibSOSB.Types;
using Newtonsoft.Json;

namespace SOSB;

public class Commands 
{
    public static void Version()
    {
        Console.WriteLine("SOSB - The Sipaa Operating System Builder");
        Console.WriteLine("Version 0.1 (BETA)");
        Console.WriteLine();
        Console.WriteLine("Copyright (C) 2023 The SipaaOS Project");
    }

    public static void Help()
    {
        Console.WriteLine("SOSB - The Sipaa Operating System Builder");
        Console.WriteLine();
        Console.WriteLine("Commands :");
        Console.WriteLine("\tversion : Show the builder version.");
        Console.WriteLine("\tnew <template> <outputjson:optional> : Create a new project.");
        Console.WriteLine("\tdelete : Delete a project");
        Console.WriteLine("\tbuild <outputjson:optional> : Build the project.");
        Console.WriteLine("\tcleanup <outputjson:optional> : Delete all the object & output files of the project.");
        Console.WriteLine("\thelp : Show this message.");
    }

    public static void BuildISO(string projectJson = "project.json")
    {
        DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
        Project p = JsonConvert.DeserializeObject<Project>(File.ReadAllText(Path.Combine(di.FullName, projectJson)));
        ProjectTarget pt = ProjectTarget.FromProject(p);

        if (!File.Exists(p.KernelExecutable))
        {
            Build(projectJson);
        }

        Console.WriteLine($"Building ISO for {p.Name}");
        pt.BuildISO();
    }

    public static void NewProject(string type, string outputProjectFile = "project.json")
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
            File.WriteAllText(Path.Combine(di.FullName, outputProjectFile).ToString(), json);

            // Create kernel directories
            Directory.CreateDirectory(p.KernelSourceDir);
            Directory.CreateDirectory(Path.Combine(p.KernelSourceDir, "limine"));

            // Get files
            File.WriteAllText(Path.Combine(Path.Combine(p.KernelSourceDir, "limine"), "limine.h"), LimineAMD64ProjectTemplate.GetFile(LimineAMD64ProjectTemplate.FileType.LimineBootProtocolHeader));
            File.WriteAllText(Path.Combine(p.KernelSourceDir, "kernel.c"), LimineAMD64ProjectTemplate.GetFile(LimineAMD64ProjectTemplate.FileType.Kernel));
            File.WriteAllText(p.KernelLinkFile, LimineAMD64ProjectTemplate.GetFile(LimineAMD64ProjectTemplate.FileType.LinkFile));
        }
        else if (type == "limine-aarch64")
        {
            Console.WriteLine($"The project will be created with Limine BareBones ARM64 template.");

            Project p = LimineARM64ProjectTemplate.GetProject();
            p.Name = di.Name;

            // Create project JSON file
            string json = JsonConvert.SerializeObject(p, Formatting.Indented);
            File.WriteAllText(Path.Combine(di.FullName, outputProjectFile).ToString(), json);

            // Create kernel directories
            Directory.CreateDirectory(p.KernelSourceDir);
            Directory.CreateDirectory(Path.Combine(p.KernelSourceDir, "limine"));

            // Get files
            File.WriteAllText(Path.Combine(Path.Combine(p.KernelSourceDir, "limine"), "limine.h"), LimineARM64ProjectTemplate.GetFile(LimineARM64ProjectTemplate.FileType.LimineBootProtocolHeader));
            File.WriteAllText(Path.Combine(p.KernelSourceDir, "kernel.c"), LimineARM64ProjectTemplate.GetFile(LimineARM64ProjectTemplate.FileType.Kernel));
            File.WriteAllText(p.KernelLinkFile, LimineARM64ProjectTemplate.GetFile(LimineARM64ProjectTemplate.FileType.LinkFile));
        }
        else
        {
            Console.WriteLine($"The provided template isn't recognized. Making a basic project...");

            Project p = new();
            p.Name = di.Name;

            // Create project JSON file
            string json = JsonConvert.SerializeObject(p, Formatting.Indented);
            File.WriteAllText(Path.Combine(di.FullName, outputProjectFile).ToString(), json);

            // Create kernel directories
            Directory.CreateDirectory(p.KernelSourceDir);
        }
    }

    public static void Delete()
    {
        DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
        Directory.Delete(di.FullName, true);
    }

    public static void Build(string projectJson = "project.json")
    {
        DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
        Project p = JsonConvert.DeserializeObject<Project>(File.ReadAllText(Path.Combine(di.FullName, projectJson)));
        ProjectTarget pt = ProjectTarget.FromProject(p);

        Console.WriteLine($"Compiling {p.Name}");
        pt.Build();
    }

    public static void Clean(string projectJson = "project.json")
    {
        DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
        Project p = JsonConvert.DeserializeObject<Project>(File.ReadAllText(Path.Combine(di.FullName, projectJson)));

        if (Directory.Exists(p.KernelBinDir))
            Directory.Delete(p.KernelBinDir, true);

        if (Directory.Exists(p.KernelObjectDir))
            Directory.Delete(p.KernelObjectDir, true);
    }

    public static void ListTemplates()
    {
        Console.WriteLine("Available templates: ");
        Console.WriteLine("\tlimine-amd64: AMD64 Limine BareBones");
        Console.WriteLine("\tlimine-aarch64: ARM64 Limine BareBones");
        Console.WriteLine("");
        Console.WriteLine("More templates will come soon!");
    }
}