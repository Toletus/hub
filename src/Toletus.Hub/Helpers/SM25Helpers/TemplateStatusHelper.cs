using Toletus.SM25.Command.Enums;

namespace Toletus.Hub.Helpers.SM25Helpers;

public static class TemplateStatusHelper
{
    private static IReadOnlyDictionary<TemplateStatus, string> Descriptions { get; } =
        new Dictionary<TemplateStatus, string>
        {
            [TemplateStatus.GD_TEMPLATE_NOT_EMPTY]    = "The appointed Template are not empty",
            [TemplateStatus.GD_TEMPLATE_EMPTY]       = "The appointed Template have been emptied",
        };
    
    public static string GetDescription(TemplateStatus templateStatus)
    {
        return Descriptions.TryGetValue(templateStatus, out var description)
            ? description
            : $"Unknown error code: {templateStatus}";
    }
}