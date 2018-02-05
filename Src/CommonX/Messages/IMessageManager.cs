namespace CommonX.Messages
{
    public interface IMessageManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exceptionTypeName"></param>
        /// <returns></returns>
        string GetErrorCode(string exceptionTypeName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageKey"></param>
        /// <returns></returns>
        Message GetMessage(MessageKey messageKey);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageKey"></param>
        /// <param name="messageVariables"></param>
        /// <returns></returns>
        Message GetMessage(MessageKey messageKey, string[] messageVariables);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageKey"></param>
        /// <param name="cultureName"></param>
        /// <param name="messageVariables"></param>
        /// <returns></returns>
        Message GetMessage(MessageKey messageKey, string cultureName,  string[] messageVariables);
    }
}
