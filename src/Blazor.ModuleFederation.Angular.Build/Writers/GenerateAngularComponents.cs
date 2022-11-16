using Blazor.ModuleFederation.Angular.Build.Common;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace Blazor.ModuleFederation.Angular.Build.Writers
{
    public class GenerateAngularComponents : Task
    {
        [Required]
        public string OutputPath { get; set; }
        
        [Required]
        public string JsFilesPath { get; set; }

        [Required]
        public string IntermediateOutputPath { get; set; }

        [Required]
        public string AssemblyName { get; set; }

        [Required]
        public string JavaScriptComponentOutputDirectory { get; set; }

        [Required]
        public string ModuleFederationName { get; set; }

        [Required]
        public string MicroFrontendBaseUrl { get; set; }

        private void CopyFiles(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);

            foreach (var directory in Directory.GetDirectories(sourceDir))
                CopyFiles(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
        }

        public override bool Execute()
        {
            var assemblyFilePath = $"{OutputPath}/{AssemblyName}.dll";
            HashSet<string> componentNames;

            try
            {
                componentNames = new(RazorComponentReader.ReadWithAttributeFromAssembly(assemblyFilePath, "GenerateAngularAttribute"));
            }
            catch (Exception e)
            {
                Log.LogError($"An exception occurred while reading the specified assembly: {e.Message}");
                return false;
            }

            var tagHelperCacheFileName = $"{IntermediateOutputPath}/{AssemblyName}.TagHelpers.output.cache";
            List<RazorComponentDescriptor> componentDescriptors;

            try
            {
                componentDescriptors = RazorComponentDescriptorReader.ReadWithNamesFromTagHelperCache(tagHelperCacheFileName, componentNames);
            }
            catch (Exception e)
            {
                Log.LogError($"An exception occurred while reading the tag helper output cache: {e.Message}");

                return false;
            }

            try
            {
                CopyFiles(JsFilesPath, JavaScriptComponentOutputDirectory);
            }
            catch (Exception e)
            {
                Log.LogError($"Could not copy the template files: {e.Message}");

                return false;
            }

            foreach (var componentDescriptor in componentDescriptors)
            {
                try
                {
                    AngularComponentWriter.Write(JavaScriptComponentOutputDirectory, componentDescriptor);
                }
                catch (Exception e)
                {
                    Log.LogError($"Could not write an Angular component from Razor component '{componentDescriptor.Name}': {e.Message} {e.StackTrace}");
                    return false;
                }
            }

            try
            {
                AngularModuleWriter.Write(JavaScriptComponentOutputDirectory, componentDescriptors);
            }
            catch (Exception e)
            {
                Log.LogError($"Could not write Angular module': {e.Message} {e.StackTrace}");
                return false;
            }

            try
            {
                AngularWebpackConfigWriter.Write(JavaScriptComponentOutputDirectory, 
                    componentDescriptors, ModuleFederationName);
            }
            catch (Exception e)
            {
                Log.LogError($"Could not write Angular Webpack Config': {e.Message} {e.StackTrace}");
                return false;
            }

            try
            {
                AngularEnvironmentWriter.Write(JavaScriptComponentOutputDirectory, MicroFrontendBaseUrl, false);
                AngularEnvironmentWriter.Write(JavaScriptComponentOutputDirectory, MicroFrontendBaseUrl, true);
            }
            catch (Exception e)
            {
                Log.LogError($"Could not write Angular environments': {e.Message} {e.StackTrace}");
                return false;
            }

            Log.LogMessage("Angular component generation complete.");
            return true;
        }
    }
}
