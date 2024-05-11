using KafkaClient.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace KafkaClient.Utils.Handlers;
internal class HandlerProvider(IEnumerable<Type> handlerTypes) : IHandlerProvider
{
    #region Fields

    protected IReadOnlyDictionary<string, IEnumerable<Type>> GroupHandlers { get; } = CreateGroupHandlerTypeMap(handlerTypes);

    #endregion

    #region Methods

    public IEnumerable<IAsyncHandler> GetAsyncHandlers(string group, IServiceProvider provider) =>
            GroupHandlers.GetValueOrDefault(group)
                ?.Where(type => type.IsAssignableTo(typeof(IAsyncHandler)))
                .Select(provider.GetRequiredService)
                .Cast<IAsyncHandler>() ?? [];

    public IEnumerable<IHandler> GetHandlers(string group, IServiceProvider provider) =>
            GroupHandlers.GetValueOrDefault(group)
                ?.Where(type => type.IsAssignableTo(typeof(IHandler)))
                .Select(provider.GetRequiredService)
                .Cast<IHandler>() ?? [];

    #endregion

    #region Private Util

    private static Dictionary<string, IEnumerable<Type>> CreateGroupHandlerTypeMap(IEnumerable<Type> types)
    {
        var map = new Dictionary<string, IEnumerable<Type>>();
        foreach (var type in types)
        {
            if (!(type.IsAssignableTo(typeof(IAsyncHandler)) || type.IsAssignableTo(typeof(IHandler))))
                continue;

            foreach (var att in type.GetCustomAttributes<HandleGroupAttribute>())
            {
                var current = map.GetValueOrDefault(att.Group) ?? [];

                map[att.Group] = current.Append(type);
            }
        }
        return map;
    }

    #endregion
}
