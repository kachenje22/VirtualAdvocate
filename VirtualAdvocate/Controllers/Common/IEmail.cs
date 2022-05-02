#region VirtualAdvocate.Common
namespace VirtualAdvocate.Common
{
    #region IEmail
    public interface IEmail
    {
        string[] ToAddress { get; set; }
        string Body { get; set; }
        string Subject { get; set; }
        string[] CCAddress { get; set; }
    } 
    #endregion
} 
#endregion