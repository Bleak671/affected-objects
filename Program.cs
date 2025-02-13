using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace affected_objects
{
    internal class Program
    {
        static Dictionary<string, string> opMapping = new Dictionary<string, string>()
        {
            { "M","[*]" },
            { "A","[+]" },
            { "D","[-]" }
        };
        static Dictionary<string, string> fileMapping = new Dictionary<string, string>()
        {
            { ".js","js:" },
            { ".cs","cs:" },
            { "/data.json","data:" },
            { ".sql","sql:" }
        };
        static void Main(string[] args)
        {
            var repoPath = args[0];
            var sha = args[1];

            Process process = new Process();
            process.StartInfo.WorkingDirectory = repoPath;
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = $"show --name-status {sha}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            StreamReader reader = process.StandardOutput;
            string output = reader.ReadToEnd();

            var strings = output.Split(new char[] { '\n', });
            Dictionary<string, List<string>> subResult = new Dictionary<string, List<string>>();
            var result = "";
            foreach (string s in strings)
            {
                var textArr = s.Split('/');
                if (s.EndsWith(".js") || s.EndsWith(".cs") || s.EndsWith(".sql") || s.EndsWith("/data.json"))
                {
                    if (!subResult.Keys.Any(e => e == textArr[1]))
                    {
                        subResult.Add(textArr[1], new List<string>());
                    }

                    string opString = "";
                    foreach (var op in opMapping.Keys)
                    {
                        if (s.StartsWith(op))
                        {
                            opString = opMapping[op];
                        }
                    }
                    string fileStringK = "";
                    string fileStringV = "";
                    string name = "";
                    foreach (var fileExt in fileMapping.Keys)
                    {
                        if (s.EndsWith(fileExt))
                        {
                            fileStringK = fileExt;
                            fileStringV = fileMapping[fileExt];
                            if (fileStringV == "data:")
                            {
                                name = textArr[textArr.Length - 2];
                            }
                            else
                            {
                                name = textArr[textArr.Length - 1].Replace(fileStringK, "");
                            }
                        }
                    }

                    subResult[textArr[1]].Add($"{fileStringV} *{name}* {opString}\n");
                }
            }

            foreach (string pkg in subResult.Keys)
            {
                result += $"pkg: *{pkg}*\n";
                foreach (string file in subResult[pkg]) 
                {
                    result += file;
                }
            }

            Console.WriteLine(result);

            process.WaitForExit();
            process.Close();

            Console.ReadKey();
        }
    }
}
