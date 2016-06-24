using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NAudio.Wave;
using System.Threading.Tasks;
using OpenFpt.TTS;

namespace PlayText2Speech
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (button1.Text.Equals("Play"))
            {
                Start();
            }
            else
            {
                Cancel();
            }
        }

        CancellationTokenSource _cts;
        
        public void Start()
        {
            // do something
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            string url = this.textBox1.Text;
            string text = this.richTextBox1.Text;
            string mytoken = txtToken.Text;
            Voice voice = comboBox1.Text.Equals("male") ? Voice.Male : Voice.Female;
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("bạn cần nhập nội dung văn bản vào ô text ở dưới");
                return;
            }
            button1.Text = "Stop";
            Task.Factory.StartNew(() =>
            {
                Console.WriteLine("Start");
                PlayMp3FromUrl(url, text, voice, mytoken);
            }, token);
        }

        public void Cancel()
        {
            Console.WriteLine("Cancel");
            //_run = false;
            //button1.Text = "Play";
            //button1.Enabled = false;

            InnerControl();
            Thread.Sleep(100);
            // stop that.
            _cts?.Cancel();
            //button1.Enabled = true;
        }

        private WaveOut _waveOut;
        public void PlayMp3FromUrl(string url, string text, Voice voice, string mytoken)
        {
            Text2Speech tts = new Text2Speech(mytoken, url);
            
            AsyncResponseData responseData = tts.Speech(text, voice);
            // nhận một object chứa link của file Mp3.
            Console.WriteLine(responseData.audio_menv_url);
            
            WebClient wc = new WebClient();

            wc.DownloadFile(responseData.audio_menv_url ?? responseData.async, responseData.request_id + ".mp3");

            FileStream memoryStream = File.OpenRead(responseData.request_id + ".mp3");
            
            try
            {
                memoryStream.Position = 0;
                using (
                    WaveStream blockAlignedStream =
                        new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(memoryStream))))
                {
                    using (_waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        _waveOut.Init(blockAlignedStream);
                        InnerControl(false);
                        while (_waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(100);
                        }
                        InnerControl();
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                InnerControl();
            }
        }

        void InnerControl(bool isStop = true)
        {
            Invoke((MethodInvoker)delegate
            {
                button1.Text = isStop? "Play": "Stop";
                if (isStop)
                    _waveOut?.Stop();
                else
                    _waveOut?.Play();
            });
        }       
    }
}
