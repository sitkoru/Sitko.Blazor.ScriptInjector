interface Window {
  scriptInjectFunction(instance: any, id: string, scriptPath: string): void;

  scriptInlineInjectFunction(instance: any, id: string, script: string): void;

  cssInjectFunction(instance: any, id: string, cssPath: string): void;

  cssInlineInjectFunction(instance: any, id: string, css: string): void;
}

if (!window.scriptInjectFunction) {
  function injectElement(instance: any, id: string, element: HTMLElement): void {
    element.onload = () => {
      console.debug(id, 'loaded');
      instance.invokeMethodAsync("RequestLoadedAsync", id);
    }
    // if it fails, return reject
    element.onerror = () => {
      console.debug(id, 'failed');
      instance.invokeMethodAsync("RequestFailedAsync", id);
    }
    // scripts will load at end of body
    document["body"].appendChild(element);
  }

  function checkElement(id: string) {
    if (document.getElementById(id)) {
      throw new Error(`Resource ${id} already loaded`);
    }
  }

  window.scriptInjectFunction = function (instance, id, scriptPath) {
    console.debug("Inject script from url");
    const elementId = 'js-' + id;
    checkElement(elementId);
    const script = document.createElement("script");
    script.src = scriptPath;
    script.type = "text/javascript";
    script.id = elementId;
    injectElement(instance, id, script);
  }
  window.scriptInlineInjectFunction = function (instance, id, scriptContent) {
    console.debug("Inject inline script");
    const elementId = 'js-' + id;
    checkElement(elementId);
    const script = document.createElement("script");
    script.type = "text/javascript";
    script.id = elementId;
    script.text = scriptContent;
    injectElement(instance, id, script);
  }
  window.cssInjectFunction = function (instance, id, cssPath) {
    console.debug("Inject css from url");
    const elementId = 'css-' + id;
    checkElement(elementId);
    const linkElement = document.createElement("link");
    linkElement.href = cssPath;
    linkElement.rel = "stylesheet";
    linkElement.id = elementId;
    injectElement(instance, id, linkElement);
  }
  window.cssInlineInjectFunction = function (instance, id, css: string) {
    console.debug("Inject inline css");
    const elementId = 'css-' + id;
    checkElement(elementId);
    const styleElement = document.createElement('style');
    styleElement.id = elementId;
    styleElement.textContent = css;
    injectElement(instance, id, styleElement);
  }
}
