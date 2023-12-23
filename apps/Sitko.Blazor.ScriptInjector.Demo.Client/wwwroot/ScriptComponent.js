console.log("ScriptComponent.js loaded")
function InitScriptComponent(id) {
  console.info("Execute for " + id);
  document.getElementById(id).innerText = id + " Loaded!";
}
