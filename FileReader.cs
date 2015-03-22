using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hunt
{
    public class FileReader
    {
        private const string StartNamespace = "{";
        private const string EndNamespace = "}";
        private readonly StringBuilder MainClass;
        private readonly List<string> ExcludedFiles;
        private List<string> ExcludedDirs;
        private readonly List<string> References;
        private readonly Dictionary<string, StringBuilder> NamespaceMergedContent;
        private readonly string BasePath;
        private readonly List<string> OutputPath;
        private readonly string Extension;
        private readonly string OutputFileName;

        public FileReader(string basePath, List<string> outputPath, string outputFileName)
        {
            BasePath = basePath;
            Extension = "*.cs";
            NamespaceMergedContent = new Dictionary<string, StringBuilder>();
            MainClass = new StringBuilder();
            References = new List<string>();
            ExcludedFiles = new List<string> {"AssemblyInfo.cs"};
            ExcludedDirs = new List<string>() {"obj"};
            OutputPath = outputPath;
            OutputFileName = outputFileName;
        }

        public void AddExcludedFiles(List<string> excludedFilesToAdd)
        {
            ExcludedFiles.AddRange(excludedFilesToAdd);
        }

        private void GetAllFilesOfTypeInDirectoryRecursivly(string directoryPath, List<string> fileList)
        {
            try
            {
                foreach (string file in Directory.GetFiles(directoryPath, Extension))
                {
                    var fileName = Path.GetFileName(file);
                    if (ExcludedFiles.Contains(fileName))
                        continue;
                    fileList.Add(file);
                }
                foreach (string directory in Directory.GetDirectories(directoryPath))
                {
                    var directoryName = new DirectoryInfo(directory).Name;
                    if(ExcludedDirs.Contains(directoryName))
                        continue;
                    GetAllFilesOfTypeInDirectoryRecursivly(directory, fileList);
                }
            }
            catch (Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        public void MergeDirectory()
        {
            var fileList = new List<string>();
            GetAllFilesOfTypeInDirectoryRecursivly(BasePath, fileList);
            foreach (var file in fileList)
            {
                ReadFile(file);
            }
            foreach (var outpath in OutputPath)
            {
                using (var outfile = new StreamWriter(Path.Combine(outpath, OutputFileName)))
                {
                    outfile.WriteLine("// Reference: Oxide.Ext.Rust");
                    outfile.WriteLine("// Reference: Newtonsoft.Json");
                    foreach (var reference in References)
                        outfile.WriteLine("using {0}", reference);
                    outfile.WriteLine();
                    outfile.WriteLine("namespace Oxide.Plugins");
                    outfile.WriteLine(StartNamespace);
                    outfile.Write(MainClass.ToString());
                    outfile.WriteLine(EndNamespace);
                    outfile.WriteLine();
                    foreach (var namespaceContent in NamespaceMergedContent)
                    {
                        outfile.WriteLine("namespace {0}", namespaceContent.Key);
                        outfile.WriteLine(StartNamespace);
                        outfile.Write(namespaceContent.Value.ToString());
                        outfile.WriteLine(EndNamespace);
                        outfile.WriteLine();
                    }
                }


            }
        }

        private void ReadFile(string filePath)
        {
            var file = new StreamReader(filePath);
            string line;
            List<string> fileContent = new List<string>();
            string namespaceLine = "";
            while ((line = file.ReadLine()) != null)
            {
                var isReferenceLine = line.StartsWith("using");
                var strings = line.Split(' ');
                if (isReferenceLine)
                {
                    var referenceLine = "";
                    for (int i = 1; i < strings.Length; i++)
                        referenceLine += strings[i];
                    if (!References.Contains(referenceLine))
                        References.Add(referenceLine);
                    continue;
                }
                    
                var isNamespaceLine = line.StartsWith("namespace");
                if (isNamespaceLine)
                {
                    namespaceLine = strings[1];
                    continue;
                }
                var isBrackAtStart = line.StartsWith(StartNamespace) || line.StartsWith(EndNamespace);
                if (isBrackAtStart)
                    continue;
                fileContent.Add(line);
            }
            if (namespaceLine.Contains("Oxide.Plugins"))
            {
                foreach (var fileLine in fileContent)
                    MainClass.AppendLine(fileLine);
            }
            else
            {
                if (!NamespaceMergedContent.ContainsKey(namespaceLine))
                    NamespaceMergedContent.Add(namespaceLine, new StringBuilder());
                var sb = NamespaceMergedContent[namespaceLine];
                foreach (var fileLine in fileContent)
                    sb.AppendLine(fileLine);
            }
            file.Close();
        }
    }
}