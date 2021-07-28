namespace Sitko.Blazor.ScriptInjector
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class InjectRequest
    {
        public string Id { get; }
        public InjectRequestType Type { get; }

        internal string Path { get; set; } = "";
        internal string Content { get; set; } = "";

        protected InjectRequest(string id, InjectRequestType type)
        {
            Id = id;
            Type = type;
        }

        protected static string LoadResource(Assembly assembly, string fileName)
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

    public class ScriptInjectRequest : InjectRequest
    {
        private ScriptInjectRequest(string id, InjectRequestType type) : base(id, type)
        {
        }

        public static ScriptInjectRequest FromUrl(string id, string path) =>
            new(id, InjectRequestType.JsFile) { Path = path };

        public static ScriptInjectRequest Inline(string id, string content) =>
            new(id, InjectRequestType.JsInline) { Content = content };

        public static ScriptInjectRequest FromResource(string id, Assembly assembly, string fileName) =>
            new(id, InjectRequestType.JsInline) { Content = LoadResource(assembly, fileName) };

        public static ScriptInjectRequest InlineEval(string id, string content) =>
            new(id, InjectRequestType.JsEval) { Content = content };

        public static ScriptInjectRequest FromResourceEval(string id, Assembly assembly, string fileName) =>
            new(id, InjectRequestType.JsEval) { Content = LoadResource(assembly, fileName) };
    }

    public class CssInjectRequest : InjectRequest
    {
        private CssInjectRequest(string id, InjectRequestType type) : base(id, type)
        {
        }

        public static CssInjectRequest FromUrl(string id, string path) =>
            new(id, InjectRequestType.CssFile) { Path = path };

        public static CssInjectRequest Inline(string id, string content) =>
            new(id, InjectRequestType.CssInline) { Content = content };

        public static CssInjectRequest FromResource(string id, Assembly assembly, string fileName) =>
            new(id, InjectRequestType.CssInline) { Content = LoadResource(assembly, fileName) };
    }
}
