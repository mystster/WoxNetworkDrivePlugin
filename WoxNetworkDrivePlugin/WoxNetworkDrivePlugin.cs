/*
 
The MIT License (MIT)

Copyright (c) 2019 mystster

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Wox.Plugin;
using System.Diagnostics;

namespace WoxNetworkDrivePlugin
{
    public class WoxNetworkDrivePlugin : IPlugin
    {
        private static readonly List<Result> networkDrives = [];

        public string Name => "NetworkDrivePlugin";

        public string Description => "NetworkDrivePlugin";
        public static string PluginID => "7CEF9A17BC1E428D997479F006AC8479";

        public void Init(PluginInitContext context)
        {
            Wox.Plugin.Logger.Log.Info("Start NetworkDrivePlugin init process.", MethodBase.GetCurrentMethod().DeclaringType);

            CreateOrUpdateNetworkDriveList();
        }

        private static void CreateOrUpdateNetworkDriveList()
        {
            networkDrives.RemoveAll(_ => true);
            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                if (drive.DriveType == System.IO.DriveType.Network)
                {
                    foreach (var (result, index) in ExecCmd($"net use {drive.Name.Replace("\\", "")}").Select((item, index) => (item, index)))
                    {
                        if (index == 1)
                        {
                            Wox.Plugin.Logger.Log.Info($"add NetworkDrive {drive.Name}->{result.Split(' ').Last()}", MethodBase.GetCurrentMethod().DeclaringType);
                            string label;
                            try
                            {
                                label = drive.VolumeLabel;
                            }
                            catch(Exception ex)
                            {
                                Wox.Plugin.Logger.Log.Warn($"Add {drive.Name} is error:{ex.Message}", MethodBase.GetCurrentMethod().DeclaringType);
                                label = "";
                            }
                            networkDrives.Add(
                                new Result()
                                {
                                    Title = $"{drive.Name}[{label}]",
                                    IcoPath = "Images\\icon.png",
                                    SubTitle = result.Split(' ').Last(),
                                    Action = ac =>
                                    {
                                        Wox.Plugin.Logger.Log.Info($"copy to clipboard:{result.Split(' ').Last()}", MethodBase.GetCurrentMethod().DeclaringType);
                                        ExecCmd($"set /P =\"{result.Split(' ').Last()}\" < nul | clip");
                                        return true;
                                    }
                                });
                        }
                    }

                }
            }
            networkDrives.Add(
                new Result()
                {
                    Title = "[Reflesh Network Drive List]",
                    IcoPath = "Images\\icon.png",
                    Action = ac =>
                    {
                        CreateOrUpdateNetworkDriveList();
                        return true;
                    }
                });
            Wox.Plugin.Logger.Log.Info($"NetworkDrive is {networkDrives.Count}", MethodBase.GetCurrentMethod().DeclaringType);
        }

        public List<Result> Query(Query query)
        {
            if (string.IsNullOrEmpty(query?.Search))
            {
                return networkDrives;
            }
            else
            {
                return networkDrives
                .Where(x => x.Title.Contains(query?.Search, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
            }
        }

        private static string[] ExecCmd(string command)
        {
            var p = new Process();

            p.StartInfo.FileName = Environment.GetEnvironmentVariable("ComSpec");
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = $"/c {command}";

            p.Start();

            var results = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
            p.Close();

            return results.Split('\n');
        }
    }
}
