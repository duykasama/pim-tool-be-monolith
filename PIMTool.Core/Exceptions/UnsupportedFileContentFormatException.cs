namespace PIMTool.Core.Exceptions;

public class UnsupportedFileContentFormatException : ArgumentException
{
    public override string Message => "File content format is not supportec";
}