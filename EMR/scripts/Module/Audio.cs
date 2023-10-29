using EMR.Entity;
using System.Collections.Generic;
using UnityEngine;
using EMR.Struct;

namespace EMR.Module
{
    /// <summary>
    /// 音频
    /// </summary>
    public class Audio
    {
        #region 基本字段
        /// <summary>
        /// 空间音效模式 2d|3d
        /// </summary>
        private AudioMode _audioMode;

        /// <summary>
        /// 播放音频的节点
        /// </summary>
        private Node _node;

        /// <summary>
        /// 音频组件
        /// </summary>
        private AudioSource _audioSource;

        /// <summary>
        /// 音频剪辑字典
        /// </summary>
        private Dictionary<string, AudioClip> _clipCollection = new Dictionary<string, AudioClip>();
        #endregion

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="node">音频所在节点</param>
        public Audio(Node node)
        {
            this._node = node;

            // 创建音频组件
            if (this._audioSource == null)
            {
                // 添加AudioSource组件
                this._audioSource = this._node.parasitifer.AddComponent<AudioSource>();

                // 设置基本混响
                this._audioSource.outputAudioMixerGroup = Resources.Load<UnityEngine.Audio.AudioMixerGroup>("mixed/Master");

                this._audioSource.playOnAwake = false;

                // 默认为2D音效
                this.audioMode = AudioMode.two;
            }
        }

        #region 基本属性
        /// <summary>
        /// audio的 AudioSource组件
        /// </summary>
        public AudioSource audioSource
        {
            get
            {
                return this._audioSource;
            }
        }

        /// <summary>
        /// 当前音频时长 (秒)
        /// </summary>
        public float length
        {
            get
            {
                return this._audioSource.clip != null ? this._audioSource.clip.length : 0f;
            }
        }

        /// <summary>
        /// 音频剪辑
        /// </summary>
        public AudioClip clip
        {
            get
            {
                return this._audioSource.clip;
            }

            set
            {
                this._audioSource.clip = value;
            }
        }

        /// <summary>
        /// clip集合
        /// </summary>
        public List<AudioClip> audioList
        {
            get
            {
                List<AudioClip> result = new List<AudioClip>();
                foreach(var item in this._clipCollection)
                {
                    result.Add(item.Value);
                }
                return result;
            }
        }

        /// <summary>
        /// 音频模式
        /// </summary>
        public AudioMode audioMode
        {
            get
            {
                return this._audioMode;
            }

            set
            {
                // 设置空间3D音效
                if(this._audioMode != value && value == AudioMode.three)
                {
                    this._audioSource.spatialize = true;
                    this._audioSource.spatialBlend = 1;
                    this._audioMode = value;
                }

                // 设置2D音效
                if(this._audioMode != value && value == AudioMode.two)
                {
                    this._audioSource.spatialize = false;
                    this._audioSource.spatialBlend = 0;
                    this._audioMode = value;
                }
            }
        }

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool isPlaying
        {
            get
            {
                return this._audioSource.isPlaying;
            }
        }

        /// <summary>
        /// 速度
        /// </summary>
        public float speed
        {
            get
            {
                return this._audioSource.pitch;
            }

            set
            {
                this._audioSource.pitch = value;
            }
        }

        /// <summary>
        /// 循环
        /// </summary>
        public bool isLoop
        {
            get
            {
                return this._audioSource.loop;
            }

            set
            {
                this._audioSource.loop = value;
            }
        }

        /// <summary>
        /// 音量 (0-1)
        /// </summary>
        public float volume
        {
            get
            {
                return this._audioSource.volume;
            }

            set
            {
                this._audioSource.volume = value;
            }
        }
        #endregion

        #region 音频相关操作方法
        /// <summary>
        /// 添加音频剪辑
        /// </summary>
        /// <param name="name"></param>
        /// <param name="clip"></param>
        public void addClip(string name, AudioClip clip)
        {
            if (this._clipCollection.ContainsKey(name))
            {
                this._clipCollection.Add(name, clip);
            }
        }

        /// <summary>
        /// 添加音频剪辑
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filePath"></param>
        public void addClip(string name, string filePath)
        {
            var clip = Resources.Load<AudioClip>(filePath);
            this._clipCollection.Add(name, clip);
        }

        /// <summary>
        /// 移除音频剪辑
        /// </summary>
        /// <param name="name"></param>
        public void removeClip(string name)
        {
            if (this._clipCollection.ContainsKey(name))
            {
                if(this._clipCollection[name] == this.clip)
                {
                    this.clip = null;
                }

                this._clipCollection.Remove(name);
            }
        }

        /// <summary>
        /// 播放
        /// </summary>
        public void play()
        {
            var list = this.audioList;
            if (list.Count > 0)
            {
                this._audioSource.clip = list[0];
                this._audioSource.Play();
            }
        }

        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="name"></param>
        public void play(string name)
        {
            if (this._clipCollection.ContainsKey(name))
            {
                this._audioSource.clip = this._clipCollection[name];
                this._audioSource.Play();
            }
        }

        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="clip"></param>
        public void play(AudioClip clip)
        {
            this._audioSource.clip = clip;
            this._audioSource.Play();
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void pause()
        {
            if(this._audioSource.clip != null && this._audioSource.isPlaying)
            {
                this._audioSource.Pause();
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void stop()
        {
            if (this._audioSource.clip != null && this._audioSource.isPlaying)
            {
                this._audioSource.Stop();
            }
        }
        #endregion
    }
}


