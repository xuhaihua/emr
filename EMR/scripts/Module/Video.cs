using UnityEngine.Video;
using EMR.Event;
using EMR.Entity;
using EMR.Struct;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EMR.Module
{
    public class Video
    {
        #region 基本字段
        private Node _node;
        private VideoPlayer _videoPlayer;

        /// <summary>
        /// 开始事件
        /// </summary>
        public VideoPlayEvent onVideoPlay;

        /// <summary>
        /// 新帧准备就绪事件
        /// </summary>
        public VideoFrameReadyEvent onVideoFrameReady;

        /// <summary>
        /// VideoPlayer 的准备工作完成事件
        /// </summary>
        public VideoReadyEvent onVideoReady;

        /// <summary>
        /// 视频解码器在播放期间没有按照时间源生成帧时事件
        /// </summary>
        public VideoFrameDroppedEvent onVideoFrameDropped;

        /// <summary>
        /// HTTP 连接问题等错误事件
        /// </summary>
        public VideoErrorEvent onVideoError;

        /// <summary>
        /// VideoPlayer 到达播放内容的结尾事件
        /// </summary>
        public VideoEndEvent onVideoEnd;

        /// <summary>
        /// 搜寻操作完成事件
        /// </summary>
        public VideoPlaySeekCompletedEvent onVideoPlaySeekCompleted;

        public ClockResyncOccurredEvent onClockResyncOccurred;

        private Audio _audio = null;
        #endregion

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="node">视频所属节点</param>
        public Video(Node node)
        {
            this._node = node;

            this._audio = new Audio(node);


            // 初始化video事件
            this.onVideoPlay = new VideoPlayEvent();
            this.onVideoFrameReady = new VideoFrameReadyEvent();
            this.onVideoReady = new VideoReadyEvent();
            this.onVideoFrameDropped = new VideoFrameDroppedEvent();
            this.onVideoError = new VideoErrorEvent();
            this.onVideoEnd = new VideoEndEvent();
            this.onVideoPlaySeekCompleted = new VideoPlaySeekCompletedEvent();
            this.onClockResyncOccurred = new ClockResyncOccurredEvent();


            // 添加VideoPlayer组件
            this._videoPlayer = this._node.parasitifer.AddComponent<VideoPlayer>();

            // 手动控制video播放
            this._videoPlayer.playOnAwake = false;

            // 设置source类型为url
            this._videoPlayer.source = VideoSource.Url;

            // 默认output mode为direct
            this._videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

            /*
             * 以下逻辑用于video抛出相应事件
             */
            this._videoPlayer.errorReceived += (VideoPlayer source, string message) =>
            {
                VideoErrorEventData eventData = new VideoErrorEventData
                {
                    source = source,
                    message = message
                };

                this.onVideoError.Invoke(eventData);
            };

            this._videoPlayer.frameDropped += (VideoPlayer source) =>
            {
                VideoFrameDroppedEventData eventData = new VideoFrameDroppedEventData
                {
                    source = source
                };

                this.onVideoFrameDropped.Invoke(eventData);
            };

            this._videoPlayer.seekCompleted += (VideoPlayer source) =>
            {
                VideoPlaySeekCompletedEventData eventData = new VideoPlaySeekCompletedEventData
                {
                    source = source
                };

                this.onVideoPlaySeekCompleted.Invoke(eventData);
            };

            this._videoPlayer.clockResyncOccurred += (VideoPlayer source, double seconds) =>
            {
                ClockResyncOccurredEventData eventData = new ClockResyncOccurredEventData
                {
                    source = source,
                    seconds = seconds
                };

                this.onClockResyncOccurred.Invoke(eventData);
            };

            this._videoPlayer.frameReady += (VideoPlayer source, long frameIdx) =>
            {
                VideoFrameReadyEventData eventData = new VideoFrameReadyEventData
                {
                    source = source,
                    frameIdx = frameIdx
                };

                this.onVideoFrameReady.Invoke(eventData);
            };

            this._videoPlayer.started += (VideoPlayer source) =>
            {
                VideoPlayEventData eventData = new VideoPlayEventData
                {
                    source = source
                };

                this.onVideoPlay.Invoke(eventData);
            };

            this._videoPlayer.loopPointReached += (VideoPlayer source) =>
            {
                VideoEndEventData eventData = new VideoEndEventData
                {
                    source = source
                };

                this.onVideoEnd.Invoke(eventData);
            };

            this._videoPlayer.prepareCompleted += (VideoPlayer source) =>
            {
                VideoReadyEventData eventData = new VideoReadyEventData
                {
                    source = source
                };

                this.onVideoReady.Invoke(eventData);
            };

            this._videoPlayer.SetTargetAudioSource(0, this._audio.audioSource);
            this._videoPlayer.SetTargetAudioSource(0, this._audio.audioSource);
            this._videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        }

        #region 基本属性
        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool isPlaying
        {
            get
            {
                return this._videoPlayer.isPlaying;
            }
        }

        /// <summary>
        /// 视频是否循环播放
        /// </summary>
        public bool isLooping
        {
            get
            {
                return this._videoPlayer.isLooping;
            }

            set
            {
                this._videoPlayer.isLooping = value;
            }
        }

        private string _url = "";
        /// <summary>
        /// 视频url
        /// </summary>
        public string url
        {
            get
            {
                return this._url;
            }

            set
            {
                string reg = @"^(http(s)?:\/\/)?(www\.)?[\w-]+(\.\w{2,4})?\.\w{2,4}?(\/)?$";
                Regex r = new Regex(reg);
                Match m = r.Match(value.Trim());

                if (m.Success)
                {
                    this._videoPlayer.url = value;
                    this._url = value.Trim();
                }
                else
                {
                    this._videoPlayer.clip = Resources.Load<VideoClip>(value.Trim());
                    this._url = value.Trim();
                }
            }
        }

        /// <summary>
        /// 视频是否已准备好
        /// </summary>
        public bool isPrepared
        {
            get
            {
                return this._videoPlayer.isPrepared;
            }
        }

        /// <summary>
        /// 音量
        /// </summary>
        public float volume
        {
            get
            {
                return this._audio.volume;
            }

            set
            {
                this._audio.volume = value;
            }
        }

        /// <summary>
        /// 播放速度
        /// </summary>
        public float speed
        {
            get
            {
                return this._videoPlayer.playbackSpeed;
            }

            set
            {
                this._videoPlayer.playbackSpeed = value;
            }
        }

        /// <summary>
        /// 音频模式
        /// </summary>
        public AudioMode audioMode
        {
            get
            {
                return this._audio.audioMode;
            }

            set
            {
                this._audio.audioMode = value;
            }
        }
        #endregion

        #region 基本操作方法
        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="url">播放地址</param>
        public void play()
        {
            if (this.url != null && !this.isPlaying)
            {
                this._videoPlayer.Play();
            }
        }

        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="url">播放地址</param>
        public void play(string url)
        {

            if (url != null)
            {
                this.url = url;
            }

            if (this.url != null && !this.isPlaying)
            {
                this._videoPlayer.Play();
            }
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void pause()
        {
            if(this.isPlaying)
            {
                this._videoPlayer.Pause();
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void stop()
        {
            if(this.isPlaying)
            {
                this._videoPlayer.Stop();
            }
        }
        #endregion
    }
}
