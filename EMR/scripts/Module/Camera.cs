using UnityEngine.Windows.WebCam;
using System.Linq;
using UnityEngine;
using EMR.Event;
using EMR.Struct;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

namespace EMR.Module
{
    public class Camera
    {
        #region 基本字段
        /// <summary>
        /// 照片保存路径
        /// </summary>
        private string _phoneFilePath;

        /// <summary>
        /// PhotoCapture
        /// </summary>
        private PhotoCapture _photoCaptureObject;

        /// <summary>
        /// PhotoCaptureFrame
        /// </summary>
        private PhotoCaptureFrame _photoCaptureFrame;

        /// <summary>
        /// 视频保存路径
        /// </summary>
        private string _videoFilePath;

        /// <summary>
        /// VideoCapture
        /// </summary>
        private VideoCapture _videoCaptureObject = null;
        #endregion

        #region 属性
        /// <summary>
        /// Camera当前是否可用
        /// </summary>
        public bool isUsable
        {
            get
            {
                return WebCam.Mode == WebCamMode.None;
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 拍照开始事件
        /// </summary>
        public PhoneStartEvent onPhoneStart = new PhoneStartEvent();

        /// <summary>
        /// 拍照完成事件
        /// </summary>
        public PhoneCompleteEvent onPhoneComplete = new PhoneCompleteEvent();

        /// <summary>
        /// 开始录像事件
        /// </summary>
        public VideoStartRecordingEvent onStartRecording = new VideoStartRecordingEvent();

        /// <summary>
        /// 录像结束事件
        /// </summary>
        public VideoStopRecordingEvent onStopRecording = new VideoStopRecordingEvent();
        #endregion

        private static Vector3 UnProjectVector(Matrix4x4 proj, Vector3 to)
        {
            Vector3 from = new Vector3(0, 0, 0);
            var axsX = proj.GetRow(0);
            var axsY = proj.GetRow(1);
            var axsZ = proj.GetRow(2);
            from.z = to.z / axsZ.z;
            from.y = (to.y - (from.z * axsY.z)) / axsY.y;
            from.x = (to.x - (from.z * axsX.z)) / axsX.x;
            return from;
        }

        /// <summary>
        /// 获取投影信息
        /// </summary>
        /// <param name="vector">屏幕点</param>
        /// <param name="photoCaptureFrame">PhotoCaptureFrame</param>
        /// <returns></returns>
        public static PhoneProjection? getPhoneProjection(Vector2 vector, PhotoCaptureFrame photoCaptureFrame)
        {
            PhoneProjection result;
            Matrix4x4 cameraToWorldMatrix;
            bool isSuccess1 = photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);

            Matrix4x4 projectionMatrix;
            bool isSuccess2 = photoCaptureFrame.TryGetProjectionMatrix(out projectionMatrix);

            if (!isSuccess1) { return null; }
            if (!isSuccess2) { return null; }

            var imagePosZeroToOne = new Vector2(vector.x / 3904, 1 - (vector.y / 2196));
            var imagePosProjected = (imagePosZeroToOne * 2) - new Vector2(1, 1);    // -1 to 1 space

            var cameraSpacePos = UnProjectVector(projectionMatrix, new Vector3(imagePosProjected.x, imagePosProjected.y, 1));
            var worldSpaceCameraPos = cameraToWorldMatrix.MultiplyPoint(Vector3.zero);
            var worldSpaceBoxPos = cameraToWorldMatrix.MultiplyPoint(cameraSpacePos);

            result = new PhoneProjection(worldSpaceCameraPos, worldSpaceBoxPos - worldSpaceCameraPos);
            return result;
        }

        #region 拍照方法
        /// <summary>
        /// 拍照
        /// </summary>
        public void takePhoto(string name = null)
        {
            var cameraMode = WebCam.Mode;
            if (cameraMode == WebCamMode.None)
            {
                if (name != null)
                {
                    this._phoneFilePath = System.IO.Path.Combine(Application.persistentDataPath, name);
                }
                PhotoCapture.CreateAsync(true, OnPhotoCaptureCreated);
            }
            else
            {
                Debug.LogError("当前相机不可用！");
            }
        }

        private void OnPhotoCaptureCreated(PhotoCapture captureObject)
        {
            this.onPhoneStart.Invoke(new PhoneStartEventData
            {
                photoCapture = captureObject
            });

            this._photoCaptureObject = captureObject;
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            CameraParameters cameraParm = new CameraParameters();
            cameraParm.hologramOpacity = 1.0f;
            cameraParm.cameraResolutionWidth = cameraResolution.width;
            cameraParm.cameraResolutionHeight = cameraResolution.height;
            cameraParm.pixelFormat = CapturePixelFormat.BGRA32;
            captureObject.StartPhotoModeAsync(cameraParm, OnPhotoModeStarted);
        }

        private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
        {
            if (result.success)
            {
                this._photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
            }
            else
            {
                Debug.LogError("无法开启拍照模式!");
            }
        }

        private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
        {
            if (result.success)
            {
                this._photoCaptureFrame = photoCaptureFrame;

                if (this._phoneFilePath != null)
                {
                    this._photoCaptureObject.TakePhotoAsync(this._phoneFilePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhoneToDisk);
                }
                else
                {
                    this.onPhoneComplete.Invoke(new PhoneCompleteEventData
                    {
                        photoCaptureFrame = photoCaptureFrame,
                        photoCaptureResult = result
                    });
                }
            }

            if (this._phoneFilePath == null)
            {
                this._photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
            }
        }

        private void OnCapturedPhoneToDisk(PhotoCapture.PhotoCaptureResult result)
        {
            if (result.success)
            {
                this.onPhoneComplete.Invoke(new PhoneCompleteEventData
                {
                    photoCaptureFrame = this._photoCaptureFrame,
                    photoCaptureResult = result
                });
            }
            this._photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);

        }

        private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
        {
            this._phoneFilePath = null;
            this._photoCaptureObject.Dispose();
            this._photoCaptureObject = null;
        }
        #endregion

        #region 录像方法
        /// <summary>
        /// 开始录像
        /// </summary>
        public void startRecording(string name)
        {
            var cameraMode = WebCam.Mode;
            if (cameraMode == WebCamMode.None)
            {
                if (name != null)
                {
                    this._videoFilePath = System.IO.Path.Combine(Application.persistentDataPath, name);
                }

                VideoCapture.CreateAsync(true, OnVideoCaptureCreated);

                Debug.Log("开始录像");
            }
            else
                Debug.LogError("当前相机不可用！");
        }

        void OnVideoCaptureCreated(VideoCapture videoCapture)
        {
            if (videoCapture != null)
            {
                this.onStartRecording.Invoke(new VideoStartRecordingEventData
                {
                    videoCapture = videoCapture
                });

                _videoCaptureObject = videoCapture;

                Resolution cameraResolution = VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
                float cameraFramerate = VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();
                CameraParameters cameraParameters = new CameraParameters();
                cameraParameters.hologramOpacity = 1.0f;
                cameraParameters.frameRate = cameraFramerate;
                cameraParameters.cameraResolutionWidth = cameraResolution.width;
                cameraParameters.cameraResolutionHeight = cameraResolution.height;
                cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

                _videoCaptureObject.StartVideoModeAsync(cameraParameters, VideoCapture.AudioState.MicAudio, OnStartedVideoCaptureMode);
            }
            else
            {
                Debug.LogError("无法创建视频对象！");
            }
        }

        void OnStartedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
        {
            if (result.success)
            {
                _videoCaptureObject.StartRecordingAsync(this._videoFilePath, OnStartedRecordingVideo);
            }
        }

        void OnStartedRecordingVideo(VideoCapture.VideoCaptureResult result)
        {
            //更新UI、允许停止录制、定时录制等操作
            Debug.Log("视频录制开始！");
        }

        /// <summary>
        /// 停止录像
        /// </summary>
        public void stopRecording()
        {
            Debug.Log("停止录像！");
            _videoCaptureObject.StopRecordingAsync(OnStoppedRecordingVideo);
        }

        void OnStoppedRecordingVideo(VideoCapture.VideoCaptureResult result)
        {
            this.onStopRecording.Invoke(new VideoStopRecordingEventData
            {
                videoCaptureResult = result
            });

            Debug.Log("停止视频录制！");
            _videoCaptureObject.StopVideoModeAsync(OnStoppedVideoCaptureMode);
        }

        void OnStoppedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
        {
            _videoCaptureObject.Dispose();
            _videoCaptureObject = null;
        }
        #endregion
    }
}
