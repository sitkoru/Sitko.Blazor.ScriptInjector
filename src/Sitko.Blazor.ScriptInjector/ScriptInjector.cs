﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Sitko.Blazor.ScriptInjector;

using Microsoft.AspNetCore.Components;

[PublicAPI]
public interface IScriptInjector
{
    Task InjectAsync(InjectRequest request, Func<CancellationToken, Task>? callback = null,
        CancellationToken cancellationToken = default);

    Task InjectAsync(IEnumerable<InjectRequest> requests, Func<CancellationToken, Task>? callback = null,
        CancellationToken cancellationToken = default);
}

public record InjectTask(InjectScope Scope, TaskCompletionSource<bool> CompletionSource);

public class ScriptInjector : IScriptInjector
{
    private readonly DotNetObjectReference<ScriptInjector> instance;
    private readonly IJSRuntime jsRuntime;
    private readonly ILogger<ScriptInjector> logger;
    private readonly ConcurrentDictionary<string, InjectTask> requestTasks = new();
    private bool isInitialized;

    public ScriptInjector(IJSRuntime jsRuntime, NavigationManager navigationManager, ILogger<ScriptInjector> logger)
    {
        this.jsRuntime = jsRuntime;
        this.logger = logger;
        instance = DotNetObjectReference.Create(this);
        navigationManager.LocationChanged += (_, _) =>
        {
            var toRemove = requestTasks.Where(task => task.Value.Scope == InjectScope.Transient)
                .Select(pair => pair.Key).ToList();
            foreach (var injectTaskId in toRemove)
            {
                logger.LogDebug("Remove task {Id}", injectTaskId);
                requestTasks.TryRemove(injectTaskId, out _);
            }
        };
    }

    public Task InjectAsync(InjectRequest request, Func<CancellationToken, Task>? callback = null,
        CancellationToken cancellationToken = default) =>
        InjectAsync(new[] { request }, callback, cancellationToken);

    public async Task InjectAsync(IEnumerable<InjectRequest> requests,
        Func<CancellationToken, Task>? callback = null,
        CancellationToken cancellationToken = default)
    {
        if (!isInitialized)
        {
            logger.LogDebug("Init injector");
            await DoInjectAsync(
                new[] { ScriptInjectRequest.FromResourceEval("inject", GetType().Assembly, "inject.js") }, null,
                cancellationToken);
            isInitialized = true;
        }

        await DoInjectAsync(requests, callback, cancellationToken);
    }

    private async Task DoInjectAsync(IEnumerable<InjectRequest> requests, Func<CancellationToken, Task>? callback,
        CancellationToken cancellationToken)
    {
        var tasks = new List<Task<bool>>();
        foreach (var injectRequest in requests)
        {
            logger.LogDebug("Load request {Id}", injectRequest.Id);
            var newTask = new InjectTask(injectRequest.Scope, new TaskCompletionSource<bool>());
            var task = requestTasks.GetOrAdd(injectRequest.Id, newTask);
            if (task == newTask)
            {
                logger.LogDebug("Add task {Id}", injectRequest.Id);
                await InjectAsync(injectRequest, cancellationToken);
            }

            tasks.Add(task.CompletionSource.Task);
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
            logger.LogError("Not all resources are loaded successfully");
        }
    }

    private async Task InjectAsync(InjectRequest request, CancellationToken cancellationToken)
    {
        switch (request.Type)
        {
            case InjectRequestType.JsFile:
                await jsRuntime.InvokeVoidAsync("scriptInjectFunction", cancellationToken, instance,
                    request.Id, request.Path);
                break;
            case InjectRequestType.JsInline:
                await jsRuntime.InvokeVoidAsync("scriptInlineInjectFunction", cancellationToken, instance,
                    request.Id, request.Content);
                await RequestLoadedAsync(request.Id);
                break;
            case InjectRequestType.CssFile:
                await jsRuntime.InvokeVoidAsync("cssInjectFunction", cancellationToken, instance,
                    request.Id, request.Path);
                break;
            case InjectRequestType.CssInline:
                await jsRuntime.InvokeVoidAsync("cssInlineInjectFunction", cancellationToken, instance,
                    request.Id, request.Content);
                break;
            case InjectRequestType.JsEval:
                await jsRuntime.InvokeVoidAsync("eval", cancellationToken, request.Content);
                await RequestLoadedAsync(request.Id);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Type {request.Type} unknown");
        }
    }

    [JSInvokable]
    public Task RequestLoadedAsync(string name)
    {
        RequestFinished(name, true);
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task RequestFailedAsync(string name)
    {
        RequestFinished(name, false);
        return Task.CompletedTask;
    }

    private void RequestFinished(string name, bool isSuccess)
    {
        if (requestTasks.TryGetValue(name, out var injectTask))
        {
            if (isSuccess) { logger.LogDebug("Request {Name} is loaded", name); }
            else { logger.LogError("Request {Name} is failed", name); }

            injectTask.CompletionSource.TrySetResult(isSuccess);
        }
        else
        {
            logger.LogError("Task for request {Name} not found", name);
        }
    }
}
