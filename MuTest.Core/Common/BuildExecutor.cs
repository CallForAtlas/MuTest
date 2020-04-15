﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MuTest.Core.Common.Settings;
using MuTest.Core.Model.Service;
using Newtonsoft.Json;
using static MuTest.Core.Common.Constants;

namespace MuTest.Core.Common
{
    public class BuildExecutor : IBuildExecutor
    {
        private readonly VSTestConsoleSettings _consoleSettings;
        private readonly string _project;

        public event EventHandler<EventArgs> BuildStarted;
        public event EventHandler<EventArgs> BuildFinished;
        public event EventHandler<string> OutputDataReceived;

        public string BaseAddress { get; set; }

        public bool EnableLogging { get; set; } = true;

        public string OutputPath { get; set; } = string.Empty;

        public string IntermediateOutputPath { get; set; } = string.Empty;

        public BuildExecutionStatus LastBuildStatus { get; private set; }

        public bool QuietWithSymbols { get; set; }

        public BuildExecutor(VSTestConsoleSettings settings, string project)
        {
            if (string.IsNullOrWhiteSpace(project))
            {
                throw new ArgumentNullException(nameof(project));
            }

            _project = project;
            _consoleSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task ExecuteBuildInDebugModeWithoutDependencies()
        {
            var projectBuilder = new StringBuilder($@"""{_project}""")
                .Append(_consoleSettings.MSBuildDependenciesOption);

            await Build(projectBuilder);
        }

        public async Task ExecuteBuildInDebugModeWithDependencies()
        {
            await Build(new StringBuilder($@"""{_project}"""));
        }

        public async Task ExecuteBuildInReleaseModeWithDependencies()
        {
            var projectBuilder = new StringBuilder($@"""{_project}""")
                .Append(_consoleSettings.MSBuildConfigurationOption);

            await Build(projectBuilder);
        }

        public async Task ExecuteBuildInReleaseModeWithoutDependencies()
        {
            var projectBuilder = new StringBuilder($@"""{_project}""")
                .Append(_consoleSettings.MSBuildConfigurationOption)
                .Append(_consoleSettings.MSBuildDependenciesOption);

            await Build(projectBuilder);
        }

        protected virtual void OnBuildStarted(EventArgs args)
        {
            BuildStarted?.Invoke(this, args);
        }

        protected virtual void OnBuildFinished(EventArgs args)
        {
            BuildFinished?.Invoke(this, args);
        }

        protected virtual void OnOutputDataReceived(DataReceivedEventArgs args)
        {
            OutputDataReceived?.Invoke(this, args.Data);
        }

        private async Task Build(StringBuilder projectBuilder)
        {
            OnBuildStarted(EventArgs.Empty);
            projectBuilder
                .Append(_consoleSettings.PostBuildEvents)
                .Append(_consoleSettings.PreBuildEvents)
                .Append(_consoleSettings.MSBuildCustomOption);

            if (EnableLogging)
            {
                var buildLogFilePath = $@"""{_consoleSettings.MSBuildLogDirectory}build_{DateTime.Now:yyyyMdhhmmss}.log""";
                projectBuilder.Append(_consoleSettings.MSBuildLogger)
                    .Append(buildLogFilePath)
                    .Append(";")
                    .Append(VerbosityOption)
                    .Append(_consoleSettings.MSBuildVerbosity);
            }
            else if (QuietWithSymbols)
            {
                projectBuilder
                    .Append(VerbosityOption)
                    .Append(_consoleSettings.QuietBuildWithSymbols);
            }
            else
            {
                projectBuilder
                    .Append(VerbosityOption)
                    .Append(_consoleSettings.QuietBuild);
            }

            if (!string.IsNullOrWhiteSpace(OutputPath))
            {
                projectBuilder.Append($" /p:OutputPath=\"{OutputPath}\"");
            }

            if (!string.IsNullOrWhiteSpace(IntermediateOutputPath))
            {
                projectBuilder.Append($" /p:IntermediateOutputPath=\"{IntermediateOutputPath}\\\"");
            }

            if (string.IsNullOrWhiteSpace(BaseAddress))
            {
                try
                {

                    var processInfo = new ProcessStartInfo(_consoleSettings.MSBuildPath)
                    {
                        Arguments = $" {projectBuilder}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    await Task.Run(() =>
                    {
                        using (var process = new Process
                        {
                            StartInfo = processInfo,
                            EnableRaisingEvents = true
                        })
                        {
                            process.OutputDataReceived += ProcessOnOutputDataReceived;
                            process.ErrorDataReceived += ProcessOnOutputDataReceived;
                            process.Start();
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();
                            process.WaitForExit();

                            LastBuildStatus = process.ExitCode == 0
                                ? BuildExecutionStatus.Success
                                : BuildExecutionStatus.Failed;

                            OnBuildFinished(EventArgs.Empty);
                            process.OutputDataReceived -= ProcessOnOutputDataReceived;
                            process.ErrorDataReceived -= ProcessOnOutputDataReceived;
                        }
                    });
                }
                catch (Exception exp)
                {
                    Trace.TraceError("Unable to Build Product {0}", exp);
                }
            }
            else
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));
                        var content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>(string.Empty, $" {projectBuilder}")
                        });

                        client.Timeout = Timeout.InfiniteTimeSpan;
                        var response = await client.PostAsync($"{BaseAddress}api//mutest/build", content);
                        if (response.IsSuccessStatusCode)
                        {
                            OutputDataReceived?.Invoke(this, $"MuTest Build Service is executing build at {BaseAddress}".PrintWithPreTag());
                            var responseData = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<BuildResult>(responseData);
                            OutputDataReceived?.Invoke(this, result.BuildOutput);
                            LastBuildStatus = result.Status;
                        }
                        else
                        {
                            LastBuildStatus = BuildExecutionStatus.Failed;
                            Trace.TraceError("MuTest Build Service not Found at {0}", BaseAddress);
                            OutputDataReceived?.Invoke(this, $"MuTest Build Service is not running at {BaseAddress}".PrintWithPreTag());
                        }
                    }
                }
                catch (Exception exp)
                {
                    Trace.TraceError("MuTest Build Service not Found at {0}. Unable to Build Product {1}", BaseAddress, exp);
                    OutputDataReceived?.Invoke(this, $"MuTest Build Service is not running at {BaseAddress}".PrintWithPreTag());
                    LastBuildStatus = BuildExecutionStatus.Failed;
                }
            }
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            OnOutputDataReceived(args);
        }
    }
}