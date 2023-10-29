using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using EMR.Struct;
using EMR.Event;

namespace EMR.Module
{
    public class Voice : IMixedRealitySpeechHandler, IMixedRealityDictationHandler
    {
        public Voice()
        {
            // 全局注册语音事件
            CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityDictationHandler>(this);
        }

        #region 基本属性
        /// <summary>
        /// 语音模式
        /// </summary>
        private VoiceMode _mode = VoiceMode.none;

        public VoiceMode mode
        {
            get
            {
                return this._mode;
            }

            set
            {
                if (value == VoiceMode.none)
                {
                    var dictationSystem = CoreServices.GetInputSystemDataProvider<IMixedRealityDictationSystem>();
                    if (dictationSystem.IsListening)
                    {
                        dictationSystem.StopRecording();
                        Debug.Log("关闭听写");
                    }

                    this._mode = VoiceMode.none;
                }

                if (value == VoiceMode.command)
                {
                    if (this._mode != VoiceMode.command)
                    {
                        var dictationSystem = CoreServices.GetInputSystemDataProvider<IMixedRealityDictationSystem>();
                        if (dictationSystem.IsListening)
                        {
                            dictationSystem.StopRecording();
                            Debug.Log("关闭听写");
                        }
                        this._mode = VoiceMode.command;
                    }
                }

                if (value == VoiceMode.dictation)
                {
                    if (this._mode != VoiceMode.dictation)
                    {
                        this._mode = VoiceMode.dictation;
                    }
                }
            }
        }

        /// <summary>
        /// 语间当前是否可用
        /// </summary>
        public bool supportVoice
        {
            get
            {
                var dictationSystem = CoreServices.GetInputSystemDataProvider<IMixedRealityDictationSystem>();


                bool result = false;
                IMixedRealityCapabilityCheck capabilityCheck = CoreServices.InputSystem as IMixedRealityCapabilityCheck;
                if (capabilityCheck != null)
                {
                    if(this.mode == VoiceMode.command)
                    {
                        result = capabilityCheck.CheckCapability(MixedRealityCapability.VoiceCommand);
                    }

                    if (this.mode == VoiceMode.dictation)
                    {
                        result = capabilityCheck.CheckCapability(MixedRealityCapability.VoiceDictation);
                    }
                }

                return result;
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 语音命令触发事件
        /// </summary>
        public VoiceCommandEvent onVoiceCommand = new VoiceCommandEvent();

        /// <summary>
        /// 听写假设事件
        /// </summary>
        public VoiceDictationHypothesisEvent onDictationHypothesis = new VoiceDictationHypothesisEvent();

        /// <summary>
        /// 听写结果事件
        /// </summary>
        public VoiceDictationResultEvent onDictationResult = new VoiceDictationResultEvent();

        /// <summary>
        /// 听写完成事件
        /// </summary>
        public VoiceDictationCompleteEvent onDictationComplete = new VoiceDictationCompleteEvent();

        /// <summary>
        /// 听写异常事件
        /// </summary>
        public VoiceDictationErrorEvent onDictationError = new VoiceDictationErrorEvent();
        #endregion

        /*
         *  以下方法用于触发相关语音事件
         */
        void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData)
        {
            if (this._mode == VoiceMode.command)
            {
                // 触发onVoiceCommand事件
                this.onVoiceCommand.Invoke(new VoiceCommandEventData
                {
                    command = eventData.Command.Keyword,
                    original = eventData,
                });
            }
        }

        public void OnDictationHypothesis(DictationEventData eventData)
        {
            if (this._mode == VoiceMode.dictation)
            {
                this.onDictationHypothesis.Invoke(new VoiceDictationHypothesisEventData
                {
                    result = eventData.DictationResult,
                    original = eventData
                });
            }
        }

        public void OnDictationResult(DictationEventData eventData)
        {
            if (this._mode == VoiceMode.dictation)
            {
                this.onDictationResult.Invoke(new VoiceDictationResultEventData
                {
                    result = eventData.DictationResult,
                    original = eventData
                });
            }
        }

        public void OnDictationComplete(DictationEventData eventData)
        {
            if (this._mode == VoiceMode.dictation)
            {
                this.onDictationComplete.Invoke(new VoiceDictationCompleteEventData
                {
                    result = eventData.DictationResult,
                    original = eventData
                });
            }
        }

        public void OnDictationError(DictationEventData eventData)
        {
            if (this._mode == VoiceMode.dictation)
            {
                this.onDictationError.Invoke(new VoiceDictationErrorEventData
                {
                    result = eventData.DictationResult,
                    original = eventData
                });
            }
        }

        #region 基本方法
        /// <summary>
        /// 开始听写
        /// </summary>
        /// <param name="recordingTime">听写时长 单位秒</param>
        public void startDictation(int recordingTime)
        {
            if (this._mode == VoiceMode.dictation)
            {
                var dictationSystem = CoreServices.GetInputSystemDataProvider<IMixedRealityDictationSystem>();
                if (dictationSystem != null && !dictationSystem.IsListening)
                {
                    dictationSystem.StartRecording(null, 5, 20, recordingTime);
                }
            }
        }

        /// <summary>
        /// 停止听写
        /// </summary>
        public void stopDictation()
        {
            if (this._mode == VoiceMode.dictation)
            {
                var dictationSystem = CoreServices.GetInputSystemDataProvider<IMixedRealityDictationSystem>();
                if (dictationSystem != null && dictationSystem.IsListening)
                {
                    dictationSystem.StopRecording();
                    Debug.Log("关闭听写");
                }
            }
        }
        #endregion
    }
}