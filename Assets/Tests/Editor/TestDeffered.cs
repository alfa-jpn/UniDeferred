using NUnit.Framework;
using System.Collections;
using UniDeferred;

namespace Tests {
    [TestFixture]
    public class TestDeffered {
        private class CallCounter {
            public int Done;
            public int Fail;
            public int Always;
            public int Progress;
            public int Next;
            public int Coroutine;
        }
        
        [Test]
        public void StateTest() {
            Deferred<None> d = new Deferred<None>();
            Assert.AreEqual(StateType.Pending, d.State);

            d.Resolve();
            Assert.AreEqual(StateType.Resolved, d.State);

            d.Reject();
            Assert.AreEqual(StateType.Rejected, d.State);
        }

        [Test]
        public void DoneCallbackTest() {
            CallCounter counter = new CallCounter();
            Deferred<None> d = new Deferred<None>();

            d.Done(p =>
                counter.Done++
            ).Fail(p =>
                counter.Fail++
            ).Progress(p =>
                counter.Progress++
            );

            d.Resolve();

            Assert.AreEqual(1, counter.Done);
            Assert.AreEqual(0, counter.Fail);
            Assert.AreEqual(0, counter.Progress);
        }

        [Test]
        public void FailCallbackTest() {
            CallCounter counter = new CallCounter();
            Deferred<None> d = new Deferred<None>();

            d.Done(p =>
                counter.Done++
            ).Fail(p =>
                counter.Fail++
            ).Progress(p =>
                counter.Progress++
            );

            d.Reject();

            Assert.AreEqual(0, counter.Done);
            Assert.AreEqual(1, counter.Fail);
            Assert.AreEqual(0, counter.Progress);
        }

        [Test]
        public void AlwaysCallbackTest() {
            CallCounter counter = new CallCounter();
            Deferred<None> d = new Deferred<None>();

            d.Always(p =>
                counter.Always++
            );

            d.Resolve();
            d.Reject();

            Assert.AreEqual(2, counter.Always);
        }

        [Test]
        public void ProgressCallbackTest() {
            CallCounter counter = new CallCounter();
            Deferred<None> d = new Deferred<None>();

            d.Done(p =>
                counter.Done++
            ).Fail(p =>
                counter.Fail++
            ).Progress(p =>
                counter.Progress++
            );

            d.Notify(1);

            Assert.AreEqual(0, counter.Done);
            Assert.AreEqual(0, counter.Fail);
            Assert.AreEqual(1, counter.Progress);
        }

        [Test]
        public void NextCallBackTest() {
            CallCounter counter = new CallCounter();
            Deferred<None> d = new Deferred<None>();

            d.Done(p =>
                counter.Done++
            ).Next<None>(df => {
                Assert.AreEqual(1, counter.Done);
                counter.Next++;
                df.Reject();
            }).Fail(p =>
                counter.Fail++
            ).Next<None>(df =>
                counter.Next++
            );

            d.Resolve();
            Assert.AreEqual(1, counter.Done);
            Assert.AreEqual(1, counter.Fail);
            Assert.AreEqual(1, counter.Next);
        }

        [Test]
        public void GetResultTest() {
            CallCounter counter = new CallCounter();
            Deferred<System.String> d = new Deferred<System.String>();

            d.Done(p => {
                Assert.AreEqual("UniDeffered", p.GetResult<System.String>());
                counter.Done++;
            });

            d.Resolve("UniDeffered");
            Assert.AreEqual(1, counter.Done);
        }

        [Test]
        public void GetErrorTest() {
            CallCounter counter = new CallCounter();
            Deferred<System.String> d = new Deferred<System.String>();

            d.Fail(p => {
                Assert.AreEqual("UniDeffered", p.Error);
                counter.Fail++;
            });

            d.Reject("UniDeffered");
            Assert.AreEqual(1, counter.Fail);
        }

        [Test]
        public void GetProgressTest() {
            CallCounter counter = new CallCounter();
            Deferred<System.String> d = new Deferred<System.String>();

            d.Progress(p => {
                Assert.AreEqual(0.5f, p.ProgressValue);
                counter.Progress++;
            });

            d.Notify(0.5f);
            Assert.AreEqual(1, counter.Progress);
        }

        [Test]
        public void DeferTest() {
            CallCounter counter = new CallCounter();

            Deferred.Defer<None>(d =>
                d.Resolve()
            ).Done(p =>
                counter.Done++
            ).Next<None>(d => {
                counter.Next++;
                d.Resolve();
            }).Next<None>(d => {
                counter.Next++;
                d.Reject();
            });

            Assert.AreEqual(1, counter.Done);
            Assert.AreEqual(2, counter.Next);
        }

        [Test]
        public void WhenDoneTest() {
            CallCounter counter = new CallCounter();

            Deferred.When(
                Deferred.Defer<None>(d => { counter.Coroutine++; d.Resolve(); }),
                Deferred.Defer<None>(d => { counter.Coroutine++; d.Resolve(); }),
                Deferred.Defer<None>(d => { counter.Coroutine++; d.Resolve(); }),
                Deferred.Defer<None>(d => { counter.Coroutine++; d.Resolve(); }),
                Deferred.Defer<None>(d => { counter.Coroutine++; d.Resolve(); })
            ).Done(p => {
                counter.Done++;
            });

            Assert.AreEqual(5, counter.Coroutine);
            Assert.AreEqual(1, counter.Done);
        }

        [Test]
        public void WhenFailTest() {
            CallCounter counter = new CallCounter();

            Deferred.When(
                Deferred.Defer<None>(d => { counter.Coroutine++; d.Resolve(); }),
                Deferred.Defer<None>(d => { counter.Coroutine++; d.Resolve(); }),
                Deferred.Defer<None>(d => { counter.Coroutine++; d.Reject(); }),
                Deferred.Defer<None>(d => { counter.Coroutine++; d.Reject(); }),
                Deferred.Defer<None>(d => { counter.Coroutine++; d.Resolve(); })
            ).Fail(p => {
                counter.Fail++;
            });

            Assert.AreEqual(5, counter.Coroutine);
            Assert.AreEqual(1, counter.Fail);
        }
    }
}
