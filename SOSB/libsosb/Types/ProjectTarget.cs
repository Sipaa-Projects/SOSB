using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

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

    public void BuildISO()
    {
        if (proj.Type == "limine")
        {
            string liminePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".sosb/limine");
            string sosbUserPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".sosb");

            if (!Directory.Exists(liminePath))
            {
                Console.WriteLine("[INIT] We didn't found Limine. We will extract it now.");
                Directory.CreateDirectory(sosbUserPath);

                Stream limineZip = Assembly.GetExecutingAssembly().GetManifestResourceStream("SOSB.limine.zip");
                if (limineZip == null)
                {
                    Console.WriteLine("[INIT] Cannot get Limine ZIP file.");
                    Environment.Exit(1);
                }

                Console.WriteLine("[INIT] Extracting Limine...");
                var file = File.Create(Path.Combine(sosbUserPath, "limine.zip"));
                limineZip.CopyTo(file);
                file.Close();

                ZipFile.ExtractToDirectory(Path.Combine(sosbUserPath, "limine.zip"), sosbUserPath);
            }

            string isoRoot = Path.Combine(proj.KernelBinDir, "IsoRoot");

            if (Directory.Exists(isoRoot))
                Directory.Delete(isoRoot, true);

            Directory.CreateDirectory(isoRoot);

            File.Copy(proj.KernelExecutable, Path.Combine(isoRoot, "kernel.elf"));
            File.Copy(Path.Combine(liminePath, "limine-bios.sys"), Path.Combine(isoRoot, "limine-bios.sys"));
            File.Copy(Path.Combine(liminePath, "limine-bios-cd.bin"), Path.Combine(isoRoot, "limine-bios-cd.bin"));
            File.Copy(Path.Combine(liminePath, "limine-uefi-cd.bin"), Path.Combine(isoRoot, "limine-uefi-cd.bin"));
            File.Copy(proj.BootloaderConfigPath, Path.Combine(isoRoot, "limine.cfg"));

            Console.WriteLine($"[XORRISO] {proj.OutputISOFile}");
            ProcessStartInfo i = new("xorriso", $"-as mkisofs -b limine-bios-cd.bin -no-emul-boot -boot-load-size 4 -boot-info-table --efi-boot limine-uefi-cd.bin -efi-boot-part --efi-boot-image --protective-msdos-label {isoRoot} -o {proj.OutputISOFile}");
            i.RedirectStandardOutput = true;

            Process.Start(i).WaitForExit();

            Console.WriteLine($"[LIMINE-DEPLOY] {proj.OutputISOFile}");
            i = new(Path.Combine(liminePath, "limine"), $"bios-install {proj.OutputISOFile}");
            i.RedirectStandardOutput = true;

            Process.Start(i).WaitForExit();
        }
    }
}