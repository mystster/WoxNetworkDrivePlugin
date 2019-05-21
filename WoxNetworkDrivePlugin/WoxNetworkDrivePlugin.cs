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

using Kolibri;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using Wox.Plugin;

namespace WoxNetworkDrivePlugin
{
    public class WoxNetworkDrivePlugin : IPlugin
    {
        private static readonly List<Result> networkDrives = new List<Result>();


        public void Init(PluginInitContext context)
        {
            CreateOrUpdateNetworkDriveList();

        }

        private static void CreateOrUpdateNetworkDriveList()
        {
            networkDrives.RemoveAll(_ => true);
            var searcher = new ManagementObjectSearcher(
            "root\\CIMV2",
            "SELECT * FROM Win32_MappedLogicalDisk");

            foreach (var item in searcher.Get())
            {
                networkDrives.Add(
                    new Result()
                    {
                        Title = $"{item["Name"]} [{item["VolumeName"]}]",
                        IcoPath = "Images\\icon.png",
                        SubTitle = $"{item["ProviderName"]}",
                        Action = ac =>
                        {
                            Clippy.PushUnicodeStringToClipboard($"{item["ProviderName"]}");
                            return true;
                        }
                    });
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
        }

        public List<Result> Query(Query query)
        {
            if (string.IsNullOrEmpty(query.FirstSearch))
            {
                return networkDrives;
            }
            else
            {
                return networkDrives
                .Where(x => x.Title.ToLower().Contains(query.FirstSearch.ToLower()))
                .ToList();
            }
        }
    }
}
