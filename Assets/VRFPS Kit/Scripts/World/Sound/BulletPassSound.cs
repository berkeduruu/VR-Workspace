using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VRFPSKit
{
    [RequireComponent(typeof(AudioSource))]
    public class BulletPassSound : MonoBehaviour
    {
        public float minimumVelocity = SpeedOfSound.Value;
        [Tooltip("Can be used to make whiz sounds be played earlier")] 
        public float bulletOffset = 0;
        public float behindSourceVolume =.5f;
        
        private static AudioListener _cachedListener;
        private Vector3 _lastListenerPositionLocal;
        private Vector3 _lastFramePosition;
        private Vector3 _lastFrameVelocity;
        private bool _hasPlayed;
        private bool _movingTowardsListenerAtSpawn;
        
        private Bullet _bullet;

        // Update is called once per frame
        void FixedUpdate()
        {
            AudioListener listener = GetListener();
            if (listener == null) return;
            if (_hasPlayed) return;

            // Get the listener position in local space
            Vector3 listenerPositionLocal = ListenerPositionLocal();
    
            if (_lastFrameVelocity.magnitude > minimumVelocity)
            {
                float remainingDistanceToListener = listenerPositionLocal.z - bulletOffset;
                float remainingDistanceToListenerLastFrame = _lastListenerPositionLocal.z - bulletOffset;

                //Check if just passed listener
                if (remainingDistanceToListenerLastFrame <= 0 && remainingDistanceToListener >= 0){

                    //Deparent the audio source so sound doesn't keep traveling
                    transform.position += transform.forward * ListenerPositionLocal().z;;
                    
                    transform.parent = null;
                    GetComponent<AudioSource>().PlayDelayedBySpeedOfSound();
                    Destroy(gameObject, 5);
                    _hasPlayed = true;
                }

                //Check if stop moving
                if (_lastFrameVelocity.magnitude < .2f && _movingTowardsListenerAtSpawn)
                {
                    GetComponent<AudioSource>().PlayDelayedBySpeedOfSound();
                    transform.parent = null;
                    Destroy(gameObject, 5);
                    _hasPlayed = true;
                }
            }

            UpdateLastFrameValues();
        }

        private void UpdateLastFrameValues()
        {
            _lastListenerPositionLocal = ListenerPositionLocal();
            _lastFramePosition = transform.position;
            _lastFrameVelocity = EstimateVelocity();
        }

        private void Start()
        {
            if(ListenerPositionLocal().z - bulletOffset > 0) _movingTowardsListenerAtSpawn = true;

            _lastFrameVelocity = transform.forward * _bullet.ballisticProfile.startVelocity;
            
            //If we're already past the listener and moving away, just play the sound immediately
            if (!_movingTowardsListenerAtSpawn && _bullet.ballisticProfile.startVelocity > minimumVelocity)
            {
                //Reduce volume if behind
                float behindFactor01 = Mathf.Clamp01(Vector3.Dot(-transform.forward, ListenerPositionLocal().normalized)); //1 when directly behind, 0 when perpendicular, -1 when in front
                GetComponent<AudioSource>().volume = Mathf.SmoothStep(1, behindSourceVolume, behindFactor01);
                
                GetComponent<AudioSource>().PlayDelayedBySpeedOfSound();
                transform.parent = null;
                Destroy(gameObject, 5);
                _hasPlayed = true;
            }
        }
        
        /// <summary>
        /// We need to estimate velocity by measuring how far we've traveled since last frame as we can't
        /// Use RigidBody.velocity in multiplayer
        /// </summary>
        /// <returns></returns>
        /// TODO WARNING can be null if listener is null
        private Vector3 EstimateVelocity() => (_lastFramePosition - transform.position) / Time.fixedDeltaTime;
        
        private float GetRemainingVelocity01() => Mathf.InverseLerp(0, _bullet.ballisticProfile.startVelocity, EstimateVelocity().magnitude); 
        
        private Vector3 ListenerPositionLocal() => transform.InverseTransformPoint(GetListener().transform.position);
        
        private static AudioListener GetListener()
        {
            if(_cachedListener == null || !_cachedListener.enabled) 
                _cachedListener = Object.FindAnyObjectByType<AudioListener>(FindObjectsInactive.Exclude);

            return _cachedListener;
        }

        private void Awake()
        {
            _bullet = GetComponentInParent<Bullet>();
        }
    }
}
