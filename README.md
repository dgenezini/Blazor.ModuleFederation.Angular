![](blazor-angular.png)

# Angular Module Federation wrapper for Blazor

Generates an Angular application exposing the registered Blazor components through module federation.

The exposed components accept input parameters and subscription to events.

***Disclaimer: This package is not ready for production.***

[![Package Version](https://img.shields.io/nuget/v/Blazor.ModuleFederation.Angular.svg)](https://www.nuget.org/packages/Blazor.ModuleFederation.Angular)

# Current issues and limitations

- Only works with Blazor WebAssembly;
- Blazor App server needs to have CORS enabled.

# Blazor Configuration

The source code for the Angular application is generated by an MSBuild task. For this, we need to configure which component will be exposed.

## Counter.razor:

In the `.razor` file of the component include the attribute `GenerateModuleFederationComponent` at the top.

```razor
@attribute [GenerateModuleFederationComponent]
@using Blazor.ModuleFederation.Angular;

...
```

## Program.cs:

In the `Program.cs` file, register the component with the `RegisterForModuleFederation` method.

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.RegisterForModuleFederation<Counter>();

...
```

## Project.csproj:

In the `.csproj` file, configure the following parameters:

- **ModuleFederationName**: Name of the module that will be exposed;
- **MicroFrontendBaseUrl**: The URL where the Blazor application will be published to;
- **BuildModuleFederationScript**: Enable or disable the Angular module federation wrapper generation;
- **IsProduction**: If the Angular app will be compiled with production configuration;

```xml
<PropertyGroup>
    <ModuleFederationName>blazormodule</ModuleFederationName>
</PropertyGroup>

<PropertyGroup Condition="'$(Configuration)' == 'DEBUG'">
    <MicroFrontendBaseUrl>http://localhost:5289/</MicroFrontendBaseUrl>
    <BuildModuleFederationScript>False</BuildModuleFederationScript>
    <IsProduction>False</IsProduction>
</PropertyGroup>

<PropertyGroup Condition="'$(Configuration)' == 'RELEASE'">
    <MicroFrontendBaseUrl>http://localhost:8080/</MicroFrontendBaseUrl>
    <BuildModuleFederationScript>True</BuildModuleFederationScript>
    <IsProduction>True</IsProduction>
</PropertyGroup>
```

# Host Configuration

## ComponentLoader:

Create a loader component to load the remote Blazor component. Don't forget to include it in the app module.

```typescript
import {
  Component,
  OnInit,
  ViewContainerRef,
  ComponentRef,
  EventEmitter
} from '@angular/core';

import { loadRemoteModule } from '@angular-architects/module-federation';

@Component({
  selector: 'counter-loader',
  template: ''
})
export class CounterLoaderComponent implements OnInit {
  constructor(
    private vcref: ViewContainerRef
  ) {}

  async ngOnInit() {
    const { CounterComponent } = await loadRemoteModule({
      remoteEntry: 'http://localhost:8080/js/remoteEntry.js',
      remoteName: 'blazormodule',
      exposedModule: './Counter',
    });

    const componentRef: ComponentRef<{
        startFromId: number;
        onDataLoaded: EventEmitter<any>;
      }> = this.vcref.createComponent(CounterComponent);

    componentRef.instance.startAt = 100;
    componentRef.instance.onDataLoaded.subscribe(evt => console.log('Data Loaded'));
  }
}
```

## AppComponent:

Include the loader component in the HTML.

```html
<div class="toolbar" role="banner">
  <span class="header-title">Host App</span>
</div>

<div class="content" role="main">
  <counter-loader></counter-loader>
</div>
```

## webpack.config.js

Import the Blazor exposed component in the `ModuleFederationPlugin.remotes`.

```javascript
const ModuleFederationPlugin = require("webpack/lib/container/ModuleFederationPlugin");
const mf = require("@angular-architects/module-federation/webpack");
const path = require("path");
const share = mf.share;

const sharedMappings = new mf.SharedMappings();
sharedMappings.register(
  path.join(__dirname, 'tsconfig.json'),
  [/* mapped paths to share */]);

module.exports = {
  output: {
    uniqueName: "hostApp",
    publicPath: "auto"
  },
  optimization: {
    runtimeChunk: false
  },
  resolve: {
    alias: {
      ...sharedMappings.getAliases(),
    }
  },
  experiments: {
    outputModule: true
  },
  plugins: [
    new ModuleFederationPlugin({
        library: { type: "module" },

        remotes: {
            blazormodule: 'blazormodule@http://localhost:8080/js/remoteEntry.js'
        },

        shared: share({
          "@angular/core": { singleton: true, strictVersion: true, requiredVersion: 'auto' },
          "@angular/common": { singleton: true, strictVersion: true, requiredVersion: 'auto' },
          "@angular/common/http": { singleton: true, strictVersion: true, requiredVersion: 'auto' },
          "@angular/router": { singleton: true, strictVersion: true, requiredVersion: 'auto' },

          ...sharedMappings.getDescriptors()
        })

    }),
    sharedMappings.getPlugin()
  ],
};
```

# Sample applications

https://github.com/dgenezini/BlazorModuleFederationSample
