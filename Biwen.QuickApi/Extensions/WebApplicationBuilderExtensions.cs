namespace Biwen.QuickApi;

using Microsoft.AspNetCore.Builder;

/// <summary>
/// <see cref="IApplicationBuilder"/> extension methods.
/// </summary>
[SuppressType]
public static class WebApplicationBuilderExtensions
{

    /// <summary>
    /// Executes the specified action if the specified <paramref name="condition"/> is <c>true</c> which can be
    /// used to conditionally add to the request execution pipeline.
    /// </summary>
    /// <param name="application">The application builder.</param>
    /// <param name="condition">If set to <c>true</c> the action is executed.</param>
    /// <param name="action">The action used to add to the request execution pipeline.</param>
    /// <returns>The same application builder.</returns>
    public static WebApplication UseIf(
        this WebApplication application,
        bool condition,
        Action<WebApplication> action)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(action);
        if (condition)
        {
            action(application);
        }
        return application;
    }

    /// <summary>
    /// Executes the specified <paramref name="ifAction"/> if the specified <paramref name="condition"/> is
    /// <c>true</c>, otherwise executes the <paramref name="elseAction"/>. This can be used to conditionally add to
    /// the request execution pipeline.
    /// </summary>
    /// <param name="application">The application builder.</param>
    /// <param name="condition">If set to <c>true</c> the <paramref name="ifAction"/> is executed, otherwise the
    /// <paramref name="elseAction"/> is executed.</param>
    /// <param name="ifAction">The action used to add to the request execution pipeline if the condition is
    /// <c>true</c>.</param>
    /// <param name="elseAction">The action used to add to the request execution pipeline if the condition is
    /// <c>false</c>.</param>
    /// <returns>The same application builder.</returns>
    public static WebApplication UseIfElse(
        this WebApplication application,
        bool condition,
        Action<WebApplication> ifAction,
        Action<WebApplication> elseAction)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(ifAction);
        ArgumentNullException.ThrowIfNull(elseAction);

        if (condition)
        {
            ifAction(application);
        }
        else
        {
            elseAction(application);
        }
        return application;
    }
}