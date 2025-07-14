using Toletus.SM25.Command.Enums;

namespace Toletus.Hub.Helpers.SM25Helpers;

public static class ReturnCodeHelper
{
    private static IReadOnlyDictionary<ReturnCodes, string> Descriptions { get; } =
        new Dictionary<ReturnCodes, string>
        {
            [ReturnCodes.ERR_SUCCESS] = "Success for command execute",
            [ReturnCodes.ERR_FAIL] = "Fail for instruction execute",
            [ReturnCodes.ERR_VERIFY] = "One to one match fail",
            [ReturnCodes.ERR_IDENTIFY] = "Identify fail, no matched template",
            [ReturnCodes.ERR_TMPL_EMPTY] = "The appointed Template Data is Null",
            [ReturnCodes.ERR_TMPL_NOT_EMPTY] = "Existed template for the appointed ID.",
            [ReturnCodes.ERR_ALL_TMPL_EMPTY] = "All template is Null, Without enrolled Template.",
            [ReturnCodes.ERR_EMPTY_ID_NOEXIST] = "Without available Template ID.",
            [ReturnCodes.ERR_BROKEN_ID_NOEXIST] = "Without damaged Template.",
            [ReturnCodes.ERR_INVALID_TMPL_DATA] = "Appointed Template Data invalid.",
            [ReturnCodes.ERR_DUPLICATION_ID] = "The fingerprint has been enrolled.",
            [ReturnCodes.ERR_BAD_QUALITY] = "Bad quality fingerprint image.",
            [ReturnCodes.ERR_TIME_OUT] = "During Time Out period, no finger is detected",
            [ReturnCodes.ERR_NOT_AUTHORIZED] =
                """
                Not authorized by the password. 
                If password is set and Verify Device Password was not used, all commands (except Test Connection and Verify Device Password) return this error. 
                If no password is set, all commands are available without it.
                """,
            [ReturnCodes.ERR_GENERALIZE] = "Generalize template data fail",
            [ReturnCodes.ERR_FP_CANCEL] = "Relative fingerprint command have been cancelled",
            [ReturnCodes.ERR_INTERNAL] = "Internal error",
            [ReturnCodes.ERR_MEMORY] = "Memory Error",
            [ReturnCodes.ERR_EXCEPTION] = "Firmware error.",
            [ReturnCodes.ERR_INVALID_TMPL_NO] = "Template No. is invalid",
            [ReturnCodes.ERR_INVALID_SEC_VAL] = "The Value of Security Level is invalid",
            [ReturnCodes.ERR_INVALID_TIME_OUT] = "The Value of Time Out is invalid",
            [ReturnCodes.ERR_INVALID_BAUDRATE] = "The Value of Baudrate is invalid",
            [ReturnCodes.ERR_DEVICE_ID_EMPTY] = "Not setting Device ID",
            [ReturnCodes.ERR_INVALID_DUP_VAL] = "Option Value of Duplication Check is invalid",
            [ReturnCodes.ERR_INVALID_PARAM] = "Parameter is invalid.",
            [ReturnCodes.ERR_NO_RELEASE] = "In process of IdentifyFree command, Finger is not released"
        };

    public static bool IsSuccess(ReturnCodes returnCode) =>
        returnCode == ReturnCodes.ERR_SUCCESS;

    public static bool IsError(ReturnCodes returnCode) =>
        returnCode != ReturnCodes.ERR_SUCCESS;

    public static string GetDescription(ReturnCodes returnCode)
    {
        return Descriptions.TryGetValue(returnCode, out var description)
            ? description
            : $"Unknown error code: {returnCode}";
    }
}