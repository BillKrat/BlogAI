<%@ Application Language="C#" %>
<%@ Import Namespace="BlogEngine.NET.App_Start" %>

<script RunAt="server">
    void Application_BeginRequest(object sender, EventArgs e)
    {
        var app = (HttpApplication)sender;
        BlogEngineConfig.Initialize(app.Context);
    }
        
    void Application_PreRequestHandlerExecute(object sender, EventArgs e)
    {
        BlogEngineConfig.SetCulture(sender, e);
    }

    // Unhandled exceptions used to vanish (customErrors hides them from visitors
    // and nothing wrote them down). Send them to the standard BlogEngine log
    // (App_Data/logger.txt and the dashboard log view).
    void Application_Error(object sender, EventArgs e)
    {
        try
        {
            var ex = Server.GetLastError();
            if (ex == null) return;

            var http = ex as HttpException;
            if (http != null && http.GetHttpCode() == 404) return;

            var inner = ex.GetBaseException();
            var url = Context != null && Context.Request != null ? Context.Request.RawUrl : "?";
            BlogEngine.Core.Utils.Log((object)string.Format("Unhandled error at {0}: {1}: {2}{3}{4}",
                url, inner.GetType().FullName, inner.Message, Environment.NewLine, inner.StackTrace));
        }
        catch
        {
            // logging must never throw from the error handler
        }
    }
</script>