namespace SOSB;

public class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("error: invalid arguments");
            return;
        }
        else if (args[0] == "help")
            Commands.Help();
        else if (args[0] == "version")
            Commands.Version();
        else if (args[0] == "delete")
            Commands.Delete();
        else if (args[0] == "new")
            if (args.Length > 1)
                if (args[1] == "list")
                    Commands.ListTemplates();
                else
                    if (args.Length > 2)
                        Commands.NewProject(args[1], args[2]);
                    else
                        Commands.NewProject(args[1]);
            else
                Console.WriteLine("error: invalid arguments");
        else if (args[0] == "build")
            if (args.Length > 1)
                Commands.Build(args[1]);
            else
                Commands.Build();
        else if (args[0] == "build-iso")
            if (args.Length > 1)
                Commands.BuildISO(args[1]);
            else
                Commands.BuildISO();
        else if (args[0] == "cleanup")
            Commands.Clean();
        else
        {
            Console.WriteLine("error: invalid arguments");
            return;
        }
    }
}