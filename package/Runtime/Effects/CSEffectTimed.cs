using System.Collections;
using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// Manages the full lifetime of a composite visual effect.
    /// - Coordinates multiple visual components (sprite, quad, particles, child effects, floating effects).
    /// - Particle systems may be started all at once or incrementally in groups.
    /// - Effects remain active while _countDown > 0.
    /// - Countdown driven externally via TurnEnd().
    ///
    /// Lifecycle model:
    /// - Effects are started via PlayEffect().
    /// - If _destroyOnDuration > 0, the effect will automatically fade out and
    ///   clean itself up once the countdown reaches zero.
    /// - If _destroyOnDuration <= 0, the effect is persistent until
    ///   StopEffect() is explicitly called.
    ///
    /// This class owns effect lifetime and shutdown rules, but does not assume
    /// how or when external systems decide to end the effect.
    /// 
    /// Floating sub-effects are managed by a secondary coroutine but remain
    /// lifecycle-bound to this instance.
    ///
    /// Sample under Samples/EffectTimed/
    /// </summary>
    public class CSEffectTimed : MonoBehaviour
    {
        [Header("Flat effect - use a sprite or a quad, not both")]
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField, Tooltip("Must use transparent shader")] private MeshRenderer _quad;
        [SerializeField] private AnimationCurve _flatFade = AnimationCurve.Linear(0, 1, 1, 0);
        [Header("Particles")]
        [SerializeField] private ParticleSystem[] _particles; // an array of particle systems
        [SerializeField] private int _particleGroup = 1; // the group size of particles - how many particles should be played at once
        [Header("Additional CSEffects")]
        [SerializeField] private CSEffectTimed[] _effects;
        [SerializeField] private CSSimpleCounter _countDownObject; // if non-null, this object will be set active as long as the countdown is > 0
        [Header("Floating effects")]
        [SerializeField] private CSEffectTimed _floatingEffectPrefab;
        [SerializeField] private int _floatingCountMin = 3;
        [SerializeField] private int _floatingCountMax = 5;
        [SerializeField] private float _floatingDistance = 1f;
        [SerializeField] private float _floatingRubber = 1f; // how fast to accelerate towards 0
        [SerializeField] private float _floatingSpeed = 1f; // how fast it starts moving
        [SerializeField] private bool _floating2D;
        [Header("Particle positioning")]
        [SerializeField] private float _particleRandomSpread = 0f; // the maximum amount of random repositioning for each particle system
        [SerializeField] private float _particleKickY = 0f;
        [Header("Effect ending")]
        [SerializeField] private float _particleIncrementDelay = 0f; // the time delay between each particle system being triggered
        [SerializeField] private float _destroyOnDuration;
        [SerializeField] private bool _disableNotDestroy;
        [SerializeField] private int _countDownDelay = 0; // if > 0, this waits for X "turns" before the delay starts (make sure to tell this what a turn is!)
        [SerializeField, Tooltip("Played on ending - safe to self-reference")] private CSEffectTimed _effectEnding;
        private CSEffectTimed[] _floatingEffects = new CSEffectTimed[0];
        private Vector3[] _floatingEffectSpeeds = new Vector3[0];
        protected int _countDown;
        private Coroutine _effectLifeTime;
        private Coroutine _effectFloaterManager;
        private bool _effectActive;
        private bool _floaterActive;
        // cache values on first initialisation
        private Color _colorCacheFlat;
        private ParticleSystem.MinMaxGradient[] _colorCacheParticles;
        private Color _colorOverride;
        private bool _colorInitNeeded = true;
        private bool _colorOverrideActive;
        public bool IsPlaying => _effectActive;

        #region starting effects
        // this stays until called again
        private void ColorInit()
        {
            if (_colorInitNeeded)
            {
                if (_quad)
                {
                    if (_sprite)
                    {
                        Debug.LogWarning("CSEffectTimed has a quad and a sprite - sprite disabled");
                        _sprite.enabled = false;
                    }
                    _colorCacheFlat = _quad.material.color;
                }
                else
                {
                    if (_sprite)
                        _colorCacheFlat = _sprite.color;
                    else
                        _colorCacheFlat = Color.white;
                }

                _colorCacheParticles = new ParticleSystem.MinMaxGradient[_particles.Length];
                for (int i = 0; i < _colorCacheParticles.Length; i++)
                {
                    ParticleSystem.MainModule particleMain = _particles[i].main;
                    particleMain.startColor = CreateColorGradient(particleMain.startColor.colorMax);
                    _colorCacheParticles[i] = particleMain.startColor;
                }
                _colorInitNeeded = false;
            }
        }
        // standardise color gradient creation
        private ParticleSystem.MinMaxGradient CreateColorGradient(Color colorBase)
        {
            Color colorDark = Color.Lerp(colorBase, Color.clear, 0.5f);
            return new ParticleSystem.MinMaxGradient(colorBase, colorDark);
        }
        // does not check _quad first
        private void SetQuadColor(Color colorSet)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            _quad.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", colorSet);
            _quad.SetPropertyBlock(mpb);
        }
        public void SetColorOverride(Color colorSet)
        {
            ColorInit();

            if (colorSet.a > 0)
            {
                // only needs to be set here - the gradient handles the fade effect
                for (int i = 0; i < _particles.Length; i++)
                {
                    ParticleSystem.MainModule particleMain = _particles[i].main;
                    particleMain.startColor = CreateColorGradient(colorSet);
                }

                // needs to be cached for the sprite fade effect
                _colorOverride = colorSet;
                if (_quad)
                    SetQuadColor(colorSet);
                else if (_sprite)
                    _sprite.color = colorSet;

                _colorOverrideActive = true;
            }
            else
                ClearColorOverride();
        }
        public void ClearColorOverride()
        {
            if (_colorOverrideActive)
            {
                _colorOverrideActive = false;

                // reset colors
                for (int i = 0; i < _particles.Length; i++)
                {
                    ParticleSystem.MainModule particleMain = _particles[i].main;
                    particleMain.startColor = _colorCacheParticles[i];
                }

                if (_quad)
                    SetQuadColor(_colorCacheFlat);
                else if (_sprite)
                    _sprite.color = _colorCacheFlat;
            }
        }

        /// <summary>
        /// Starts the effect lifecycle.
        ///
        /// If already playing, this call is ignored.
        /// If timeOverride >= 0, overrides the configured countdown value.
        /// Child effects are started recursively using the same timing rules.
        /// </summary>
        public void PlayEffect(int timeOverride = -1)
        {
            if (_effectActive) return;

            _effectActive = true;

            ColorInit();

            gameObject.SetActive(true);
            _effectLifeTime = StartCoroutine(EffectLifetime(timeOverride));
        
            PlayEffectCustom();
        }
        // this is for orienting the effect relative to an origin
        public void PlayEffectDirected(Vector3 direction)
        {
            if (_particleKickY > 0)
                direction.y += _particleKickY; // shift the direction up slightly to make the particles "bounce up" off the floor

            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg);

            PlayEffect();
        }
        public void PlayEffectOriented(Vector3 origin)
        {
            Vector3 direction = (transform.position - origin).normalized;

            PlayEffectDirected(direction);
        }
        #endregion

        #region Stopping the effects
        /// <summary>
        /// Immediately stops the effect and performs cleanup.
        ///
        /// This is the only way to terminate "eternal" effects
        /// (_destroyOnDuration <= 0).
        /// </summary>
        public void StopEffect()
        {
            _effectActive = false;
            if (_effectLifeTime != null)
            {
                StopCoroutine(_effectLifeTime);

                FinishEffects();
            }
        }
        private void FinishEffects()
        {
            StopEffectCustom();

            transform.rotation = Quaternion.identity;

            if (_countDownObject)
                _countDownObject.ClearCounter();
            if (_quad)
            {
                _quad.enabled = false;
                if (_colorOverrideActive)
                    SetQuadColor(_colorOverride);
                else
                    SetQuadColor(_colorCacheFlat);
            }
            else if (_sprite)
            {
                _sprite.enabled = false;
                if (_colorOverrideActive)
                    _sprite.color = _colorOverride;
                else
                    _sprite.color = _colorCacheFlat;
            }

            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i])
                {
                    _particles[i].transform.localPosition = Vector3.zero;
                    _particles[i].Stop(true, ParticleSystemStopBehavior.StopEmitting); // allow remaining particles to finish animating
                }
                else
                    Debug.LogWarning($"CSEffectTimed _particles index {i} is null", this);
            }
            for (int i = 0; i < _effects.Length; i++)
            {
                if (_effects[i])
                    _effects[i].StopEffect();
                else
                    Debug.LogWarning($"CSEffectTimed _effects index {i} is null", this);
            }

            // clear floatingeffect
            FloaterCleanUp();

            _countDown = 0;
            _effectLifeTime = null;
            _effectActive = false;

            if (_disableNotDestroy)
            {
                if (_effectEnding)
                {
                    if (_colorOverrideActive)
                        _effectEnding.SetColorOverride(_colorOverride);
                    _effectEnding.PlayEffect();
                }
            }
            else
                Destroy(gameObject);
        }
        #endregion

        /// <summary>
        /// Advances the turn-based countdown.
        ///
        /// Returns true when the countdown reaches zero, signalling that the effect
        /// is eligible to end (if not eternal).
        /// </summary>
        public bool TurnEnd()
        {
            if (_effectLifeTime == null)
            {
                Debug.LogWarning($"CSEffectTimed {name} TurnEnd called while not playing", this);
                _effectActive = false;
                return true; // not playing so treat as if finished (allow deletion, etc)
            }
            if (!_effectActive)
            {
                Debug.LogWarning($"CSEffectTimed {name} TurnEnd called while inactive", this);
                return true; // not playing so treat as if finished (allow deletion, etc)
            }
            _countDown--;
            if (_countDownObject)
                _countDownObject.SetCounter(_countDown);

            return (_countDown <= 0);
        }

        #region Effect Lifetime
        // Orchestrates the full effect lifecycle.
        // Executes startup, particle triggering, countdown waiting,
        // and optional shutdown phases sequentially.
        private IEnumerator EffectLifetime(int timeOverride)
        {
            yield return EffectStartup(timeOverride);
            yield return EffectStartupParticles();
            yield return EffectCountdown();
            // all turn-timed effects have now completed, we can fade out destroy the effects
            yield return new WaitForEndOfFrame(); // always wait at least one frame for self-reference safety
            if (_destroyOnDuration > 0)
                yield return EffectEnd();
            // must not yield now - need to allow coroutine to end as _effectLifeTime = null
            // the effects will play indefinitely if _destroyOnDuration <= 0
            // they will only end if StopEffect is called explicitly
        }
        // Enables initial visual elements and starts floating sub-effects.
        private IEnumerator EffectStartup(int timeOverride)
        {
            // startup
            if (timeOverride >= 0)
                _countDown = timeOverride;
            else
                _countDown = _countDownDelay;

            for (int i = 0; i < _effects.Length; i++)
                _effects[i].PlayEffect(timeOverride);

            // show countdown object
            if (_countDown > 0 && _countDownObject)
                _countDownObject.SetCounter(_countDown);

            if (_quad)
                _quad.enabled = true;
            else if (_sprite)
                _sprite.enabled = true;

            // clear any remaining particles
            for (int i =0; i < _particles.Length; i++)
                _particles[i].Clear();

            // set up floaters
            if (_floatingEffectPrefab && _floatingCountMin > 0 && _floatingCountMax > 0)
            {
                _floaterActive = true;
                _effectFloaterManager = StartCoroutine(FloaterTimer());
            }
            else
                _floaterActive = false;

            yield break;
        }
        // Triggers particle systems, optionally incrementally over time.
        // This phase must complete before countdown monitoring begins.
        private IEnumerator EffectStartupParticles()
        {
            if (_particles.Length == 0)
                yield break;

            int particlesStarted = 0; // tracks which _particles have already started
            int particleGroupSize = 1;
            bool particlesFinished = false;

            // set up particles
            if (_particleIncrementDelay <= 0) // play all particles now
            {
                PlayParticleGroup(0, _particles.Length);
                yield break;
            }
            else if (_particleGroup > 1)
                particleGroupSize = _particleGroup;

            // increment over particle groups until all particles have been started
            while (!particlesFinished)
            {
                PlayParticleGroup(particlesStarted, particlesStarted + particleGroupSize);
                particlesStarted += particleGroupSize;
                // if all have started, we can start the timer
                if (particlesStarted < _particles.Length)
                    yield return new WaitForSeconds(_particleIncrementDelay);
                else
                    particlesFinished = true;
            }
        }
        // plays the particle systems
        private void PlayParticleGroup(int startInc, int endExc)
        {
            Vector3 particleGroupPos = new Vector3(Random.Range(-_particleRandomSpread, _particleRandomSpread), Random.Range(-_particleRandomSpread, _particleRandomSpread));

            for (int i = startInc; i < endExc; i++)
            {
                if (i < _particles.Length)
                {
                    if (_particles[i])
                    {
                        if (_particleRandomSpread > 0)
                            _particles[i].transform.localPosition = particleGroupPos;

                        _particles[i].Clear(); // in case any were left from a previous play
                        _particles[i].Play();
                    }
                }
            }
        }
        // Waits until the externally-driven countdown reaches zero.
        private IEnumerator EffectCountdown()
        {
            // all effects have now started - start monitoring the countdown
            while (_countDown > 0)
                yield return null;
        }
        // Handles fade-out and cleanup for finite-duration effects only.
        // Must be the final phase; no yielding is allowed after FinishEffects().
        private IEnumerator EffectEnd()
        {
            float timeSpent = 0;
            Color spriteColor;

            if (_colorOverrideActive)
                spriteColor = _colorOverride;
            else
                spriteColor = _colorCacheFlat; // start with the original sprite color

            // the countdown has ended, end effects
            if (_countDownObject)
                _countDownObject.ClearCounter();

            // fade out the sprite if present
            while (timeSpent < _destroyOnDuration)
            {
                if (_quad || _sprite)
                {
                    // fade the quad/sprite gradually over the destroyOnDuration
                    spriteColor.a = _flatFade.Evaluate(timeSpent / _destroyOnDuration);

                    if (_quad)
                        SetQuadColor(spriteColor);
                    else
                        _sprite.color = spriteColor;
                }
                timeSpent += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            FinishEffects();
        }
        #endregion

        #region Floater Management
        // Floaters require per-frame positional integration, so they are managed
        // by a dedicated coroutine while lifetime remains centrally controlled.
        private IEnumerator FloaterTimer()
        {
            // instantiate floaters
            int count = Random.Range(_floatingCountMin, _floatingCountMax + 1);
            _floatingEffects = new CSEffectTimed[count];
            _floatingEffectSpeeds = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                Vector3 offset;
                Vector3 speed;
                if (_floating2D)
                {
                    offset = Random.insideUnitCircle;
                    speed = (Vector3)CSMathRand.RandomVector2D() - offset;
                    
                }
                else
                {
                    offset = Random.insideUnitSphere;
                    speed = Random.onUnitSphere - offset;
                }
                _floatingEffects[i] = Instantiate(_floatingEffectPrefab, transform);
                _floatingEffects[i].transform.localPosition = offset * _floatingDistance;
                _floatingEffectSpeeds[i] = speed * _floatingSpeed;
                _floatingEffects[i].PlayEffect();
            }

            // run floater movement until the EffectTimer tells it to stop
            while (_floaterActive)
            {
                for (int i = 0; i < _floatingEffects.Length; i++)
                {
                    if (_floatingEffects[i])
                    {
                        Vector3 pos = _floatingEffects[i].transform.localPosition;
                        _floatingEffectSpeeds[i] -= pos.normalized * _floatingRubber * Time.deltaTime;
                        pos += _floatingEffectSpeeds[i] * Time.deltaTime;
                        _floatingEffects[i].transform.localPosition = pos;
                    }
                    else
                        Debug.LogWarning("CSEffectTimed _floatingEffects " + i + " is null", this);
                }
                yield return new WaitForEndOfFrame();
            }
        }
        private void FloaterCleanUp()
        {
            if (_floaterActive)
            {
                StopCoroutine(_effectFloaterManager);
                for (int i = 0; i < _floatingEffects.Length; i++)
                {
                    if (_floatingEffects[i])
                    {
                        _floatingEffects[i].StopEffect();
                        Destroy(_floatingEffects[i].gameObject);
                    }
                    else
                        Debug.LogWarning("CSEffectTimed _floatingEffects index " + i + " is null", this);
                }
                _floatingEffects = new CSEffectTimed[0];
                _floatingEffectSpeeds = new Vector3[0];
                _effectFloaterManager = null;
                _floaterActive = false;
            }
        }
        #endregion

        // hook for extension classes to play additional effects
        public virtual void PlayEffectCustom()
        {
        }
        protected virtual void StopEffectCustom()
        {
        }

    }
}