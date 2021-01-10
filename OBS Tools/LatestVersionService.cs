using RiotSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OBS_Tools
{
    public class LatestVersionService
    {
        public string Basepath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))), @"TextFiles\");
        public RiotApi Api = RiotApi.GetDevelopmentInstance(GlobalValues.RiotAPIKey, 20, 100);

        public string GetLatestVersion()
        {
            string latestVersionFromFile = File.ReadAllText(Basepath + "Latest_Version.txt");
            var versions = Api.StaticData.Versions.GetAllAsync().Result;

            if (versions[0] != latestVersionFromFile)
                File.WriteAllText(Basepath + "Latest_Version.txt", versions[0]);

            return versions[0];
        }
    }
}
