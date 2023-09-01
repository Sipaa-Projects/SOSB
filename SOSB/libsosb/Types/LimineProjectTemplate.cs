using System.Net;

namespace LibSOSB.Types;

public class LimineAMD64ProjectTemplate 
{
    const string Config = @"TIMEOUT=5

:Kernel
PROTOCOL=limine
KERNEL_PATH=boot:///kernel.elf
";
    const string LinkFile = @"OUTPUT_FORMAT(elf64-x86-64)
OUTPUT_ARCH(i386:x86-64)

ENTRY(kmain)

PHDRS
{
    text    PT_LOAD    FLAGS((1 << 0) | (1 << 2)) ; /* Execute + Read */
    rodata  PT_LOAD    FLAGS((1 << 2)) ;            /* Read only */
    data    PT_LOAD    FLAGS((1 << 1) | (1 << 2)) ; /* Write + Read */
}

SECTIONS
{
    . = 0xffffffff80000000;

    .text : {
        *(.text .text.*)
    } :text

   . += CONSTANT(MAXPAGESIZE);

    .rodata : {
        *(.rodata .rodata.*)
    } :rodata

    . += CONSTANT(MAXPAGESIZE);

    .data : {
        *(.data .data.*)
    } :data

    .bss : {
        *(COMMON)
        *(.bss .bss.*)
    } :data

    /DISCARD/ : {
        *(.eh_frame)
        *(.note .note.*)
    }
}
    ";

    const string Kernel = @"#include <stdint.h>

#include <limine/limine.h>

static volatile struct limine_framebuffer_request fbr = {
    .id = LIMINE_FRAMEBUFFER_REQUEST,
    .revision = 0
};

void kmain()
{
    // Should set the pixel at 0,0 to 0xFFFFFF (white)
    uint32_t *address = fbr.response->framebuffers[0]->address;
    address[0] = 0xFFFFFF;

    // Lock in a while loop
    while (1)
        ;;
}
    ";
    public enum FileType
    {
        LimineBootProtocolHeader,
        Kernel,
        LinkFile,
        ConfigFile
    }

    public static Project GetProject()
    {
        Project p = new();
        p.Type = "limine";
        p.KernelSourceDir = "kernel/src";
        p.KernelBinDir = "kernel/bin";
        p.KernelObjectDir = "kernel/obj";
        p.KernelExecutable = "kernel/bin/kernel.elf";
        p.CPPTarget = "x86_64-elf-";
        p.CTarget = "x86_64-elf-";
        p.CPPCompilerPath = "g++";
        p.CPPCompilerFlags = "-w -Dlimine -std=gnu++11 -ffreestanding -fno-stack-protector -fpermissive -fno-stack-check -fno-lto -fno-PIE -fno-PIC -m64 -Ikernel/src/ -march=x86-64 -mabi=sysv -mno-mmx -mno-sse -mno-sse2 -mno-red-zone -mcmodel=kernel";
        p.CCompilerPath = "gcc";
        p.CCompilerFlags = "-w -Dlimine -std=gnu++11 -ffreestanding -fno-stack-protector -fpermissive -fno-stack-check -fno-lto -fno-PIE -fno-PIC -m64 -Ikernel/src/ -march=x86-64 -mabi=sysv -mno-mmx -mno-sse -mno-sse2 -mno-red-zone -mcmodel=kernel";
        p.LinkerTarget = "x86_64-elf-";
        p.LinkerPath = "ld";
        p.LinkerFlags = "-nostdlib -z max-page-size=0x1000";
        p.AssemblerPath = "nasm";
        p.AssemblerFlags = "-f elf64";
        return p;
    }

    public static string GetFile(FileType t)
    {
        switch (t)
        {
            case FileType.LimineBootProtocolHeader:
                HttpClient c = new();
                Stream s = c.GetStreamAsync("https://raw.githubusercontent.com/limine-bootloader/limine/v5.x-branch/limine.h").Result;
                StreamReader reader = new StreamReader(s);
                return reader.ReadToEnd();
            case FileType.Kernel:
                return Kernel;
            case FileType.LinkFile:
                return LinkFile;
            case FileType.ConfigFile:
                return Config;
            default:
                return "";

        }
    }
}

public class LimineARM64ProjectTemplate 
{
    const string Config = @"TIMEOUT=5

:Kernel
PROTOCOL=limine
KERNEL_PATH=boot:///kernel.elf
";    
    
    const string LinkFile = @"OUTPUT_FORMAT(elf64-littleaarch64)
OUTPUT_ARCH(aarch64)

ENTRY(kmain)

/* Define the program headers we want so the bootloader gives us the right */
/* MMU permissions */
PHDRS
{
    text    PT_LOAD    FLAGS((1 << 0) | (1 << 2)) ; /* Execute + Read */
    rodata  PT_LOAD    FLAGS((1 << 2)) ;            /* Read only */
    data    PT_LOAD    FLAGS((1 << 1) | (1 << 2)) ; /* Write + Read */
    dynamic PT_DYNAMIC FLAGS((1 << 1) | (1 << 2)) ; /* Dynamic PHDR for relocations */
}

SECTIONS
{
    . = 0xffffffff80000000;

    .text : {
        *(.text .text.*)
    } :text

    . += CONSTANT(MAXPAGESIZE);

    .rodata : {
        *(.rodata .rodata.*)
    } :rodata

    . += CONSTANT(MAXPAGESIZE);

    .data : {
        *(.data .data.*)
    } :data

    .dynamic : {
        *(.dynamic)
    } :data :dynamic

    .bss : {
        *(.bss .bss.*)
        *(COMMON)
    } :data

    /DISCARD/ : {
        *(.eh_frame)
        *(.note .note.*)
    }
}
    ";

    const string Kernel = @"#include <stdint.h>

#include <limine/limine.h>

static volatile struct limine_framebuffer_request fbr = {
    .id = LIMINE_FRAMEBUFFER_REQUEST,
    .revision = 0
};

void kmain()
{
    // Should set the pixel at 0,0 to 0xFFFFFF (white)
    uint32_t *address = fbr.response->framebuffers[0]->address;
    address[0] = 0xFFFFFF;

    // Lock in a while loop
    while (1)
        ;;
}
    ";
    public enum FileType
    {
        LimineBootProtocolHeader,
        Kernel,
        LinkFile,
        ConfigFile
    }

    public static Project GetProject()
    {
        Project p = new();
        p.Type = "limine";
        p.KernelSourceDir = "kernel/src";
        p.KernelBinDir = "kernel/bin";
        p.KernelObjectDir = "kernel/obj";
        p.KernelExecutable = "kernel/bin/kernel.elf";
        p.CPPTarget = "aarch64-elf-";
        p.CTarget = "aarch64-elf-";
        p.CPPCompilerPath = "g++";
        p.CPPCompilerFlags = "-w -DAARCH64 -std=gnu++11 -ffreestanding -fno-stack-protector -fpermissive -fno-stack-check -fno-lto -fno-PIE -fno-PIC -Ikernel/src/ -g";
        p.CCompilerPath = "gcc";
        p.CCompilerFlags = "-w -DAARCH64 -std=gnu++11 -ffreestanding -fno-stack-protector -fpermissive -fno-stack-check -fno-lto -fno-PIE -fno-PIC -Ikernel/src/ -g";
        p.LinkerTarget = "aarch64-elf-";
        p.LinkerPath = "gcc";
        p.LinkerFlags = "-nostdlib -ffreestanding -lgcc -O2";
        p.AssemblerPath = "nasm";
        p.AssemblerFlags = "-f elf64";
        return p;
    }

    public static string GetFile(FileType t)
    {
        switch (t)
        {
            case FileType.LimineBootProtocolHeader:
                HttpClient c = new();
                Stream s = c.GetStreamAsync("https://raw.githubusercontent.com/limine-bootloader/limine/v5.x-branch/limine.h").Result;
                StreamReader reader = new StreamReader(s);
                return reader.ReadToEnd();
            case FileType.Kernel:
                return Kernel;
            case FileType.LinkFile:
                return LinkFile;
            case FileType.ConfigFile:
                return Config;
            default:
                return "";

        }
    }
}