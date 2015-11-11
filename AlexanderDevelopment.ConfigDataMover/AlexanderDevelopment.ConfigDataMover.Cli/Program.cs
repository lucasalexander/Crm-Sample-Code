// --------------------------------------------------------------------------------------------------------------------
// Program.cs
//
// Copyright 2015 Lucas Alexander
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlexanderDevelopment.ConfigDataMover.Lib;
using System.Xml;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace AlexanderDevelopment.ConfigDataMover.Cli
{
    class Program
    {
        static string _sourceString = null;
        static string _targetString = null;
        static bool _mapBaseBu = false;
        static bool _mapBaseCurrency = false;
        static List<GuidMapping> _guidMappings = new List<GuidMapping>();
        static List<JobStep> _jobSteps = new List<JobStep>();

        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // consume Options instance properties
                if (options.Verbose)
                {
                    Console.WriteLine("Config file: {0}", options.ConfigFile);
                    Console.WriteLine("Source connection string: {0}", options.Source);
                    Console.WriteLine("Target connection string: {0}", options.Target);
                }

                //parse the config file
                ParseConfig(options.ConfigFile);

                //set source/target connection from parameters if specified - this will overwrite connections from the config file
                if (!string.IsNullOrEmpty(options.Source))
                {
                    _sourceString = options.Source;
                }
                if (!string.IsNullOrEmpty(options.Target))
                {
                    _targetString = options.Target;
                }

                //do some basic validations
                if (string.IsNullOrEmpty(_sourceString))
                {
                    Console.WriteLine("no source connection specified - exiting");
                    return;
                }
                if (string.IsNullOrEmpty(_targetString))
                {
                    Console.WriteLine("no target connection specified - exiting");
                    return;
                }
                if (!(_jobSteps.Count > 0))
                {
                    Console.WriteLine("no steps in job - exiting");
                    return;
                }

                Importer importer = new Importer();
                importer.GuidMappings = _guidMappings;
                importer.JobSteps = _jobSteps;
                importer.SourceString = _sourceString;
                importer.TargetString = _targetString;
                importer.MapBaseBu = _mapBaseBu;
                importer.MapBaseCurrency = _mapBaseCurrency;
                importer.Process();

                int errorCount = importer.ErrorCount;

                importer = null;
                
                //show a message to the user
                if (errorCount == 0)
                {
                    Console.WriteLine("Job finished with no errors.");
                }
                else
                {
                    Console.WriteLine("Job finished with errors. See the RecordError.log file for more details.");
                }
            }
        }

        static void ParseConfig(string filepath)
        {
            StreamReader sr = new StreamReader(filepath);
            string jobdata = (sr.ReadToEnd());
            sr.Close();

            XmlDocument xml = new XmlDocument();
            try
            {
                xml.LoadXml(jobdata);
                _jobSteps.Clear();
                _guidMappings.Clear();

                XmlNodeList stepList = xml.GetElementsByTagName("Step");
                foreach (XmlNode xn in stepList)
                {
                    JobStep step = new JobStep();
                    step.StepName = xn.SelectSingleNode("Name").InnerText;
                    step.StepFetch = xn.SelectSingleNode("Fetch").InnerText;
                    step.UpdateOnly = Convert.ToBoolean(xn.Attributes["updateOnly"].Value);

                    _jobSteps.Add(step);
                }

                XmlNodeList configData = xml.GetElementsByTagName("JobConfig");
                _mapBaseBu = Convert.ToBoolean(configData[0].Attributes["mapBuGuid"].Value);
                _mapBaseCurrency = Convert.ToBoolean(configData[0].Attributes["mapCurrencyGuid"].Value);

                XmlNodeList mappingList = xml.GetElementsByTagName("GuidMapping");
                foreach (XmlNode xn in mappingList)
                {
                    Guid sourceGuid = new Guid(xn.Attributes["source"].Value);
                    Guid targetGuid = new Guid(xn.Attributes["target"].Value);
                    _guidMappings.Add(new GuidMapping { sourceId = sourceGuid, targetId = targetGuid });
                }
                XmlNodeList connectionNodes = xml.GetElementsByTagName("ConnectionDetails");
                if (connectionNodes.Count > 0)
                {
                    _sourceString = connectionNodes[0].Attributes["source"].Value;
                    _targetString = connectionNodes[0].Attributes["target"].Value;
                    //Console.WriteLine(connectionNodes[0].InnerText);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Could not parse job configuration data in {0} - exiting", filepath));
            }
        }
    }
}
