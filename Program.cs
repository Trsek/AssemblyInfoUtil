using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssemblyInfoUtil
{
    /// <summary>
    /// Summary description for AssemblyInfoUtil.
    /// </summary>
    public class AssemblyInfoUtil
    {
        private static int incParamNum = 0;
        private static string fileName = "";
        private static string versionStr = null;
        private static string saveFile = null;
        private static string droidFile = null;
        private static bool isNew = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            droidFile = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-inc:"))
                {
                    string s = args[i].Substring("-inc:".Length);
                    incParamNum = int.Parse(s);
                }
                else if (args[i].StartsWith("-set:"))
                {
                    versionStr = args[i].Substring("-set:".Length);
                }
                else if (args[i].StartsWith("-new"))
                {
                    isNew = true;
                }
                else if (args[i].StartsWith("-save:"))
                {
                    saveFile = args[i].Substring("-save:".Length);
                }
                else if (args[i].StartsWith("-droid:"))
                {
                    droidFile = args[i].Substring("-droid:".Length);
                }
                else
                {
                    fileName = args[i];
                }
            }

            if (isNew && !IsChangeFiles(fileName))
            {
                Console.WriteLine("Nothing change in source.");
                return;
            }

            if (fileName == "")
            {
                Console.WriteLine("Usage: AssemblyInfoUtil path to AssemblyInfo.cs file> [options]");
                Console.WriteLine("Options: ");
                Console.WriteLine("  -set:<new version number> - set new version number (in NN.NN.NN.NN format)");
                Console.WriteLine("  -inc:<parameter index> - increases the parameter with specified index (can be from 1 to 4)");
                Console.WriteLine("  -save:<external file> - save increases number to external file");
                Console.WriteLine("  -droid:<android manifest file> - version store to android manifest file");
                Console.WriteLine("  -new - increases only when change source file");
                return;
            }

            if (!File.Exists(fileName))
            {
                Console.WriteLine("Error: Can not find file \"" + fileName + "\"");
                return;
            }

            Console.Write("Processing \"" + fileName + "\"...");

            List<string> lines = new List<string>();

            foreach(string line in File.ReadAllLines(fileName))
            {
                lines.Add(ProcessLine(line));
            }

            File.Delete(fileName);
            File.WriteAllLines(fileName, lines.ToArray());
            Console.WriteLine("Done!");
        }

        private static string ProcessLine(string line)
        {
            line = ProcessLinePart(line, "[assembly: AssemblyVersion(\"");
            line = ProcessLinePart(line, "[assembly: AssemblyFileVersion(\"");
            return line;
        }

        private static void ChangeManifest(string xmlFile, string version)
        {
            string[] lines = File.ReadAllLines(xmlFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("android:versionCode"))
                {
                    string line = string.Empty;

                    foreach (string variable in lines[i].Split(' '))
                    {
                        if (variable.Contains("android:versionCode"))
                        {
                            line += "android:versionCode=\"" + version.Split('.')[0] + "\" ";
                        }
                        else if (variable.Contains("android:versionName"))
                        {
                            line += "android:versionName=\"" + version + "\" ";
                        }
                        else
                        {
                            line += variable + " ";
                        }
                    }
                    lines[i] = line.TrimEnd();
                }
            }

            File.WriteAllLines(xmlFile, lines);
        }

        private static bool IsChangeFiles(string fileName)
        {
            DirectoryInfo dirf = new DirectoryInfo(Path.GetDirectoryName(fileName) + "\\..");
            DateTime lastTime = new DateTime(0);
            FileInfo assemblyFile = new FileInfo(fileName);

            foreach (FileInfo nfo in dirf.GetFiles())
            {
                if (nfo.LastWriteTime > lastTime)
                    lastTime = nfo.LastWriteTime;
            }

            if (assemblyFile.LastWriteTime < lastTime)
                return true;

            return false;
        }

        private static string ProcessLinePart(string line, string part)
        {
            int spos = line.IndexOf(part);
            if (spos >= 0)
            {
                spos += part.Length;
                int epos = line.IndexOf('"', spos);
                string oldVersion = line.Substring(spos, epos - spos);
                string newVersion = "";
                bool performChange = false;

                if (incParamNum > 0)
                {
                    string[] nums = oldVersion.Split('.');
                    if (nums.Length >= incParamNum && nums[incParamNum - 1] != "*")
                    {
                        Int64 val = Int64.Parse(nums[incParamNum - 1]);
                        val++;
                        nums[incParamNum - 1] = val.ToString();
                        newVersion = nums[0];
                        for (int i = 1; i < nums.Length; i++)
                        {
                            newVersion += "." + nums[i];
                        }
                        performChange = true;
                    }
                }
                else if (versionStr != null)
                {
                    newVersion = versionStr;
                    performChange = true;
                }

                if (performChange)
                {
                    StringBuilder str = new StringBuilder(line);
                    str.Remove(spos, epos - spos);
                    str.Insert(spos, newVersion);
                    line = str.ToString();
                    if (saveFile != null)
                        File.WriteAllText(saveFile, "Release " + newVersion, Encoding.ASCII);
                    if (droidFile != null)
                        ChangeManifest(droidFile, newVersion);
                }
            }
            return line;
        }
    }
}