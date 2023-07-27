using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NET.Sdk.Embedded
{
    public class PackCabTask : ToolTask
    {
        protected override string ToolName => @"cabwiz.exe";

        public ITaskItem[] CabFiles { get; set; }

        public string CabOutputPath { get; set; }

        public string CabCompany { get; set; }

        public string CabAppName { get; set; }

        public string CabVersion { get; set; }

        public string FastStorageRootDirName { get; set; }

        protected override string GenerateFullPathToTool()
        {
            return string.Empty;
        }

        public override bool Execute()
        {
            var cabInf = new Dictionary<string, IDictionary<string, string>>
            {
                ["Version"] = new Dictionary<string, string>
                {
                    ["Signature"] = "\"$Windows NT$\"",
                    ["Provider"] = $"\"{CabCompany}\"",
                    ["CESignature"] = "\"$Windows CE$\""
                },
                ["CEStrings"] = new Dictionary<string, string>
                {
                    ["AppName"] = $"\"{CabAppName}\"",
                    ["InstallDir"] = "\"%CE1%\\%Manufacturer%\\%AppName%\""
                },
                ["Strings"] = new Dictionary<string, string>
                {
                    ["Manufacturer"] = $"\"{CabCompany}\"",
                    ["FastStorageDir"] = $"\"{FastStorageRootDirName}\\{CabCompany}\\{CabAppName}\""
                },
                ["SourceDisksNames"] = new Dictionary<string, string>(),
                ["SourceDisksFiles"] = new Dictionary<string, string>(),
                ["DestinationDirs"] = new SortedDictionary<string, string>(new DestinationDirectoriesComparer()),
                ["DefaultInstall"] = new Dictionary<string, string>(),
                ["Shortcuts"] = new Dictionary<string, string>()
            };

            var sourceFilesGroups = CabFiles
                .GroupBy(item => new
                {
                    CabPath = item.GetMetadata("CabPath"),
                    SourcePath = new FileInfo(item.GetMetadata("FullPath")).DirectoryName,
                    AddToFastStorage = item.GetMetadata("AddToFastStorage")
                        .Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase)
                }, item => new
                {
                    FileInfo = new FileInfo(item.GetMetadata("FullPath")),
                    AddToStartup = item.GetMetadata("AddToStartup")
                        .Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase),
                    AddToPrograms = item.GetMetadata("AddToPrograms")
                        .Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase),
                    AddToFavorites = item.GetMetadata("AddToFavorites")
                        .Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();

            var sourceDiskId = 1;
            foreach (var sourceFilesGroup in sourceFilesGroups)
            {
                cabInf["SourceDisksNames"][$"{sourceDiskId}"] =
                    $",\"{sourceDiskId}\",,\"{sourceFilesGroup.Key.SourcePath}\"";

                cabInf["DefaultInstall"]["CopyFiles"] = !cabInf["DefaultInstall"].ContainsKey("CopyFiles")
                    ? $"Files.{sourceDiskId}"
                    : string.Join(",", cabInf["DefaultInstall"]["CopyFiles"], $"Files.{sourceDiskId}");

                cabInf["DestinationDirs"][$"Files.{sourceDiskId}"] = 
                    $"0,\"{Path.Combine(sourceFilesGroup.Key.AddToFastStorage ? "%FastStorageDir%" : "%InstallDir%", sourceFilesGroup.Key.CabPath)}\"";
                
                var filesSection = cabInf[$"Files.{sourceDiskId}"] = new Dictionary<string, string>();

                foreach (var sourceFile in sourceFilesGroup)
                {
                    cabInf["SourceDisksFiles"][$"\"{sourceFile.FileInfo.Name}\""] = $"{sourceDiskId}";

                    filesSection[$"\"{sourceFile.FileInfo.Name}\",\"{sourceFile.FileInfo.Name}\",,0"] = string.Empty;

                    if (sourceFile.AddToStartup)
                    {
                        cabInf["Shortcuts"][$"\"{CabAppName}\",0,\"{sourceFile.FileInfo.Name}\",\"%CE4%\""] =
                            string.Empty;
                        cabInf["DestinationDirs"]["Shortcuts"] = "0,\"%CE11%\"";
                        cabInf["DefaultInstall"]["CEShortcuts"] = "Shortcuts";
                    }

                    if (sourceFile.AddToPrograms)
                    {
                        cabInf["Shortcuts"][$"\"{CabAppName}\",0,\"{sourceFile.FileInfo.Name}\",\"%CE11%\""] =
                            string.Empty;
                        cabInf["DestinationDirs"]["Shortcuts"] = "0,\"%CE11%\"";
                        cabInf["DefaultInstall"]["CEShortcuts"] = "Shortcuts";
                    }

                    if (sourceFile.AddToFavorites)
                    {
                        cabInf["Shortcuts"][$"\"{CabAppName}\",0,\"{sourceFile.FileInfo.Name}\",\"%CE17%\""] =
                            string.Empty;
                        cabInf["DestinationDirs"]["Shortcuts"] = "0,\"%CE11%\"";
                        cabInf["DefaultInstall"]["CEShortcuts"] = "Shortcuts";
                    }
                }

                sourceDiskId++;
            }

            var builder = cabInf.Aggregate(new StringBuilder(), (stringBuilder, section) =>
            {
                if (section.Value.Count != 0)
                {
                    stringBuilder.AppendLine($"[{section.Key}]");
                    foreach (var valuePair in section.Value)
                        stringBuilder.AppendLine(string.IsNullOrEmpty(valuePair.Value)
                            ? valuePair.Key
                            : $"{valuePair.Key}={valuePair.Value}");
                }

                return stringBuilder;
            });

            File.WriteAllText(Path.Combine(Directory.CreateDirectory(CabOutputPath).FullName, $"{CabAppName}.inf"),
                builder.ToString());

            return base.Execute();
        }

        protected override string GenerateCommandLineCommands()
        {
            var output = Directory.CreateDirectory(CabOutputPath);

            return $"\"{Path.Combine(output.FullName, $"{CabAppName}.inf")}\" /dest \"{output.FullName}\"";
        }

        private class DestinationDirectoriesComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var result = StringComparer.OrdinalIgnoreCase.Compare(x, y);
                if (result != 0 && ("Shortcuts".Equals(x, StringComparison.OrdinalIgnoreCase) ||
                                    "Shortcuts".Equals(y, StringComparison.OrdinalIgnoreCase)))
                {
                    if ("Shortcuts".Equals(x, StringComparison.OrdinalIgnoreCase))
                        return -1;

                    if ("Shortcuts".Equals(y, StringComparison.OrdinalIgnoreCase))
                        return 1;
                }

                return result;
            }
        }
    }
}