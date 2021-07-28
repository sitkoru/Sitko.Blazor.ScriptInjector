namespace Sitko.Blazor.ScriptInjector
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class ScriptInjectRequest
    {
        public string Id { get; }
        public string Path { get; private set; } = "";
        public string Content { get; private set; } = "";
        public ScriptInjectRequestType Type { get; }

        private ScriptInjectRequest(string id, ScriptInjectRequestType type)
        {
            Id = id;
            Type = type;
        }

        public static ScriptInjectRequest FromUrl(string id, string path) =>
            new(id, ScriptInjectRequestType.Path) { Path = path };

        public static ScriptInjectRequest Inline(string id, string content) =>
            new(id, ScriptInjectRequestType.Eval) { Content = content };

        public static ScriptInjectRequest FromResource(string id, Assembly assembly, string fileName)
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
            string js = reader.ReadToEnd();
            return new ScriptInjectRequest(id, ScriptInjectRequestType.Eval) { Content = js };
        }
    }
}
