using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace PMS.Filters
{
    public class CustomModifyingErrorMessageDelegatingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //if (request.RequestUri.ToString() != "http://localhost/api/account/ShowCaptcha")
            //{

            //}
            //return null;
            return base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>((responseToCompleteTask) =>
            {
                HttpResponseMessage response = responseToCompleteTask.Result;

                if (response.TryGetContentValue<HttpError>(out HttpError error))
                {
                    error.Message = "Có lỗi xảy ra";
                    error.MessageDetail = "Có lỗi xảy ra";
                }
                return response;
            });
        }
    }
}