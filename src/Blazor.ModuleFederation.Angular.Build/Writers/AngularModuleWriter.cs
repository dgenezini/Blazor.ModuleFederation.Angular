using Blazor.ModuleFederation.Angular.Build.Common;
using Blazor.ModuleFederation.Angular.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Blazor.ModuleFederation.Angular.Build.Writers
{
    internal static class AngularModuleWriter
    {
        private const string AngularModuleTemplate =
@"import {{ NgModule }} from '@angular/core';
import {{ BrowserModule }} from '@angular/platform-browser';

{0}

@NgModule({{
  declarations: [{1}],
  imports: [
    BrowserModule
  ],
  bootstrap: [{1}]
}})
export class AppModule {{ }}
";

        private const string ComponentImportTemplate =
            @"import {{ {0}Component }} from './{1}/{1}.component'";

        public static void Write(string outputDirectory, List<RazorComponentDescriptor> componentDescriptors)
        {
            var componentNames = componentDescriptors
                .Select(a => $"{a.Name}Component")
                .ToArray();

            var componentImports = componentDescriptors
                .Select(a =>
                    string.Format(ComponentImportTemplate, a.Name, CasingUtilities.ToKebabCase(a.Name)))
                .ToArray();

            var componentDeclarations = string.Join(", ", componentNames);
            var componentImportsLines = string.Join(Environment.NewLine, componentImports);

            var componentContents = string.Format(AngularModuleTemplate,
                componentImportsLines, componentDeclarations);

            var angularModulePath = $"{outputDirectory}/src/app";
            var angularModuleFile = $"{angularModulePath}/app.module.ts";

            Directory.CreateDirectory(angularModulePath);
            File.WriteAllText(angularModuleFile, componentContents);
        }
    }
}
