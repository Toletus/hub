using Toletus.SM25.Command.Enums;

namespace Toletus.Hub.Models;

public class SM25Response
{
    public bool IsResponseComplete { get; set; }
    public byte[] Payload { get; set; }
    public byte[] ReturnRaw { get; set; }
    public byte[] RawData { get; set; }
    public byte[]? Template { get; set; }
    public int FullReturnLen { get; set; }
    public int DataLen { get; set; }
    public ushort Data { get; set; }
    public ushort ChecksumFromReturn { get; set; }
    public ushort ChecksumCalculated { get; set; }
    public ResponsePrefixes Prefix { get; set; }
    public SM25Commands Command { get; set; }
    public GDCodes DataGD { get; set; }
    public TemplateStatus DataTemplateStatus { get; set; }
    public ReturnCodes DataReturnCode { get; set; }
    public ReturnCodes ReturnCode { get; set; }
    public bool ChecksumIsValid { get; set; }
    public int ResponseLenghtExpected { get; set; }
    
    public static SM25Response? ToResponse(Toletus.SM25.Command.SM25Response? response)
    {
        if (response == null)
            return null;
        
        return new SM25Response
        {
            IsResponseComplete = response.IsResponseComplete,
            Payload = response.Payload,
            ReturnRaw = response.ReturnRaw,
            RawData = response.RawData,
            Template = response.Template,
            FullReturnLen = response.FullReturnLen,
            DataLen = response.DataLen,
            Data = response.Data,
            ChecksumFromReturn = response.ChecksumFromReturn,
            ChecksumCalculated = response.ChecksumCalculated,
            Prefix = response.Prefix,
            Command = response.Command,
            DataGD = response.DataGD,
            DataTemplateStatus = response.DataTemplateStatus,
            DataReturnCode = response.DataReturnCode,
            ReturnCode = response.ReturnCode,
            ChecksumIsValid = response.ChecksumIsValid,
            ResponseLenghtExpected = response.ResponseLenghtExpected,
        };
    }
}