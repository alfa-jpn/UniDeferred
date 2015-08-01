using System.Collections;
using UniDeferred;
using UnityEngine;

namespace Tests.IntegrationTests {
    public class DeferIEnumeratorTest : MonoBehaviour {
        private int _loopCount;

        void Start() {
            _loopCount = 0;
            Deferred.Defer<None>((DeferredCoroutine)loop).Done(p => {
                if (_loopCount == 10) {
                    IntegrationTest.Pass();
                } else {
                    IntegrationTest.Assert(false);
                }
            });
        }

        IEnumerator loop(IDeferred d) {
            for (int i = 0; i < 10; ++i) {
                _loopCount++;
                yield return null;
            }
            d.Resolve();
        }
    }
}
