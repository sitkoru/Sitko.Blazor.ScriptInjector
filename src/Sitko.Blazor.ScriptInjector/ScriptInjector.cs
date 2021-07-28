﻿namespace Sitko.Blazor.ScriptInjector
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Microsoft.JSInterop;

    [PublicAPI]
    public interface IScriptInjector
    {
        Task InjectAsync(ScriptInjectRequest script, Func<CancellationToken, Task>? callback = null,
            CancellationToken cancellationToken = default);

        Task InjectAsync(IEnumerable<ScriptInjectRequest> scripts, Func<CancellationToken, Task>? callback = null,
            CancellationToken cancellationToken = default);
    }

    public class ScriptInjector : IScriptInjector
    {
        private readonly IJSRuntime jsRuntime;
        private readonly ILogger<ScriptInjector> logger;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> requests = new();
        private bool isInitialized;
        private readonly DotNetObjectReference<ScriptInjector> instance;

        public ScriptInjector(IJSRuntime jsRuntime, ILogger<ScriptInjector> logger)
        {
            this.jsRuntime = jsRuntime;
            this.logger = logger;
            instance = DotNetObjectReference.Create(this);
        }

        public Task InjectAsync(ScriptInjectRequest script, Func<CancellationToken, Task>? callback = null,
            CancellationToken cancellationToken = default) =>
            InjectAsync(new[] { script }, callback, cancellationToken);

        public async Task InjectAsync(IEnumerable<ScriptInjectRequest> scripts,
            Func<CancellationToken, Task>? callback = null,
            CancellationToken cancellationToken = default)
        {
            if (!isInitialized)
            {
                await InitAsync(cancellationToken);
                isInitialized = true;
            }

            var tasks = new List<Task<bool>>();
            foreach (var scriptInjectRequest in scripts)
            {
                var isNew = false;
                var tcs = requests.GetOrAdd(scriptInjectRequest.Id, _ =>
                {
                    isNew = true;
                    return new TaskCompletionSource<bool>();
                });
                if (isNew)
                {
                    await InjectAsync(scriptInjectRequest, cancellationToken);
                }

                tasks.Add(tcs.Task);
            }

            await Task.WhenAll(tasks);
            if (tasks.All(t => t.Result))
            {
                if (callback is not null)
                {
                    await callback(cancellationToken);
                }
            }
            else
            {
                logger.LogError("Not all scripts are loaded successfully");
            }
        }

        private Task InitAsync(CancellationToken cancellationToken) =>
            InjectAsync(ScriptInjectRequest.FromResource("inject", GetType().Assembly, "inject.js"), cancellationToken);

        private async Task InjectAsync(ScriptInjectRequest request, CancellationToken cancellationToken)
        {
            switch (request.Type)
            {
                case ScriptInjectRequestType.Path:
                    await jsRuntime.InvokeVoidAsync("scriptInjectFunction", cancellationToken, request.Path, request.Id,
                        instance);
                    break;
                case ScriptInjectRequestType.Eval:
                    await jsRuntime.InvokeVoidAsync("eval", cancellationToken, request.Content);
                    await ScriptLoadedAsync(request.Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Type {request.Type} unknown");
            }
        }

        [JSInvokable]
        public Task ScriptLoadedAsync(string name)
        {
            if (requests.TryGetValue(name, out var tcs))
            {
                logger.LogDebug("Script {Name} is loaded", name);
                tcs.SetResult(true);
            }

            return Task.CompletedTask;
        }

        [JSInvokable]
        public Task ScriptFailedAsync(string name)
        {
            if (requests.TryGetValue(name, out var tcs))
            {
                logger.LogError("Script {Name} is failed", name);
                tcs.SetResult(false);
            }

            return Task.CompletedTask;
        }
    }
}
