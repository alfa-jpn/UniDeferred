using System;
using System.Collections;
using UniDeferred;
using UnityEngine;

namespace Tests.IntegrationTests {
    public class WrapWWWTest : MonoBehaviour {
        public string URL = "http://www.google.co.jp/";

        void Start() {
            Deferred.Defer<String>((DeferredCoroutine)WrapWWW).Progress(p =>
                Debug.Log(URL + ": " + p.ProgressValue + "%")
            ).Done(p =>
                IntegrationTest.Pass()
            ).Fail(p =>
                Debug.LogError("Failed:" + p.Error)
            );
        }

        public IEnumerator WrapWWW(IDeferred d) {
            float progress = 0;

            WWW www = new WWW(URL);
            while (!www.isDone) {
                if (progress < www.progress) {
                    progress = www.progress;
                    d.Notify(progress);
                }
                yield return new WaitForSeconds(0.1f);
            }

            if (String.IsNullOrEmpty(www.error)) {
                d.Resolve(www.text);
            } else {
                d.Reject(www.error);
            }
        }
    }
}
