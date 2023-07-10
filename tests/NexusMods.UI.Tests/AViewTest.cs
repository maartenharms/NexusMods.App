using System.Linq.Expressions;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.App.UI;
using NexusMods.UI.Tests.Framework;
using Noggog;
using ReactiveUI;

namespace NexusMods.UI.Tests;

public abstract class AViewTest<TView, TViewModel, TViewModelInterface> : AUiTest, IAsyncLifetime
    where TViewModelInterface : class, IViewModelInterface
    where TView : ReactiveUserControl<TViewModelInterface>, new()
    where TViewModel : AViewModel<TViewModelInterface>, TViewModelInterface
{
    private readonly AvaloniaApp _app;
    private ControlHost<TView, TViewModel, TViewModelInterface>? _host;
    private readonly bool _useDiForVm;


    protected ControlHost<TView, TViewModel, TViewModelInterface> Host => _host!;
    protected TViewModel ViewModel => _host!.ViewModel;
    protected TView View => _host!.View;

    /// <summary>
    /// Creates a new view test, if useDiForVm is true, the viewmodel will be resolved from the DI container otherwise
    /// it will be created by its default constructor.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="useDiForVm"></param>
    protected AViewTest(IServiceProvider provider, bool useDiForVm = false) : base(provider)
    {
        _useDiForVm = useDiForVm;
        _app = provider.GetRequiredService<AvaloniaApp>();
    }

    protected async Task<T> GetControl<T>(string name) where T : Control
    {
        return await _host!.GetViewControl<T>(name);
    }

    protected async Task<T[]> GetVisualDescendants<T>(Control parent) where T : Control
    {
        return await OnUi(() =>
        {
            Control[] GetChildren(Control control, bool topLevel)
            {
                var children = control.GetVisualChildren()
                    .SelectMany(c => GetChildren((Control)c, false));
                if (!topLevel)
                    return children.StartWith(control).ToArray();
                return children.ToArray();
            }

            var res = GetChildren(parent, true).OfType<T>().ToArray();
            return Task.FromResult(res);
        });
    }
    
    public virtual async Task InitializeAsync()
    {

        if (_useDiForVm)
        {
            _host = await _app.GetControl<TView, TViewModel, TViewModelInterface>(() => (TViewModel)Provider.GetRequiredService<TViewModelInterface>());
        }
        else
        {
            _host = await _app.GetControl<TView, TViewModel, TViewModelInterface>(() => Activator.CreateInstance<TViewModel>());
        }

        await PostInitializeSetup();
    }

    protected virtual Task PostInitializeSetup()
    {
        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        if (_host != null)
            await _host.DisposeAsync();
    }

    /// <summary>
    /// Tests that clicking the button with the given name fires the command found on the viewmodel under the given expression.
    /// </summary>
    /// <param name="commandExpression"></param>
    /// <param name="buttonName"></param>
    protected async Task ButtonShouldFireCommand(Expression<Func<TViewModelInterface, ICommand>> commandExpression,
        string buttonName)
    {

        // Create a TCS so that we can properly sync between threads
        var src = new TaskCompletionSource<bool>();

        // Recompile the expression into a setter
        // if we're handed vm => vm.Click
        // we'll rewrite that into (vm, newValue) => vm.Click = newValue
        var setterParam = Expression.Parameter(typeof(ICommand), "newValue");
        var setter = Expression.Lambda<Action<TViewModelInterface, ICommand>>(Expression.Assign(commandExpression.Body, setterParam),
            commandExpression.Parameters.First(), setterParam).Compile();

        var cmd = ReactiveCommand.Create(() => src.SetResult(true));
        setter(ViewModel, cmd);

        var button = await Host.GetViewControl<Button>(buttonName);

        // Make sure the wiring has all been done
        await EventuallyOnUi(() =>
        {
            button.Command.Should().Be(cmd);
        });

        // Click the button
        await Click(button);

        // Wait for the command to fire
        (await src.Task.WaitAsync(TimeSpan.FromSeconds(10))).Should().BeTrue();
    }
}

public class AViewTest<TView> : AUiTest, IAsyncLifetime
    where TView : Control, new()
{
    private readonly AvaloniaApp _app;
    private ControlHost<TView>? _host;


    protected ControlHost<TView> Host => _host!;
    protected TView View => _host!.View;

    protected AViewTest(IServiceProvider provider) : base(provider)
    {
        _app = provider.GetRequiredService<AvaloniaApp>();
    }

    protected async Task<T> GetControl<T>(string name) where T : Control
    {
        return await _host!.GetViewControl<T>(name);
    }

    protected async Task<T[]> GetVisualDescendants<T>(Control parent) where T : Control
    {
        return await OnUi(() =>
        {
            Control[] GetChildren(Control control, bool topLevel)
            {
                var children = control.GetVisualChildren()
                    .SelectMany(c => GetChildren((Control)c, false));
                if (!topLevel)
                    return children.StartWith(control).ToArray();
                return children.ToArray();
            }

            var res = GetChildren(parent, true).OfType<T>().ToArray();
            return Task.FromResult(res);
        });
    }


    public virtual async Task InitializeAsync()
    {
        _host = await _app.GetControl<TView>();
        await PostInitializeSetup();
    }

    protected virtual Task PostInitializeSetup()
    {
        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        if (_host != null)
            await _host.DisposeAsync();
    }
}
