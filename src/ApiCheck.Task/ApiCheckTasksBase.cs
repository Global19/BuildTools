// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.AspNetCore.BuildTools.ApiCheck.Task
{
    public abstract class ApiCheckTasksBase : ToolTask
    {
        protected const string ApiCheckToolName = "Microsoft.AspNetCore.BuildTools.ApiCheck";

        /// <summary>
        /// Path to the assembly to consider.
        /// </summary>
        [Required]
        public string AssemblyPath { get; set; }

        /// <summary>
        /// The framework moniker for <see cref="AssemblyPath"/>.
        /// </summary>
        [Required]
        public string Framework { get; set; }

        /// <inheritdoc />
        protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.High;

        /// <inheritdoc />
        protected override MessageImportance StandardOutputLoggingImportance => MessageImportance.High;

        /// <inheritdoc />
        protected override string ToolName
        {
            get
            {
                if (Framework.StartsWith("net4", StringComparison.OrdinalIgnoreCase))
                {
                    return ApiCheckToolName + ".exe";
                }

                var exeExtension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty;
                return "dotnet" + exeExtension;
            }
        }

        /// <inheritdoc />
        protected override string GenerateFullPathToTool()
        {
            // ToolExe equals ToolName by default. Assume (as base class does) any user override is in the same directory.
            if (Framework.StartsWith("net4", StringComparison.OrdinalIgnoreCase))
            {
                var taskAssemblyFolder = Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location);
                return Path.GetFullPath(Path.Combine(taskAssemblyFolder, "..", "net46", ToolExe));
            }

            // If muxer does not find dotnet, fall back to system PATH and hope for the best.
            return DotNetMuxer.MuxerPath ?? ToolExe;
        }

        /// <inheritdoc />
        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            // Since tool prints out formatted list of breaking changes,
            // anything that starts with Error considered an error; the rest is user information.
            if (singleLine.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
            {
                Log.LogError(singleLine, Array.Empty<object>());
            }
            else
            {
                base.LogEventsFromTextOutput(singleLine, messageImportance);
            }
        }
    }
}
