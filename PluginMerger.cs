using System.Collections.Generic;

namespace Hunt
{
    class PluginMerger
    {
        static void Main(string[] args)
        {
            string basePath = @"C:\Users\PedroIvo\Documents\Visual Studio 2015\Projects\Oxide.Hunt";
            var outputPath = new List<string>();
            outputPath.Add(@"C:\Users\PedroIvo\Documents\Visual Studio 2015\Projects\Oxide\Oxide.Ext.Rust\Plugins");
            outputPath.Add(@"C:\Rust Dev\devserver\server\hunt_mechs_server\oxide\plugins");
            FileReader fr = new FileReader(basePath, outputPath, "HuntPlugin.cs");
            fr.AddExcludedFiles(new List<string>() {"FileReader.cs","PluginMerger.cs"});
            fr.MergeDirectory();
        }
    }
}
