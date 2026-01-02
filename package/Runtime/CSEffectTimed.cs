using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// displays an effect in the scene for a limited duration
// could include sprites, particles, etc
// used for blood splashes, explosions, bullet sparks
// oldest date 2/11/23
// modified 12/2/24

public class CSEffectTimed : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private AnimationCurve spriteFade = AnimationCurve.Linear(0, 1, 1, 0);
    [SerializeField] private ParticleSystem[] particles; // an array of particle systems
    [SerializeField] private int particleGroup = 1; // the group size of particles - how many particles should be played at once
    [SerializeField] private CSEffectTimed[] effects;
    [Header("Floating effects")]
    [SerializeField] private CSEffectTimed floatingEffectPrefab;
    [SerializeField] private int floatingCountMin = 3;
    [SerializeField] private int floatingCountMax = 5;
    [SerializeField] private float floatingDistance = 1f;
    [SerializeField] private float floatingRubber = 1f; // how fast to accelerate towards 0
    [SerializeField] private float floatingSpeed = 1f; // how fast it starts moving
    [Header("Particle positioning")]
    [SerializeField] private float particleRandomSpread = 0f; // the maximum amount of random repositioning for each particle system
    [SerializeField] private float particleKickY = 0f;
    [Header("Particle timing")]
    [SerializeField] private float particleIncrementDelay = 0f; // the time delay between each particle system being triggered
    [SerializeField] private float destroyOnDuration;
    [SerializeField] private bool disableNotDestroy;
    [SerializeField] private int countDownDelay = 0; // if > 0, this waits for X "turns" before the delay starts (make sure to tell this what a turn is!)
    [SerializeField] private GameObject countDownObject; // if non-null, this object will be set active as long as the countdown is > 0
    private Color colorOverride;
    private int particleGroupIncrement;
    private Vector3 particleGroupPos;
    private CSEffectTimed[] floatingEffects = new CSEffectTimed[0];
    private Vector2[] floatingEffectSpeeds = new Vector2[0];
    protected int countDown;

    // start the effect sequence
    public void PlayEffect(int timeOverride = -1)
    {
        gameObject.SetActive(true);
        if (timeOverride >= 0)
            countDown = timeOverride;
        else
            countDown = countDownDelay;
        //Debug.Log("Playing effect " + name + " at " + transform.position);
        for (int i = 0; i < effects.Length; i++)
            effects[i].PlayEffect(timeOverride);
        PlayEffectCustom();
        StartCoroutine(EffectTimer());
        if (sprite)
            StartCoroutine(EffectSprite());

        if (floatingEffectPrefab)
        {
            int count = Random.Range(floatingCountMin, floatingCountMax + 1);
            floatingEffects = new CSEffectTimed[count];
            floatingEffectSpeeds = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = CSUtils.RandomVector2D();
                Vector2 speed = CSUtils.RandomVector2D() - offset;
                floatingEffects[i] = Instantiate(floatingEffectPrefab, transform);
                floatingEffects[i].transform.localPosition = offset * floatingDistance;
                floatingEffectSpeeds[i] = speed * floatingSpeed;
                floatingEffects[i].PlayEffect();
            }
            StartCoroutine(FloaterTimer());
        }
    }
    public void PlayEffect(Color colorSet)
    {
        colorOverride = colorSet;
        PlayEffect();
    }

    public void StopEffect()
    {
        StopAllCoroutines();
        StopEffectCustom();
        if (sprite)
            sprite.enabled = false;
        for (int i = 0; i < particles.Length; i++)
            particles[i].Stop();
        for (int i = 0; i < effects.Length; i++)
            effects[i].StopEffect();
        for (int i = 0; i < floatingEffects.Length; i++)
        {
            floatingEffects[i].StopEffect();
            Destroy(floatingEffects[i].gameObject);
        }
        gameObject.SetActive(false);
    }
    protected virtual void StopEffectCustom()
    {

    }

    public bool TurnEnd()
    {
        countDown--;
        return (countDown <= 0);
    }
    public virtual void PlayEffectCustom() { }

    // this is for orienting the effect relative to an origin
    public void PlayEffectDirected(Vector3 direction)
    {
        if (particleKickY > 0)
            direction.y += particleKickY; // shift the direction up slightly to make the particles bounce up off the floor

        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg);

        PlayEffect();
    }
    public void PlayEffectDirected(Vector3 direction, Color colorSet)
    {
        colorOverride = colorSet;
        PlayEffectDirected(direction);
    }
    public void PlayEffectOriented(Vector3 origin)
    {
        Vector3 direction = (transform.position - origin).normalized;

        PlayEffectDirected(direction);
    }
    public void PlayEffectOriented(Vector3 origin, Color colorSet)
    {
        colorOverride = colorSet;
        PlayEffectOriented(origin);
    }

    // this coroutine always runs so handles anything critical e.g. Destroy
    // plays particle sysems
    private IEnumerator EffectTimer()
    {
        particleGroupIncrement = 0;

        if (countDown > 0 && countDownObject) countDownObject.SetActive(true);

        for (int i = 0; i < particles.Length; i++)
        {
            if (particleRandomSpread > 0)
            {
                if (particleGroupIncrement == 0)
                    particleGroupPos = new Vector3(Random.Range(-particleRandomSpread, particleRandomSpread), Random.Range(-particleRandomSpread, particleRandomSpread));

                particles[i].transform.localPosition = particleGroupPos;
            }

            if (colorOverride.a > 0)
            {
                Color colorOverrideDark = Color.Lerp(colorOverride, Color.black, 0.5f);
                ParticleSystem.MainModule particleMain = particles[i].main;
                //particleMain.startColor = colorOverride;
                particleMain.startColor = new ParticleSystem.MinMaxGradient(colorOverride, colorOverrideDark);
            }
            particles[i].Play();
            //Debug.Log("particle effect " + i + " playing");

            if (particleGroup > 1)
            {
                // with particle groups, only pause between groups
                particleGroupIncrement++;
                if (particleGroupIncrement == particleGroup && particleIncrementDelay > 0)
                {
                    particleGroupIncrement = 0;
                    yield return new WaitForSeconds(particleIncrementDelay);
                }
            }
            else if (particleIncrementDelay > 0)
                    yield return new WaitForSeconds(particleIncrementDelay);
        }
        while (countDown > 0)
            yield return new WaitForEndOfFrame();

        if (countDownObject) countDownObject.SetActive(false);

        if (destroyOnDuration > 0)
        {
            yield return new WaitForSeconds(destroyOnDuration);
            if (disableNotDestroy)
                StopEffect();
            else
                Destroy(gameObject);
        }
    }

    private IEnumerator FloaterTimer()
    {
        float duration = 0f;
        bool eternal;

        if (destroyOnDuration > 0)
            duration = destroyOnDuration;
        if (particleIncrementDelay > 0)
            duration += particleIncrementDelay * particles.Length; // add the particle time to have them end at once

        eternal = (destroyOnDuration <= 0);

        while (eternal || duration > 0)
        {
            for (int i = 0; i < floatingEffects.Length; i++)
            {
                Vector2 pos = floatingEffects[i].transform.localPosition;
                floatingEffectSpeeds[i] -= pos.normalized * floatingRubber * Time.deltaTime;
                pos += floatingEffectSpeeds[i] * Time.deltaTime;
                floatingEffects[i].transform.localPosition = pos;
            }
            yield return new WaitForEndOfFrame();
            if (countDown <= 0)
                duration -= Time.deltaTime;
        }
        for (int i = 0; i < floatingEffects.Length; i++)
            Destroy(floatingEffects[i]);
    }

    // fade out the sprite
    private IEnumerator EffectSprite()
    {
        float timeSpent = 0;
        Color colorUpdate = sprite.color;

        sprite.enabled = true;

        while (countDown > 0)
            yield return new WaitForEndOfFrame();

        while (timeSpent < spriteFade.length)
        {
            colorUpdate.a = spriteFade.Evaluate(timeSpent);
            sprite.color = colorUpdate;
            timeSpent += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        sprite.enabled = false;
    }
}
