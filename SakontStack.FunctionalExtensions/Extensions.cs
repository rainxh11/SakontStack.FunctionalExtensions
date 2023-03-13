using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SakontStack.FunctionalExtensions;

public static class EnumerableExtensions
{
    public static string StringJoin<T>(this IEnumerable<T> enumerable,
                                       string separator)
    {
        return string.Join(separator, enumerable);
    }


    public static IEnumerable<T> Modify<T>(this IEnumerable<T> collection,
                                           Action<T> modify)
    {
        return collection.Select(x =>
        {
            modify(x);
            return x;
        });
    }

    public static TOutput PipelineWith<T, TInput, TOutput>(this IEnumerable<T> enumerable,
                                                           Func<T, TInput> stageFunction,
                                                           Func<TInput, TOutput> lastStage)
    {
        var input                              = default(TInput);
        foreach (var item in enumerable) input = stageFunction(item);

        return lastStage(input!);
    }

    public static TInput PipelineWith<T, TInput>(this IEnumerable<T> enumerable,
                                                 Func<T, TInput> stageFunction)
    {
        var input                              = default(TInput);
        foreach (var item in enumerable) input = stageFunction(item);

        return input!;
    }

    public static TOutput PipelineWith<T, TToken, TInput, TOutput>(this IEnumerable<T> enumerable,
                                                                   TToken token,
                                                                   Func<TToken, T, TInput> stageFunction,
                                                                   Func<TToken, TInput, TOutput> lastStage)
    {
        var input                              = default(TInput);
        foreach (var item in enumerable) input = stageFunction(token, item);

        return lastStage(token, input!);
    }

    public static TInput PipelineWith<T, TToken, TInput>(this IEnumerable<T> enumerable,
                                                         TToken token,
                                                         Func<TToken, T, TInput> stageFunction)
    {
        var input                              = default(TInput);
        foreach (var item in enumerable) input = stageFunction(token, item);

        return input!;
    }


    public static T PickRandom<T>(this IEnumerable<T> enumerable)
    {
        var array = enumerable.ToArray();
        return array[new Random().Next(0, array.Length - 1)];
    }

    public static IEnumerable<T> OlderThanBy<T>(this IEnumerable<T> enumerable,
                                                Func<T, DateTime> selector,
                                                TimeSpan span)
    {
        return enumerable.Where(x => DateTime.Now - selector(x) > span);
    }

    public static IEnumerable<T> NewerThanBy<T>(this IEnumerable<T> enumerable,
                                                Func<T, DateTime> selector,
                                                TimeSpan span)
    {
        return enumerable.Where(x => DateTime.Now - selector(x) < span);
    }


    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable,
                                            Action<T> action,
                                            bool suppressExceptions = false)
    {
        return enumerable.Select(x =>
        {
            try
            {
                action(x);
            }
            catch
            {
                if (!suppressExceptions) throw;
            }

            return x;
        });
    }
}

public static class FunctionalExtensions
{
    public static TResult Cast<T, TResult>(this T value)
    {
        return (TResult)(object)value;
    }

    public static TResult As<T, TResult>(this T? value) where TResult : class
    {
        return value as TResult;
    }

    public static TResult As<TResult>(this object? value) where TResult : class
    {
        return value as TResult;
    }

    public static void Loop(this int count,
                            Action<int> loop,
                            bool suppressException = false)
    {
        foreach (var index in Enumerable.Range(0, count))
            try
            {
                loop(index);
            }
            catch
            {
                if (!suppressException) throw;
            }
    }

    public static async Task LoopDelayedAsync(this int count,
                                              Func<int, TimeSpan> delaySelector,
                                              Action<int> loop,
                                              bool suppressException = false)
    {
        foreach (var index in Enumerable.Range(0, count))
            try
            {
                loop(index);
            }
            catch
            {
                if (!suppressException) throw;
            }
            finally
            {
                await Task.Delay(delaySelector(index));
            }
    }


    public static TResult Map<T, TResult>(this T value,
                                          Func<T, TResult> mapper)
    {
        return mapper(value);
    }

    public static TResult Map<T, TResult>(this T value,
                                          Func<T, TResult> mapper,
                                          Func<T, Exception, TResult> onErrorMapper)
    {
        try
        {
            return mapper(value);
        }
        catch (Exception ex)
        {
            return onErrorMapper(value, ex);
        }
    }


    public static TResult Map<T, TResult>(this T value,
                                          Func<T, TResult> mapper,
                                          TResult onErrorResult)
    {
        try
        {
            return mapper(value);
        }
        catch (Exception ex)
        {
            return onErrorResult;
        }
    }

    public static T DoWith<T>(this T value,
                              Action<T> action,
                              bool suppressExceptions = false)
    {
        try
        {
            action(value);
        }
        catch
        {
            if (!suppressExceptions) throw;
        }

        return value;
    }

    public static Task<T> DoAsync<T>(this T value,
                                     Func<T, Task> action,
                                     bool suppressExceptions = false)
    {
        try
        {
            action(value);
        }
        catch
        {
            if (!suppressExceptions) throw;
        }

        return Task.FromResult(value);
    }

    public static T Mutate<T>(this T value,
                              Action<T> mutationAction)
    {
        mutationAction.Invoke(value);
        return value;
    }

    public static async Task<T> MutateAsync<T>(this T value,
                                               Func<T, Task<T>> mutationAction)
    {
        await mutationAction.Invoke(value);
        return value;
    }
}

public static class TaskExtensions
{
    public static T RunSync<T>(this Task<T> task)
    {
        return task.GetAwaiter().GetResult();
    }

    public static void RunSync(this Task task)
    {
        task.GetAwaiter().GetResult();
    }

    public static Task WhenCanceled(this CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();
        cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
        return tcs.Task;
    }

    public static CancellationTokenSource CreateCancellationToken(this TimeSpan delay)
    {
        return new CancellationTokenSource(delay);
    }
}