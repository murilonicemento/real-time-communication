using Microsoft.AspNetCore.SignalR;
using RealTimeCommunication.Hubs.FilterAttribute;

namespace RealTimeCommunication.Hubs;

public class LanguageFilterHub : IHubFilter
{
    // Lista de palavras/expressões proibidas
    // Pode ser carregada de arquivo ou inline
    private List<string> bannedPhrases = new List<string> { "async void", ".Result" };

    public async ValueTask<object> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object>> next)
    {
        // Tenta obter o atributo LanguageFilter aplicado no método do Hub
        var languageFilter = (LanguageFilterAttribute)Attribute.GetCustomAttribute(
            invocationContext.HubMethod, typeof(LanguageFilterAttribute));

        // Se o atributo existe e o argumento existe e é string
        if (languageFilter != null &&
            invocationContext.HubMethodArguments.Count > languageFilter.FilterArgument &&
            invocationContext.HubMethodArguments[languageFilter.FilterArgument] is string str)
        {
            // Substitui todas as palavras proibidas por "***"
            foreach (var bannedPhrase in bannedPhrases)
            {
                str = str.Replace(bannedPhrase, "***");
            }

            // Cria novo array de argumentos com o valor filtrado
            var arguments = invocationContext.HubMethodArguments.ToArray();
            arguments[languageFilter.FilterArgument] = str;

            // Atualiza o contexto de invocação para usar os argumentos filtrados
            invocationContext = new HubInvocationContext(
                invocationContext.Context,
                invocationContext.ServiceProvider,
                invocationContext.Hub,
                invocationContext.HubMethod,
                arguments);
        }

        // Continua a execução do método original no Hub
        return await next(invocationContext);
    }
}