using System;
using System.IO;
using System.Net;
using System.Diagnostics;

class Program
{
    static string hashFile = "hash.txt";
    static string wordlist = "top1000.txt";
    static string rulesFile = "rules/best64.rule";

    static void Main()
    {
        // ✅ CHECK HASHCAT INSTALLATO
        if (!IsHashcatAvailable())
        {
            Console.WriteLine("Hashcat non trovato. Installalo e assicurati che sia nel PATH.");
            return;
        }

        Console.Write("Inserisci hash: ");
        string hash = Console.ReadLine().Trim();

        File.WriteAllText(hashFile, hash);

        PrepareFiles();

        int mode = DetectHashMode(hash);

        if (mode == -1)
        {
            Console.WriteLine("Tipo hash non supportato");
            return;
        }

        Console.WriteLine($"Hash mode: {mode}");

        // 🔥 SMART ATTACK ORDER

        // 1. Top passwords
        RunHashcat($"{mode} -a 0 {hashFile} {wordlist}");

        // 2. Top passwords + rules
        RunHashcat($"{mode} -a 0 {hashFile} {wordlist} -r {rulesFile}");

        // 3. Hybrid leggero
        RunHashcat($"{mode} -a 6 {hashFile} {wordlist} ?d?d");

        // 4. Maschere intelligenti
        RunMasks(mode);

        // 5. Bruteforce pesante
        RunHashcat($"{mode} -a 3 {hashFile} ?l?l?l?l?l?l?d?d");

        Console.WriteLine("Completato");
    }

    // ✅ DOWNLOAD AUTOMATICO FILE NECESSARI
    static void PrepareFiles()
    {
        if (!File.Exists(wordlist))
        {
            Console.WriteLine("Scarico top password...");
            DownloadFile(
                "https://raw.githubusercontent.com/danielmiessler/SecLists/master/Passwords/Common-Credentials/10k-most-common.txt",
                wordlist);
        }

        if (!File.Exists(rulesFile))
        {
            Console.WriteLine("Scarico rules...");
            Directory.CreateDirectory("rules");
            DownloadFile(
                "https://raw.githubusercontent.com/NotSoSecure/password_cracking_rules/refs/heads/master/OneRuleToRuleThemAll.rule",
                rulesFile);
        }
    }

    static void DownloadFile(string url, string output)
    {
        using WebClient client = new();
        client.DownloadFile(url, output);
    }

    // ✅ HASH DETECTION
    static int DetectHashMode(string hash)
    {
        switch (hash.Length)
        {
            case 32: return 0;       // MD5
            case 40: return 100;     // SHA1
            case 64: return 1400;    // SHA256
            case 128: return 1700;   // SHA512
            default: return -1;
        }
    }

    // ✅ MASCHERE INTELLIGENTI
    static void RunMasks(int mode)
    {
        string[] masks = new string[]
        {
            "?d?d?d?d",
            "?l?l?l?l?d?d",
            "?l?l?l?l?l?l",
            "admin?d?d",
            "password?d?d",
            "?u?l?l?l?l?d",
            "?l?l?l?l?l?l?l?l"
        };

        foreach (var mask in masks)
        {
            RunHashcat($"{mode} -a 3 {hashFile} {mask}");
        }
    }

    // ✅ RUN HASHCAT ON GPU
    static void RunHashcat(string args)
    {
        string fullArgs = "-O -w 3 " + args;

        Console.WriteLine($"\n[>] hashcat {fullArgs}");

        Process proc = new Process();
        proc.StartInfo.FileName = "hashcat.exe";
        proc.StartInfo.Arguments = fullArgs;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;

        proc.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine(e.Data);
        };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.WaitForExit();
    }

    static bool IsHashcatAvailable()
    {
        try
        {
            Process proc = new Process();

            // Su Windows usa "hashcat.exe", su Linux/macOS "hashcat"
            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            proc.StartInfo.FileName = isWindows ? "hashcat.exe" : "hashcat";
            proc.StartInfo.Arguments = "--version";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            proc.Start();
            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            return proc.ExitCode == 0;
        }
        catch (Exception)
        {
            // Se il processo non si avvia, il file non esiste
            return false;
        }
    }
}