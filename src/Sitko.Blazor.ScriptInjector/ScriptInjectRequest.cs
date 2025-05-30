namespace Sitko.Blazor.ScriptInjector
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public enum InjectScope
    {
        Transient = 1,
        Scoped = 2
    }

    public static class ResourceLoader
    {
        public static string LoadResource(Assembly assembly, string fileName)
        {
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.Ordinal));
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new InvalidOperationException($"Script {fileName} not found in assembly");
            }

            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new InvalidOperationException($"Script {fileName} can't be loaded from assembly");
            }

            using StreamReader reader = new(stream);
            return reader.ReadToEnd();
        }
    }

    public abstract record InjectRequest(string Id, InjectRequestType Type, InjectScope Scope = InjectScope.Transient)
    {
        internal string Path { get; set; } = "";
        internal string Content { get; set; } = "";
    }

    public record ScriptInjectRequest : InjectRequest
    {
        private ScriptInjectRequest(string Id, InjectRequestType Type, InjectScope Scope = InjectScope.Transient) :
            base(Id, Type, Scope)
        {
        }

        public static ScriptInjectRequest FromUrl(string id, string path, InjectScope scope = InjectScope.Transient) =>
            new(id, InjectRequestType.JsFile, scope) { Path = path };

        public static ScriptInjectRequest
            Inline(string id, string content, InjectScope scope = InjectScope.Transient) =>
            new(id, InjectRequestType.JsInline, scope) { Content = content };

        public static ScriptInjectRequest FromResource(string id, Assembly assembly, string fileName,
            InjectScope scope = InjectScope.Transient) =>
            new(id, InjectRequestType.JsInline, scope) { Content = ResourceLoader.LoadResource(assembly, fileName) };

        public static ScriptInjectRequest InlineEval(string id, string content,
            InjectScope scope = InjectScope.Transient) =>
            new(id, InjectRequestType.JsEval, scope) { Content = content };

        public static ScriptInjectRequest FromResourceEval(string id, Assembly assembly, string fileName,
            InjectScope scope = InjectScope.Transient) =>
            new(id, InjectRequestType.JsEval, scope) { Content = ResourceLoader.LoadResource(assembly, fileName) };
    }

    public record CssInjectRequest : InjectRequest
    {
        private CssInjectRequest(string id, InjectRequestType type, InjectScope scope = InjectScope.Transient) : base(
            id, type, scope)
        {
        }

        public static CssInjectRequest FromUrl(string id, string path, InjectScope scope = InjectScope.Transient) =>
            new(id, InjectRequestType.CssFile, scope) { Path = path };

        public static CssInjectRequest Inline(string id, string content, InjectScope scope = InjectScope.Transient) =>
            new(id, InjectRequestType.CssInline, scope) { Content = content };

        public static CssInjectRequest FromResource(string id, Assembly assembly, string fileName,
            InjectScope scope = InjectScope.Transient) =>
            new(id, InjectRequestType.CssInline, scope) { Content = ResourceLoader.LoadResource(assembly, fileName) };
    }
}
