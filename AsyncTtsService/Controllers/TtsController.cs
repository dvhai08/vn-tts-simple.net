using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Script.Serialization;
using OpenFpt.TTS;

namespace AsyncTtsService.Controllers
{
    public class TtsController : ApiController
    {
        // khai báo tên domain và đường dẫn đến thư mục chứa dữ liệu của bạn.
        private const string DOMAIN = "http://localhost:8080/data";

        /// <summary>
        /// hàm hỗ trợ lấy kết quả trả về từ phía Client
        /// </summary>
        /// <param name="request_id"></param>
        /// <returns></returns>
        // GET api/tts?request_id=xxxxxxxxxxxxxxxxxx
        public string Get(string request_id)
        {
            string forderPath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Data");
            return File.ReadAllText(forderPath + request_id + ".json"); ;
        }
        /// <summary>
        /// hàm đón kết quả trả về từ OpenFpt service.
        /// </summary>
        /// <param name="value">nội dung kết quả trả về.</param>
        // POST api/tts
        public void Post([FromBody]string value)
        {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            AsyncResponseData data = jsonSerializer.Deserialize<AsyncResponseData>(value);
            string forderPath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Data");
            WebClient webClient = new WebClient();
            string filename = Path.GetFileName(data.audio_menv_url);
            string filepath = Path.Combine(forderPath, filename);
            webClient.DownloadFile(data.audio_menv_url, filepath);
            data.audio_menv_url = DOMAIN + filename;

            File.WriteAllText(forderPath + data.request_id + ".json", jsonSerializer.Serialize(data));
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
