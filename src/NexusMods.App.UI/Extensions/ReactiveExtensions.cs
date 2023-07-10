using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Alias;

namespace NexusMods.App.UI.Extensions;

public static class ReactiveExtensions
{
    /// <summary>
    /// Applies the given class to the given control when a true value is observed
    /// removes the class when a false value is observed.
    /// </summary>
    /// <param name="obs"></param>
    /// <param name="target"></param>
    /// <param name="classToApply"></param>
    /// <typeparam name="TTarget"></typeparam>
    /// <returns></returns>
    public static IDisposable BindToClasses<TTarget>(this IObservable<bool> obs, TTarget target, string classToApply)
        where TTarget : StyledElement
    {
        return obs
            .OnUI()
            .SubscribeWithErrorLogging(logger: default, val =>
            {
                if (val)
                    target.Classes.Add(classToApply);
                else
                    target.Classes.Remove(classToApply);
            });
    }

    /// <summary>
    /// Applies the given class to the given control when a true value is observed, and the false value when false is observed
    /// removes the opposite class when an opposite value is observed.
    /// </summary>
    /// <param name="obs"></param>
    /// <param name="target"></param>
    /// <param name="trueClass"></param>
    /// <param name="falseClass"></param>
    /// <typeparam name="TTarget"></typeparam>
    /// <returns></returns>
    public static IDisposable BindToClasses<TTarget>(this IObservable<bool> obs, TTarget target, string trueClass, string falseClass)
        where TTarget : StyledElement
    {
        return obs
            .OnUI()
            .SubscribeWithErrorLogging(logger: default, val =>
            {
                if (val)
                {
                    target.Classes.Remove(falseClass);
                    target.Classes.Add(trueClass);
                }
                else
                {
                    target.Classes.Remove(trueClass);
                    target.Classes.Add(falseClass);
                }
            });
    }

    /// <summary>
    /// Shorthand for applying "Active" to the given element when true is observed
    /// </summary>
    /// <param name="obs"></param>
    /// <param name="target"></param>
    /// <typeparam name="TTarget"></typeparam>
    /// <returns></returns>
    public static IDisposable BindToActive<TTarget>(this IObservable<bool> obs,
        TTarget target) where TTarget : StyledElement
    {
        return obs.BindToClasses(target, "Active");
    }

    /// <summary>
    /// Binds the given observable to the given target collection, adding and removing items as needed.
    /// </summary>
    /// <param name="items"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IDisposable BindToUi<T>(this IObservable<IChangeSet<T>> items, AvaloniaList<T> target)
    {
        return items.Subscribe(changeSet =>
        {
            foreach (var change in changeSet)
            {
                switch (change.Reason)
                {
                    case ListChangeReason.AddRange:
                        target.AddRange(change.Range);
                        break;
                    case ListChangeReason.Add:
                        target.Add(change.Item.Current);
                        break;
                    case ListChangeReason.Replace:
                        target.Replace(change.Item.Previous.Value, change.Item.Current);
                        break;
                    default:
                        throw new NotImplementedException($"Change reason not implemented {change.Reason}");
                }
            }
        });
    }
}
