interface Window {
  scriptInjectFunction(instance: any, id: string, scriptPath: string): void;

  scriptInlineInjectFunction(instance: any, id: string, script: string): void;

  cssInjectFunction(instance: any, id: string, cssPath: string): void;

  cssInlineInjectFunction(instance: any, id: string, css: string): void;
}

if (!window.scriptInjectFunction) {
  function injectElement(instance: any, id: string, element: HTMLElement): void {
    element.onload = () => {
      onLoad(id, instance);
    }
    // if it fails, return reject
    element.onerror = () => {
      onError(id, instance);
    }
    // scripts will load at end of body
    document["body"].appendChild(element);
  }

  function isAlreadyLoaded(id: string) {
    return !!document.getElementById(id);

  }

  function onLoad(id: string, instance: any): void {
    console.debug(id, 'loaded');
    instance.invokeMethodAsync("RequestLoadedAsync", id);
  }

  function onError(id: string, instance: any): void {
    console.debug(id, 'failed');
    instance.invokeMethodAsync("RequestFailedAsync", id);
  }

  window.scriptInjectFunction = function (instance, id, scriptPath) {
    console.debug("Inject script from url");
    const elementId = 'js-' + id;
    if (!isAlreadyLoaded(elementId)) {
      const script = document.createElement("script");
      script.src = scriptPath;
      script.type = "text/javascript";
      script.id = elementId;
      injectElement(instance, id, script);
    } else {
      onLoad(id, instance);
    }
  }
  window.scriptInlineInjectFunction = function (instance, id, scriptContent) {
    console.debug("Inject inline script");
    const elementId = 'js-' + id;
    if (!isAlreadyLoaded(elementId)) {
      const script = document.createElement("script");
      script.type = "text/javascript";
      script.id = elementId;
      script.text = scriptContent;
      injectElement(instance, id, script);
    } else {
      onLoad(id, instance);
    }
  }
  window.cssInjectFunction = function (instance, id, cssPath) {
    console.debug("Inject css from url");
    const elementId = 'css-' + id;
    if (!isAlreadyLoaded(elementId)) {
      const linkElement = document.createElement("link");
      linkElement.href = cssPath;
      linkElement.rel = "stylesheet";
      linkElement.id = elementId;
      injectElement(instance, id, linkElement);
    } else {
      onLoad(id, instance);
    }
  }
  window.cssInlineInjectFunction = function (instance, id, css: string) {
    console.debug("Inject inline css");
    const elementId = 'css-' + id;
    if (!isAlreadyLoaded(elementId)) {
      const styleElement = document.createElement('style');
      styleElement.id = elementId;
      styleElement.textContent = css;
      injectElement(instance, id, styleElement);
    } else {
      onLoad(id, instance);
    }
  }
}
