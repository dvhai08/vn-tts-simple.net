using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Script.Serialization;
using OpenFpt.TTS;

namespace AsyncTtsService.Controllers
{
    public class TtsController : ApiController
    {
        // khai báo tên domain và đường dẫn đến thư mục chứa dữ liệu của bạn.
        private const string DOMAIN = "http://localhost:56299/data";

        /// <summary>
        /// hàm hỗ trợ lấy kết quả trả về từ phía Client.
        ///  GET api/tts?request_id=xxxxxxxxxxxxxxxxxx
        /// </summary>
        /// <param name="request_id"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(string request_id)
        {
            string forderPath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Data");
            string json = File.ReadAllText(Path.Combine(forderPath, request_id + ".json"));
            HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new StringContent(json,Encoding.UTF8, "aplication/json");
            return resp;
        }
        /// <summary>
        /// hàm đón kết quả trả về từ OpenFpt service.
        /// </summary>
        /// <param name="request">nội dung kết quả trả về.</param>
        // POST api/tts
        [HttpPost]
        public void Post(HttpRequestMessage request)
        {
            try
            {
                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                string content = request.Content.ReadAsStringAsync().Result;
                AsyncResponseData data = jsonSerializer.Deserialize<AsyncResponseData>(content);
                string forderPath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Data");
                WebClient webClient = new WebClient();
                string filename = Path.GetFileName(data.audio_menv_url);
                string filepath = Path.Combine(forderPath, filename);
                webClient.DownloadFile(data.audio_menv_url, filepath);
                data.audio_menv_url = Path.Combine(DOMAIN, filename);
                
                File.WriteAllText(Path.Combine(forderPath, data.request_id + ".json"), jsonSerializer.Serialize(data));
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// hàm dùng kiểm tra link đón có sống không?
        /// </summary>
        /// <returns>trả về HttpStatusCode là 200.</returns>
        // Head api/tts
        public HttpResponseMessage Head()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
