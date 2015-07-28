using System;
using System.Web;

namespace WebCommandDevice.ControlDevice
{
    public class DefaultChannelHandler : IHttpAsyncHandler
    {
        private static CometStateManager stateManager;

        static DefaultChannelHandler()
        {
            
        }

        #region IHttpAsyncHandler Members
        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            throw new NotImplementedException();
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IHttpHandler Members
        public bool IsReusable { get; }

        public void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public static CometStateManager StateManager
        {
            get { return stateManager; }
        }

        #endregion
    }

    public class CometStateManager
    {

        public void InitializeClient(String deviceId, Int32 connectionTimeoutSeconds, Int32 connectionIdleSeconds)
        {

        }
    }

    public class CometClient
    {
        public String DeviceId;
    }

    #region
    //public class CometAsyncResult
    //{
    //    private HttpContext _context;
    //    private AsyncCallback _callback;
    //    private Object _extraData;

    //    public CometAsyncResult(HttpContext context, AsyncCallback callback, Object extraData)
    //    {
    //        _context = context;
    //        _callback = callback;
    //        _extraData = extraData;
    //    }

    //    public void BeginWaitRequest()
    //    {

    //    }
    //}

    //public class CometAsyncHandler : IHttpAsyncHandler
    //{
    //    public void ProcessRequest(HttpContext context)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Boolean IsReusable { get; }

    //    public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void EndProcessRequest(IAsyncResult result)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class CometThreadPool
    //{
    //    public void QueueCometWaitRequest(CometWaitRequest request)
    //    {
    //        CometWaitThread waitThread;


    //    }
    //}

    //public class CometWaitRequest
    //{

    //}

    //public class CometWaitThread
    //{

    //}
    #endregion
}