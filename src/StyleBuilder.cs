using System.Text;

namespace WebOS;

public sealed class StyleBuilder
{
    private readonly StringBuilder _stringBuilder = new StringBuilder();

    public StyleBuilder AddStyle(string? prop, string? value)
    {
        if (prop != null && value != null)
        {
            _stringBuilder.Append($"{prop}:{value};");
        }
        return this;
    }

    public StyleBuilder AddStyle(string? style)
    {
        if (!string.IsNullOrEmpty(style))
        {
            _stringBuilder.Append(style);
            if (!style.EndsWith(';'))
            {
                _stringBuilder.Append(';');
            }
        }
        return this;
    }

    public string Build()
        => _stringBuilder.ToString();
}