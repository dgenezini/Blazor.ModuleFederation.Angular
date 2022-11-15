import { Injectable, EventEmitter } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'any'
})
export class BlazorStateManager {
  private _scriptLoaded: boolean = false;
  private _blazorLoaded: boolean = false;

  statusChanged: EventEmitter<boolean> = new EventEmitter<boolean>();

  async setBlazorAsync() {
    let blazorMicroFrontendScriptPath: URL =
      new URL("_framework/blazor.webassembly.js", environment.blazorMicroFrontendBaseUrl);

    await this.loadAndParseScriptAsync(blazorMicroFrontendScriptPath);

    await this.startBlazorAsync();
  };

  removeBlazor() {
    let blazorMicroFrontendScriptPath: URL =
      new URL("_framework/blazor.webassembly.js", environment.blazorMicroFrontendBaseUrl);

    let monoScriptPath =
      new URL("_framework/wasm/mono.js", environment.blazorMicroFrontendBaseUrl);

    // window.removeEventListener('popstate');
    // document.removeEventListeners('click');
    // document.removeEventListeners('change');
    // document.removeEventListeners('keydown');

    document.getElementsByTagName("bz-compiler")[0].innerHTML = "";

    var scriptBlazor = document.querySelector("script[src='" + blazorMicroFrontendScriptPath + "']");
    if ((scriptBlazor) && (scriptBlazor.parentNode))
      scriptBlazor.parentNode.removeChild(scriptBlazor);

    var scriptWasm = document.querySelector("script[src='" + monoScriptPath + "']");
    if ((scriptWasm) && (scriptWasm.parentNode))
      scriptWasm.parentNode.removeChild(scriptWasm);

    this._blazorLoaded = true;
  };

  async resetBlazor() {
    this.removeBlazor();
    await this.setBlazorAsync();
  };

  private async loadAndParseScriptAsync(url: URL): Promise<boolean> {
    if (this._scriptLoaded) {
      return true;
    }

    try {
      let response = await fetch(url);

      if (response.ok) {
        var scriptContent = await response.text();

        scriptContent = scriptContent.replaceAll("_framework/",
          environment.blazorMicroFrontendBaseUrl + "_framework/");

        scriptContent = scriptContent.replaceAll("credentials:\"include\",", "");

        let script = document.createElement("script");
        script.setAttribute("autostart", "false");

        script.innerHTML = scriptContent;

        document.head.appendChild(script);

        this._scriptLoaded = true;
      }
      else {
        console.error('Error loading Blazor script.');
      }
    }
    catch
    {
      console.error('Error loading Blazor script.');
    }

    return this._scriptLoaded;
  }

  private async startBlazorAsync(): Promise<boolean> {
    if (this._blazorLoaded) {
      return true;
    }

    console.debug('Loading Blazor WASM...');

    let BlazorRef: any = window["Blazor" as any];

    BlazorRef
      .start()
      .then(() => {
        this._blazorLoaded = true;

        console.debug('Blazor WASM loaded.');

        this.statusChanged.emit(true);
      })
      .catch((error: any) => {
        console.error('Blazor WASM not loaded: ' + error);

        this.statusChanged.emit(false);
      });

    return this._blazorLoaded;
  }
}
