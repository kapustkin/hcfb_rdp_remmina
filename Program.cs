
using System.Diagnostics;

namespace hcfb.rdp.remmina
{
    class Programm
    {
        private const string _tmpFileName = "/tmp/tmp.remmina";

        public static void Main(string[] args)
        {
            try
            {
                var sourceFile = GetFileContent(Directory.GetFiles(args[0], "*.rdp").FirstOrDefault());

                var (shellWorkDir, loadbalanceinfo) = GetSourceData(sourceFile);

                var resultFile = Template.Get.
                Replace("@loadbalanceinfo", loadbalanceinfo).
                Replace("@execpath", shellWorkDir);

                SaveTmpFile(resultFile);

                Console.WriteLine("Tmp file save, try exec Remmina " + $"{_tmpFileName}");

                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "remmina",
                    Arguments = $"{_tmpFileName}",
                };
                Process proc = new Process() { StartInfo = startInfo, };
                proc.Start();

                Console.WriteLine("Clean up");

                foreach (var file in Directory.GetFiles(args[0], "*.rdp"))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Args: {string.Join(";", args)}");
                Console.WriteLine($"Exception: {ex}");

                if (args.Length > 1 && args[1] == "debug")
                {
                    using (StreamWriter writer = new StreamWriter("error.log"))
                    {
                        writer.WriteLine($"Args: {string.Join(";", args)}");
                        writer.WriteLine($"Exception: {ex}");
                        writer.Close();
                    }
                }
            }
        }

        private static string GetFileContent(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException("File not found");
            }

            using (StreamReader reader = new StreamReader(file))
            {
                return reader.ReadToEnd();
            }
        }

        private static (string?, string?) GetSourceData(string sourceFile)
        {
            var parsed = sourceFile.Split(Environment.NewLine);

            var shellWorkDir = parsed.FirstOrDefault(s => s.StartsWith("shell working directory"))?.Split(":")[2];
            var loadbalanceinfo = parsed.FirstOrDefault(s => s.StartsWith("loadbalanceinfo"))?.Split(":")[2];

            return (shellWorkDir, loadbalanceinfo);
        }

        private static void SaveTmpFile(string data)
        {
            using (StreamWriter writer = new StreamWriter("/tmp/tmp.remmina"))
            {
                writer.Write(data);
            }
        }

        private static void CleanUp()
        {
            if (File.Exists(_tmpFileName))
            {
                File.Delete(_tmpFileName);
            }
        }
    }
}