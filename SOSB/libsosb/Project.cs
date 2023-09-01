using LibSOSB.Types;
using Newtonsoft.Json;

namespace LibSOSB;

/// <summary>
/// A SOSB project
/// </summary>
public class Project
{
    public string Name = "name";
    public string Type = "limine";
    public string BootloaderConfigPath = "kernel/limine.cfg";
    public string KernelSourceDir = "kernel/src";
    public string KernelBinDir = "kernel/bin";
    public string KernelExecutable = "kernel/bin/kernel.elf";
    public string KernelLinkFile = "kernel/link.ld";
    public string KernelObjectDir = "kernel/obj";
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
    public string OutputISOFile = "kernel/bin/kernel.iso";

    public ProjectLanguage Language = ProjectLanguage.StandardC;
}