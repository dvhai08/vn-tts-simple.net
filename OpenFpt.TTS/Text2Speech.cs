using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Script.Serialization;

// thư viện hỗ trợ đấu nối với dịch vụ TTS.
// tác giả Lý Tuấn Anh
// email: lytuananh2003@gmail.com
namespace OpenFpt.TTS
{
    public class AsyncResponseData
    {
        // đường dẫn đến file Mp3.
        public string audio_menv_url { get; set; }
        //--audio_wodmenv_url:
        //--labler_url: 
        // request ID là đại diện cho text theo dạng hashcode.
        public string request_id { get; set; }
        // trường tuy biến của người dùng để phân lại nội dung văn bản
        public string catalog_name { get; set; }
        // trường tuy biến của người dùng để phân lại nội dung văn bản
        public string sub_catalog_name { get; set; }
        // phiên bản của TTS engin. lastest version 3.4.1
        public string engin_version { get; set; }
    }

    public class Text2Speech
    {
        /// <summary>
        /// link đến dịch vụ đón kết quả của người dùng.
        /// </summary>
        private string _asyncLink;

        /// <summary>
        /// key của người dùng, token này được cung cấp bởi dev.OpenFpt.vn.
        /// </summary>
        private string _token;

        /// <summary>
        /// link đến dịch vụ đón kết quả của người dùng.
        /// </summary>
        public string AsyncLink
        {
            get
            {
                return _asyncLink;

            }
        }

        /// <summary>
        /// key của người dùng, token này được cung cấp bởi dev.OpenFpt.vn.
        /// </summary>
        public string Token
        {
            get
            {
                return _token;
            }
        }

        /// <summary>
        /// hàm khởi tạo đối tượng Text2Speech
        /// </summary>
        /// <param name="token">key của người dùng, token này được cung cấp bởi dev.OpenFpt.vn.</param>
        /// <param name="asyncLink">link đến dịch vụ đón kết quả của người dùng.</param>
        public Text2Speech(string token, string asyncLink=null)
        {
            _asyncLink = asyncLink;
            _token = token;
        }

        /// <summary>
        /// hàm request đến OpenFPT service, để chuyển từ văn bản sang giọng nói.
        /// </summary>
        /// <param name="text">nội dung văn bản.</param>
        /// <param name="categoryName">thông tin tùy biến của người dùng để phân loại văn bản.</param>
        /// <param name="subCategoryName">thông tin tùy biến của người dùng để phân loại văn bản.</param>
        /// <returns></returns>
        public AsyncResponseData Speech(string text, string categoryName = null, string subCategoryName = null)
        {
            const int MAX_REQUEST = 60;
            string link = "http://api.openfpt.vn/text2speech";
            HttpResponseMessage msg = Request(link, text, categoryName, subCategoryName);
            if (msg.StatusCode != HttpStatusCode.OK)
                throw new Exception(msg.Content.ToString());

            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            AsyncResponseData data = jsonSerializer.Deserialize<AsyncResponseData>(msg.Content.ToString());
            if (!string.IsNullOrEmpty(_asyncLink))
            {
                int i = 0;
                HttpResponseMessage asyncMsg = null;
                while (i < MAX_REQUEST)
                {
                    i++;
                    asyncMsg = Request(_asyncLink + "?request_id=" + data.request_id);
                    if (asyncMsg.StatusCode != HttpStatusCode.OK)
                    {
                        Thread.Sleep(5000);
                    }
                }
                if(asyncMsg == null || asyncMsg.StatusCode != HttpStatusCode.OK)
                    throw new Exception("can not get mp3 from link: " + _asyncLink);

                return jsonSerializer.Deserialize<AsyncResponseData>(asyncMsg.Content.ToString());
            }
            return data;
        }

        /// <summary>
        /// request đến máy chủ OpenFpt để lấy file Mp3.
        /// </summary>
        /// <param name="link"></param>
        /// <param name="text"></param>
        /// <param name="categoryName"></param>
        /// <param name="subCategoryName"></param>        
        /// <returns></returns>
        private HttpResponseMessage Request(string link, string text = null, string categoryName = null, string subCategoryName = null)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("cache-control", "no-cache");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("api_key", _token);
            if (string.IsNullOrEmpty(text))
            {
                if (!string.IsNullOrEmpty(_asyncLink))
                    client.DefaultRequestHeaders.Add("async", _asyncLink);
                if (!string.IsNullOrEmpty(categoryName))
                    client.DefaultRequestHeaders.Add("category_name", categoryName);
                if (!string.IsNullOrEmpty(subCategoryName))
                    client.DefaultRequestHeaders.Add("sub_category_name", subCategoryName);
                return client.PostAsync(link, new StringContent(text)).Result;
            }
            return client.GetAsync(link).Result;
        }
    }
}
