namespace RealTimeCommunication.Hubs.FilterAttribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LanguageFilterAttribute : Attribute
{
    /// <summary>
    /// Índice do argumento do método que será filtrado.
    /// Exemplo:
    /// 0 = primeiro argumento
    /// 1 = segundo argumento
    /// </summary>
    public int FilterArgument { get; }

    public LanguageFilterAttribute(int filterArgument)
    {
        FilterArgument = filterArgument;
    }
}