using Toletus.SM25.Command.Enums;

namespace Toletus.Hub.Helpers.SM25Helpers;

public static class GdCodeHelper
{
    private static IReadOnlyDictionary<GDCodes, string> Descriptions { get; } =
        new Dictionary<GDCodes, string>
        {
            [GDCodes.GD_NO_DETECT_FINGER]    = "Detect no any finger on sensor when execute 'Finger Detect' command",
            [GDCodes.GD_DETECT_FINGER]       = "Detect an finger on sensor when execute 'Finger Detect' command",
            [GDCodes.GD_DOWNLOAD_SUCCESS]    = "Download Template data was successful",
            [GDCodes.GD_NEED_FIRST_SWEEP]    = "Waiting input fingerprint for first time.",
            [GDCodes.GD_NEED_SECOND_SWEEP]   = "Waiting input fingerprint for second time.",
            [GDCodes.GD_NEED_THIRD_SWEEP]    = "Waiting input fingerprint for third time.",
            [GDCodes.GD_NEED_RELEASE_FINGER] = "Lift finger"
        };
    
    public static string GetDescription(GDCodes gdCodes)
    {
        return Descriptions.TryGetValue(gdCodes, out var description)
            ? description
            : $"Unknown code: {gdCodes}";
    }
}
