using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// displays an effect in the scene for a limited duration
// could include sprites, particles, etc
// used for blood splashes, explosions, bullet sparks
// oldest date 2/11/23
// modified 12/2/24

namespace CoSeph.Core
{
    public class CSEffectTimed : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private AnimationCurve _spriteFade = AnimationCurve.Linear(0, 1, 1, 0);
        [SerializeField] private ParticleSystem[] _particles; // an array of particle systems
        [SerializeField] private int _particleGroup = 1; // the group size of particles - how many particles should be played at once
        [SerializeField] private CSEffectTimed[] _effects;
        [Header("Floating effects")]
        [SerializeField] private CSEffectTimed _floatingEffectPrefab;
        [SerializeField] private int _floatingCountMin = 3;
        [SerializeField] private int _floatingCountMax = 5;
        [SerializeField] private float _floatingDistance = 1f;
        [SerializeField] private float _floatingRubber = 1f; // how fast to accelerate towards 0
        [SerializeField] private float _floatingSpeed = 1f; // how fast it starts moving
        [Header("Particle positioning")]
        [SerializeField] private float _particleRandomSpread = 0f; // the maximum amount of random repositioning for each particle system
        [SerializeField] private float _particleKickY = 0f;
        [Header("Particle timing")]
        [SerializeField] private float _particleIncrementDelay = 0f; // the time delay between each particle system being triggered
        [SerializeField] private float _destroyOnDuration;
        [SerializeField] private bool _disableNotDestroy;
        [SerializeField] private int _countDownDelay = 0; // if > 0, this waits for X "turns" before the delay starts (make sure to tell this what a turn is!)
        [SerializeField] private GameObject _countDownObject; // if non-null, this object will be set active as long as the countdown is > 0
        private Color _colorOverride;
        private int _particleGroupIncrement;
        private Vector3 _particleGroupPos;
        private CSEffectTimed[] _floatingEffects = new CSEffectTimed[0];
        private Vector2[] _floatingEffectSpeeds = new Vector2[0];
        protected int _countDown;

        // start the effect sequence
        public void PlayEffect(int timeOverride = -1)
        {
            gameObject.SetActive(true);
            if (timeOverride >= 0)
                _countDown = timeOverride;
            else
                _countDown = _countDownDelay;
            //Debug.Log("Playing effect " + name + " at " + transform.position);
            for (int i = 0; i < _effects.Length; i++)
                _effects[i].PlayEffect(timeOverride);
            PlayEffectCustom();
            StartCoroutine(EffectTimer());
            if (_sprite)
                StartCoroutine(EffectSprite());

            if (_floatingEffectPrefab)
            {
                int count = Random.Range(_floatingCountMin, _floatingCountMax + 1);
                _floatingEffects = new CSEffectTimed[count];
                _floatingEffectSpeeds = new Vector2[count];
                for (int i = 0; i < count; i++)
                {
                    Vector2 offset = CSMath.RandomVector2D();
                    Vector2 speed = CSMath.RandomVector2D() - offset;
                    _floatingEffects[i] = Instantiate(_floatingEffectPrefab, transform);
                    _floatingEffects[i].transform.localPosition = offset * _floatingDistance;
                    _floatingEffectSpeeds[i] = speed * _floatingSpeed;
                    _floatingEffects[i].PlayEffect();
                }
                StartCoroutine(FloaterTimer());
            }
        }
        public void PlayEffect(Color colorSet)
        {
            _colorOverride = colorSet;
            PlayEffect();
        }

        public void StopEffect()
        {
            StopAllCoroutines();
            StopEffectCustom();
            if (_sprite)
                _sprite.enabled = false;
            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i])
                    _particles[i].Stop();
                else
                    Debug.LogWarning("CSEffectTimed _particles index " + i + " is null");
            }
            for (int i = 0; i < _effects.Length; i++)
            {
                if (_effects[i])
                    _effects[i].StopEffect();
                else
                    Debug.LogWarning("CSEffectTimed _effects index " + i + " is null");
            }
            for (int i = 0; i < _floatingEffects.Length; i++)
            {
                if (_floatingEffects[i])
                {
                    _floatingEffects[i].StopEffect();
                    Destroy(_floatingEffects[i].gameObject);
                }
                else
                    Debug.LogWarning("CSEffectTimed _floatingEffects index " + i + " is null");
            }
            gameObject.SetActive(false);
        }
        protected virtual void StopEffectCustom()
        {

        }

        public bool TurnEnd()
        {
            _countDown--;
            return (_countDown <= 0);
        }
        public virtual void PlayEffectCustom() { }

        // this is for orienting the effect relative to an origin
        public void PlayEffectDirected(Vector3 direction)
        {
            if (_particleKickY > 0)
                direction.y += _particleKickY; // shift the direction up slightly to make the particles bounce up off the floor

            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg);

            PlayEffect();
        }
        public void PlayEffectDirected(Vector3 direction, Color colorSet)
        {
            _colorOverride = colorSet;
            PlayEffectDirected(direction);
        }
        public void PlayEffectOriented(Vector3 origin)
        {
            Vector3 direction = (transform.position - origin).normalized;

            PlayEffectDirected(direction);
        }
        public void PlayEffectOriented(Vector3 origin, Color colorSet)
        {
            _colorOverride = colorSet;
            PlayEffectOriented(origin);
        }

        // this coroutine always runs so handles anything critical e.g. Destroy
        // plays particle sysems
        private IEnumerator EffectTimer()
        {
            _particleGroupIncrement = 0;

            if (_countDown > 0 && _countDownObject) _countDownObject.SetActive(true);

            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i])
                {
                    if (_particleRandomSpread > 0)
                    {
                        if (_particleGroupIncrement == 0)
                            _particleGroupPos = new Vector3(Random.Range(-_particleRandomSpread, _particleRandomSpread), Random.Range(-_particleRandomSpread, _particleRandomSpread));

                        _particles[i].transform.localPosition = _particleGroupPos;
                    }

                    if (_colorOverride.a > 0)
                    {
                        Color colorOverrideDark = Color.Lerp(_colorOverride, Color.black, 0.5f);
                        ParticleSystem.MainModule particleMain = _particles[i].main;
                        //particleMain.startColor = colorOverride;
                        particleMain.startColor = new ParticleSystem.MinMaxGradient(_colorOverride, colorOverrideDark);
                    }
                    _particles[i].Play();
                    //Debug.Log("particle effect " + i + " playing");

                    if (_particleGroup > 1)
                    {
                        // with particle groups, only pause between groups
                        _particleGroupIncrement++;
                        if (_particleGroupIncrement == _particleGroup && _particleIncrementDelay > 0)
                        {
                            _particleGroupIncrement = 0;
                            yield return new WaitForSeconds(_particleIncrementDelay);
                        }
                    }
                    else if (_particleIncrementDelay > 0)
                        yield return new WaitForSeconds(_particleIncrementDelay);
                }
                else
                    Debug.LogWarning("CSEffectTimed _particles " + i + " is null");
            }
            while (_countDown > 0)
                yield return new WaitForEndOfFrame();

            if (_countDownObject) _countDownObject.SetActive(false);

            if (_destroyOnDuration > 0)
            {
                yield return new WaitForSeconds(_destroyOnDuration);
                if (_disableNotDestroy)
                    StopEffect();
                else
                    Destroy(gameObject);
            }
        }

        private IEnumerator FloaterTimer()
        {
            float duration = 0f;
            bool eternal;

            if (_destroyOnDuration > 0)
                duration = _destroyOnDuration;
            if (_particleIncrementDelay > 0)
                duration += _particleIncrementDelay * _particles.Length; // add the particle time to have them end at once

            eternal = (_destroyOnDuration <= 0);

            while (eternal || duration > 0)
            {
                for (int i = 0; i < _floatingEffects.Length; i++)
                {
                    if (_floatingEffects[i])
                    {
                        Vector2 pos = _floatingEffects[i].transform.localPosition;
                        _floatingEffectSpeeds[i] -= pos.normalized * _floatingRubber * Time.deltaTime;
                        pos += _floatingEffectSpeeds[i] * Time.deltaTime;
                        _floatingEffects[i].transform.localPosition = pos;
                    }
                    else
                        Debug.LogWarning("CSEffectTimed _floatingEffects " + i + " is null");
                }
                yield return new WaitForEndOfFrame();
                if (_countDown <= 0)
                    duration -= Time.deltaTime;
            }
            for (int i = 0; i < _floatingEffects.Length; i++)
                Destroy(_floatingEffects[i]);
        }

        // fade out the sprite
        private IEnumerator EffectSprite()
        {
            if (!_sprite)
                yield break;

            float timeSpent = 0;
            Color colorUpdate = _sprite.color;

            _sprite.enabled = true;

            while (_countDown > 0)
                yield return new WaitForEndOfFrame();

            while (timeSpent < _spriteFade.length)
            {
                colorUpdate.a = _spriteFade.Evaluate(timeSpent);
                _sprite.color = colorUpdate;
                timeSpent += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            _sprite.enabled = false;
        }
    }
}