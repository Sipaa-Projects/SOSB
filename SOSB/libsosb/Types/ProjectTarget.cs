using System.Diagnostics;

namespace LibSOSB.Types;

public class ProjectTarget
{
    public string CPPTarget = "";
    public string CTarget = "";
    public string CPPCompilerPath = "g++";
    public string CPPCompilerFlags = "";
    public string CCompilerPath = "gcc";
    public string CCompilerFlags = "";
    public string LinkerTarget = "";
    public string LinkerPath = "ld";
    public string LinkerFlags = "";
    public string AssemblerPath = "nasm";
    public string AssemblerFlags = "";
    public Project proj;

    public static ProjectTarget FromProject(Project p)
    {
        ProjectTarget pt = new();

        pt.AssemblerFlags = p.AssemblerFlags;
        pt.CPPTarget = p.CPPTarget;
        pt.CTarget = p.CTarget;
        pt.CPPCompilerPath = p.CPPCompilerPath;
        pt.CCompilerPath = p.CCompilerPath;
        pt.AssemblerPath = p.AssemblerPath;
        pt.LinkerPath = p.LinkerPath;
        pt.LinkerFlags = p.LinkerFlags;
        pt.CCompilerFlags = p.CCompilerFlags;
        pt.CPPCompilerFlags = p.CPPCompilerFlags;
        pt.proj = p;

        return pt;
    }

    public void Build()
    {
        // Prepare directories for compilation
        if (!Directory.Exists(proj.KernelObjectDir))
            Directory.CreateDirectory(proj.KernelObjectDir);
        if (!Directory.Exists(proj.KernelBinDir))
            Directory.CreateDirectory(proj.KernelBinDir);

        // Search the source files to compile
        string[] source = null;
        string[] objects = null;

        if (proj.Language == ProjectLanguage.StandardC)
            source = Directory.GetFiles(proj.KernelSourceDir, "*.c", SearchOption.AllDirectories);
        else if (proj.Language == ProjectLanguage.CPlusPlus)
            source = Directory.GetFiles(proj.KernelSourceDir, "*.cpp", SearchOption.AllDirectories);
        else
            return;

        objects = new string[source.Length];

        // Compile source files
        for (int i = 0; i < source.Length; i++)
        {
            string srcfile = source[i];

            if (proj.Language == ProjectLanguage.StandardC)
            {
                string dirreplace = srcfile.Replace(proj.KernelSourceDir, proj.KernelObjectDir).Replace(".c", ".o");

                FileInfo f = new FileInfo(dirreplace);
                if (!Directory.Exists(f.Directory.FullName))
                    Directory.CreateDirectory(f.Directory.FullName);

                objects[i] = dirreplace;

                Console.WriteLine($"[CC] {srcfile} -> {dirreplace}");

                Process p = Process.Start($"{this.CTarget}{this.CCompilerPath}", $"{this.CCompilerFlags} -o {dirreplace} -c {srcfile}");
                p.WaitForExit();
            }
            else if (proj.Language == ProjectLanguage.CPlusPlus)
            {
                string dirreplace = srcfile.Replace(proj.KernelSourceDir, proj.KernelObjectDir).Replace(".cpp", ".o");

                FileInfo f = new FileInfo(dirreplace);
                if (!Directory.Exists(f.Directory.FullName))
                    Directory.CreateDirectory(f.Directory.FullName);

                objects[i] = dirreplace;

                Console.WriteLine($"[CXX] {srcfile} -> {dirreplace}");

                Process p = Process.Start($"{this.CPPTarget}{this.CPPCompilerPath}", $"{this.CPPCompilerFlags} -o {dirreplace} -c {srcfile}");
                p.WaitForExit();
            }
        }

        // Link the files
        string oFiles = "";
        foreach (string file in objects)
        {
            oFiles += $"{file} ";
        }

        Console.WriteLine($"[LD] {proj.KernelExecutable}");
        Process.Start($"{this.LinkerTarget}{this.LinkerPath}", $"{this.LinkerFlags} -T {proj.KernelLinkFile} -o {proj.KernelExecutable} {oFiles}");
    }
}