using LibVLCSharp;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace VLCUnity.Tests
{
    public class VLCInitializationTests
    {
        private GameObject _vlcObject;
        private VLCMediaPlayer _vlcMediaPlayer;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _vlcObject = new GameObject("VLCTestObject");

            _vlcObject.SetActive(false);

            _vlcMediaPlayer = _vlcObject.AddComponent<VLCMediaPlayer>();
            _vlcMediaPlayer.playOnAwake = false;

            if (Application.isBatchMode)
            {
                _vlcMediaPlayer.libVLCArguments = new string[] {
                    "--vout=vmem",
                    "--aout=dummy"
                };
            }

            _vlcObject.SetActive(true);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_vlcObject != null)
            {
                Object.DestroyImmediate(_vlcObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Plugin_Initializes_And_Reaches_Playing_State()
        {
            Assert.That(VLCMediaPlayer.LibVLC, Is.Not.Null);
            Assert.That(_vlcMediaPlayer.MediaPlayer, Is.Not.Null);

            _vlcMediaPlayer.Open("https://streams.videolan.org/misc/unity-samples/BigBuckBunny.avi");
            _vlcMediaPlayer.Play();

            float timeoutTime = Time.realtimeSinceStartup + 10f;

            yield return new WaitUntil(() =>
                _vlcMediaPlayer.CurrentState == VLCState.Playing ||
                _vlcMediaPlayer.CurrentState == VLCState.Error ||
                Time.realtimeSinceStartup > timeoutTime
            );

            Assert.That(_vlcMediaPlayer.CurrentState, Is.EqualTo(VLCState.Playing),
                $"Failed to reach Playing state. Final state: {_vlcMediaPlayer.CurrentState}");
        }
    }
}
