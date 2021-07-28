if (!window.scriptInjectFunction) {
  window.scriptInjectFunction = function (scriptPath, scriptKey, instance) {
    const script = document.createElement("script");
    script.src = scriptPath;
    script.type = "text/javascript";
    script.id = scriptKey;
    // if the script returns okay, return resolve
    script.onload = function () {
      instance.invokeMethodAsync("ScriptLoadedAsync", scriptKey);
    };
    // if it fails, return reject
    script.onerror = function () {
      instance.invokeMethodAsync("ScriptFailedAsync", scriptKey);
    }
    // scripts will load at end of body
    document["body"].appendChild(script);
  }
}
