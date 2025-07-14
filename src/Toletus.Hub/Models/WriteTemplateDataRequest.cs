namespace Toletus.Hub.Models;

public class WriteTemplateDataRequest
{
    public Device Device { get; set; }
    public ushort Id { get; set; }
    public byte[] Template { get; set; }
}