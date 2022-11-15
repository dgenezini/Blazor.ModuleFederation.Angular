using Blazor.ModuleFederation.Angular.Build.Common;
using Blazor.ModuleFederation.Angular.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Blazor.ModuleFederation.Angular.Build.Writers;

internal static class AngularWebpackConfigWriter
{
    private const string WebpackConfigTemplate =
@"const ModuleFederationPlugin = require('webpack/lib/container/ModuleFederationPlugin');
const mf = require('@angular-architects/module-federation/webpack');
const path = require('path');
const share = mf.share;

const sharedMappings = new mf.SharedMappings();
sharedMappings.register(
  path.join(__dirname, 'tsconfig.json'),
  [/* mapped paths to share */]);

module.exports = {{
  output: {{
    uniqueName: '{0}',
    publicPath: 'auto',
    scriptType: 'text/javascript',
    path: path.resolve(__dirname, '../wwwroot/js')
  }},
  optimization: {{
    runtimeChunk: false
  }},
  resolve: {{
    alias: {{
      ...sharedMappings.getAliases(),
    }}
  }},
  plugins: [
    new ModuleFederationPlugin({{
        name: '{0}',
        filename: 'remoteEntry.js',
        exposes: {{
{1}
        }},
        shared: share({{
          '@angular/core': {{ singleton: true, strictVersion: false, requiredVersion: 'auto' }},
          '@angular/common': {{ singleton: true, strictVersion: false, requiredVersion: 'auto' }},
          '@angular/common/http': {{ singleton: true, strictVersion: false, requiredVersion: 'auto' }},
          '@angular/router': {{ singleton: true, strictVersion: false, requiredVersion: 'auto' }},

          ...sharedMappings.getDescriptors()
        }})

    }}),
    sharedMappings.getPlugin()
  ],
}};
";

    private const string ComponentExposesTemplate =
        @"          './{0}': './src/app/{1}/{1}.component.ts'";

    public static void Write(string outputDirectory,
        List<RazorComponentDescriptor> componentDescriptors,
        string moduleFederationName)
    {
        var componentExposes = componentDescriptors
            .Select(a =>
                string.Format(ComponentExposesTemplate, a.Name, CasingUtilities.ToKebabCase(a.Name)))
            .ToArray();

        var componentExposesLines = string.Join(", " + Environment.NewLine, componentExposes);

        var webpackContents = string.Format(WebpackConfigTemplate, moduleFederationName.ToLower(),
            componentExposesLines);

        var angularWebPackFile = $"{outputDirectory}/webpack.config.js";

        Directory.CreateDirectory(outputDirectory);
        File.WriteAllText(angularWebPackFile, webpackContents);
    }
}
