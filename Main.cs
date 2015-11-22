using System;
using System.IO;
using System.Text;

namespace AssemblyInfoUtil
{
    class AssemblyInfoUtil
    {
        private static int _incParamNum = 0;

        private static string _fileName = "";
        
        private static string _versionStr = null;

        private static bool _isVb = false;

        [STAThread]
        static void Main(string[] args)
        {
            foreach (var t in args)
            {
                if (t.StartsWith("-inc:")) {
                    var s = t.Substring("-inc:".Length);
                    _incParamNum = int.Parse(s);
                } else if (t.StartsWith("-set:")) {
                    _versionStr = t.Substring("-set:".Length);
                } else {
                    _fileName = t;
                }
            }

            if (Path.GetExtension(_fileName).ToLower() == ".vb")
                _isVb = true;

            if (_fileName == "") {
                Console.WriteLine("Usage: AssemblyInfoUtil <path to AssemblyInfo.cs or AssemblyInfo.vb file> [options]");
                Console.WriteLine("Options: ");
                Console.WriteLine("  -set:<new version number> - set new version number (in NN.NN.NN.NN format)");
                Console.WriteLine("  -inc:<parameter index>  - increases the parameter with specified index (can be from 1 to 4)");
                return;
            }

            if (!File.Exists(_fileName)) {
                Console.WriteLine("Error: Can not find file \"" + _fileName + "\"");
                return;
            }

            Console.Write("Processing \"" + _fileName + "\"...");
            SetReadOnly(_fileName, false);
            //var fileInfo = new FileInfo(_fileName) {IsReadOnly = false};
            var reader = new StreamReader(_fileName);
            var writer = new StreamWriter(_fileName + ".out");
            string line;

            while ((line = reader.ReadLine()) != null) {
                line = ProcessLine(line);
                writer.WriteLine(line);
            }
            reader.Close();
            writer.Close();

            File.Delete(_fileName);
            File.Move(_fileName + ".out", _fileName);
            //SetReadOnly(_fileName, true);
            Console.WriteLine("Done!");
        }

        private static void SetReadOnly(string fileName, bool toBeReadOnly)
        {
            var attr = File.GetAttributes(fileName);
            attr = toBeReadOnly ? attr | FileAttributes.ReadOnly : attr & ~FileAttributes.ReadOnly;
            File.SetAttributes(fileName, attr);
        }


        private static string ProcessLine(string line) {
            if (_isVb) {
                line = ProcessLinePart(line, "<Assembly: AssemblyVersion(\"");
                line = ProcessLinePart(line, "<Assembly: AssemblyFileVersion(\"");
            } else {
                line = ProcessLinePart(line, "[assembly: AssemblyVersion(\"");
                line = ProcessLinePart(line, "[assembly: AssemblyFileVersion(\"");
            }
            return line;
        }

        private static string ProcessLinePart(string line, string part) {
            var spos = line.IndexOf(part, StringComparison.Ordinal);
            if (spos >= 0) {
                spos += part.Length;
                var epos = line.IndexOf('"', spos);
                var oldVersion = line.Substring(spos, epos - spos);
                var newVersion = "";
                var performChange = false;

                if (_incParamNum > 0) {
                    var nums = oldVersion.Split('.');
                    if (nums.Length >= _incParamNum && nums[_incParamNum - 1] != "*") {
                        var val = long.Parse(nums[_incParamNum - 1]);
                        val++;
                        nums[_incParamNum - 1] = val.ToString();
                        newVersion = nums[0]; 
                        for (var i = 1; i < nums.Length; i++) {
                            newVersion += "." + nums[i];
                        }
                        performChange = true;
                    }

                }
                else if (_versionStr != null) {
                    newVersion = _versionStr;
                    performChange = true;
                }

                if (performChange) {
                    var str = new StringBuilder(line);
                    str.Remove(spos, epos - spos);
                    str.Insert(spos, newVersion);
                    line = str.ToString();
                }
            } 
            return line;
        }
    }
}