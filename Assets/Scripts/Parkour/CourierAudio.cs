using UnityEngine;

namespace CityCourier
{
    public sealed class CourierAudio : MonoBehaviour
    {
        public CourierStage Stage;
        public CourierMotor Motor;

        [Header("Cyberpunk BGM")]
        public AudioClip BgmClip;
        [Range(0f, 1f)] public float BgmBaseVolume = 0.42f;
        public bool PlayBgmOnAwake = true;

        [Header("Parkour Action SFX (Replaceable in Inspector)")]
        public AudioClip FootstepClip;
        public AudioClip JumpClip;
        public AudioClip WallJumpClip;
        public AudioClip SlideClip;
        public AudioClip DiveClip;
        public AudioClip LandClip;
        public AudioClip WallRunLoopClip;

        [Header("Stage Event SFX")]
        public AudioClip LetterPickupClip;
        public AudioClip CheckpointClip;
        public AudioClip DamageClip;
        public AudioClip StageClearClip;

        [Range(0f, 1f)] public float SfxVolume = 0.75f;

        public bool IsBgmPlaying => bgmSource != null && bgmSource.isPlaying;
        public bool IsWallRunLoopPlaying => wallRunSource != null && wallRunSource.isPlaying;
        public int SfxPlayCount { get; private set; }
        public string LastPlayedSfx { get; private set; } = string.Empty;

        AudioSource bgmSource;
        AudioSource sfxSource;
        AudioSource wallRunSource;
        CourierStage boundStage;
        CourierMotor boundMotor;

        void Awake()
        {
            SetupSources();
            EnsureFallbackClips();
            BindEvents();
            if (PlayBgmOnAwake) StartBgm();
        }

        void OnEnable()
        {
            SetupSources();
            EnsureFallbackClips();
            BindEvents();
            if (PlayBgmOnAwake && bgmSource != null && !bgmSource.isPlaying)
                StartBgm();
        }

        void OnDisable()
        {
            UnbindEvents();
            if (wallRunSource != null && wallRunSource.isPlaying)
                wallRunSource.Stop();
        }

        void SetupSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
                bgmSource.spatialBlend = 0f;
                bgmSource.volume = BgmBaseVolume;
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f;
                sfxSource.volume = SfxVolume;
            }
            if (wallRunSource == null)
            {
                wallRunSource = gameObject.AddComponent<AudioSource>();
                wallRunSource.loop = true;
                wallRunSource.playOnAwake = false;
                wallRunSource.spatialBlend = 0f;
                wallRunSource.volume = SfxVolume * 0.55f;
            }
        }

        public void EnsureFallbackClips()
        {
            if (BgmClip == null) BgmClip = LoadOrSynth("Cyberpunk_BGM", 2.0f, 110f, 220f, true);
            if (FootstepClip == null) FootstepClip = LoadOrSynth("SFX_Footstep", 0.09f, 95f, 55f, false);
            if (JumpClip == null) JumpClip = LoadOrSynth("SFX_Jump", 0.2f, 200f, 580f, false);
            if (WallJumpClip == null) WallJumpClip = LoadOrSynth("SFX_WallJump", 0.24f, 260f, 740f, false);
            if (SlideClip == null) SlideClip = LoadOrSynth("SFX_Slide", 0.35f, 150f, 70f, false);
            if (DiveClip == null) DiveClip = LoadOrSynth("SFX_Dive", 0.3f, 480f, 110f, false);
            if (LandClip == null) LandClip = LoadOrSynth("SFX_Land", 0.18f, 120f, 45f, false);
            if (WallRunLoopClip == null) WallRunLoopClip = LoadOrSynth("SFX_WallRunLoop", 0.5f, 130f, 145f, true);
            if (LetterPickupClip == null) LetterPickupClip = LoadOrSynth("SFX_LetterPickup", 0.28f, 880f, 1760f, false);
            if (CheckpointClip == null) CheckpointClip = LoadOrSynth("SFX_Checkpoint", 0.25f, 587f, 880f, false);
            if (DamageClip == null) DamageClip = LoadOrSynth("SFX_Damage", 0.32f, 180f, 75f, false);
            if (StageClearClip == null) StageClearClip = LoadOrSynth("SFX_StageClear", 0.7f, 587f, 1174f, false);
        }

        static AudioClip LoadOrSynth(string resourceName, float duration, float startFreq, float endFreq, bool loopable)
        {
            AudioClip loaded = Resources.Load<AudioClip>("Audio/CityCourier/" + resourceName);
            if (loaded != null) return loaded;
            return CreateSynthClip(resourceName, duration, startFreq, endFreq, loopable);
        }

        static AudioClip CreateSynthClip(string name, float duration, float startFreq, float endFreq, bool loopable)
        {
            int sampleRate = 22050;
            int count = Mathf.Max(64, Mathf.RoundToInt(duration * sampleRate));
            float[] data = new float[count];
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += freq / sampleRate;
                float env = loopable ? 0.6f : Mathf.Exp(-4.5f * t);
                data[i] = Mathf.Sin(phase * Mathf.PI * 2f) * env * 0.4f;
            }
            AudioClip clip = AudioClip.Create(name, count, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        void BindEvents()
        {
            if (Stage == null) Stage = GetComponent<CourierStage>();
            if (Motor == null && Stage != null) Motor = Stage.Player;

            if (boundStage != Stage)
            {
                if (boundStage != null)
                {
                    boundStage.StageStarted -= OnStageStarted;
                    boundStage.LettersChanged -= OnLettersChanged;
                    boundStage.CheckpointReached -= OnCheckpointReached;
                    boundStage.Damaged -= OnStageDamaged;
                    boundStage.RoadOpened -= OnRoadOpened;
                    boundStage.StageCompleted -= OnStageCompleted;
                }
                boundStage = Stage;
                if (boundStage != null)
                {
                    boundStage.StageStarted += OnStageStarted;
                    boundStage.LettersChanged += OnLettersChanged;
                    boundStage.CheckpointReached += OnCheckpointReached;
                    boundStage.Damaged += OnStageDamaged;
                    boundStage.RoadOpened += OnRoadOpened;
                    boundStage.StageCompleted += OnStageCompleted;
                }
            }

            if (boundMotor != Motor)
            {
                if (boundMotor != null)
                {
                    boundMotor.Jumped -= OnMotorJumped;
                    boundMotor.Landed -= OnMotorLanded;
                    boundMotor.SlideTriggered -= OnMotorSlideTriggered;
                    boundMotor.WallRunBegan -= OnMotorWallRunBegan;
                    boundMotor.Footstepped -= OnMotorFootstepped;
                }
                boundMotor = Motor;
                if (boundMotor != null)
                {
                    boundMotor.Jumped += OnMotorJumped;
                    boundMotor.Landed += OnMotorLanded;
                    boundMotor.SlideTriggered += OnMotorSlideTriggered;
                    boundMotor.WallRunBegan += OnMotorWallRunBegan;
                    boundMotor.Footstepped += OnMotorFootstepped;
                }
            }
        }

        void UnbindEvents()
        {
            if (boundStage != null)
            {
                boundStage.StageStarted -= OnStageStarted;
                boundStage.LettersChanged -= OnLettersChanged;
                boundStage.CheckpointReached -= OnCheckpointReached;
                boundStage.Damaged -= OnStageDamaged;
                boundStage.RoadOpened -= OnRoadOpened;
                boundStage.StageCompleted -= OnStageCompleted;
                boundStage = null;
            }
            if (boundMotor != null)
            {
                boundMotor.Jumped -= OnMotorJumped;
                boundMotor.Landed -= OnMotorLanded;
                boundMotor.SlideTriggered -= OnMotorSlideTriggered;
                boundMotor.WallRunBegan -= OnMotorWallRunBegan;
                boundMotor.Footstepped -= OnMotorFootstepped;
                boundMotor = null;
            }
        }

        public void StartBgm()
        {
            if (bgmSource == null || BgmClip == null) return;
            bgmSource.clip = BgmClip;
            bgmSource.volume = BgmBaseVolume;
            if (!bgmSource.isPlaying) bgmSource.Play();
        }

        void Update()
        {
            BindEvents();
            StepAudio(Time.deltaTime);
        }

        public void StepAudio(float dt)
        {
            if (Motor == null) return;

            // Modulate BGM intensity and pitch subtly with player horizontal speed
            if (bgmSource != null && bgmSource.isPlaying)
            {
                float speedRatio = Mathf.InverseLerp(0f, 17f, Motor.HorizontalSpeed);
                bgmSource.volume = Mathf.Lerp(BgmBaseVolume * 0.85f, Mathf.Min(1f, BgmBaseVolume * 1.25f), speedRatio);
                bgmSource.pitch = Mathf.Lerp(0.98f, 1.04f, speedRatio);
            }

            // Wall-run continuous sound loop
            if (wallRunSource != null && WallRunLoopClip != null)
            {
                if (Motor.IsWallRunning)
                {
                    if (!wallRunSource.isPlaying)
                    {
                        wallRunSource.clip = WallRunLoopClip;
                        wallRunSource.Play();
                    }
                }
                else if (wallRunSource.isPlaying)
                {
                    wallRunSource.Stop();
                }
            }
        }

        void PlaySfx(AudioClip clip, string tag, float volumeScale = 1f, float pitch = 1f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(SfxVolume * volumeScale));
            SfxPlayCount++;
            LastPlayedSfx = tag;
        }

        void OnStageStarted() => StartBgm();
        void OnLettersChanged(int current, int required) => PlaySfx(LetterPickupClip, "LetterPickup", 0.9f, 1f + current * 0.04f);
        void OnCheckpointReached(Vector3 pt) => PlaySfx(CheckpointClip, "Checkpoint", 0.75f);
        void OnRoadOpened() => PlaySfx(CheckpointClip, "RoadOpened", 0.95f, 1.12f);
        void OnStageDamaged(string reason) => PlaySfx(DamageClip, "Damage", 1f);
        void OnStageCompleted() => PlaySfx(StageClearClip, "StageClear", 1f);

        void OnMotorJumped(bool isWallJump)
        {
            if (isWallJump) PlaySfx(WallJumpClip, "WallJump", 0.95f);
            else PlaySfx(JumpClip, "Jump", 0.85f);
        }

        void OnMotorLanded(float impactSpeed, bool wasDiving)
        {
            float vol = Mathf.Clamp(impactSpeed / 16f, 0.35f, 1f);
            PlaySfx(LandClip, wasDiving ? "DiveLand" : "Land", vol);
        }

        void OnMotorSlideTriggered(bool isAirDive)
        {
            if (isAirDive) PlaySfx(DiveClip, "Dive", 0.9f);
            else PlaySfx(SlideClip, "Slide", 0.85f);
        }

        void OnMotorWallRunBegan(Vector3 normal)
        {
            PlaySfx(FootstepClip, "WallRunStart", 0.75f, 1.25f);
            StepAudio(0.016f);
        }

        void OnMotorFootstepped()
        {
            float pitchJitter = Random.Range(0.94f, 1.06f);
            PlaySfx(FootstepClip, "Footstep", 0.5f, pitchJitter);
        }
    }
}
