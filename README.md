# UniDeferred
JQuery.Deferred-like AsyncLibrary for Unity. This library provide a very ultra super easy async functions.

## Example

#### super simply www example.

```CSharp
var www = new WWW('http://3d.nicovideo.jp/');
Deferred.Defer(www).Done( p => Debug.Log(www.text));
```
_*How hard can it be?*_

#### coroutine example.

```CSharp
void Start() {
  Deferred.Defer<Int32>((DeferredCoroutine)loop).Done(p => {
    Debug.LogError("Result:" + p.GetResult<Int32>()));
  }).Fail(p => {
    Debug.LogError(p.Error);
  });
}

IEnumerator loop(IDeferred d) {
    int cnt = 0;
    while(cnt < 10) {
      yield return null;
    }
    
    if(cnt == 9) {
      d.Resolve(cnt);
    } else {
      d.Reject('Failed!');
    }
}
```

### coroutine with progress.

```CSharp
void Start() {
  Deferred.Defer<Int32>((DeferredCoroutine)loop).Progress(p => {
    Debug.Log(URL + ": " + p.ProgressValue + "%");
  });
}

IEnumerator loop(IDeferred d) {
    int cnt = 0;
    while(cnt < 10) {
      d.Notify((float)cnt / 10);
      yield return null;
    }
    d.Resolve(cnt);
}
```

### serial execution
```CSharp
void Start () {
  // Call next action, When A Resolved.
  Deferred.Defer<Int32>((DeferredCoroutine)CoroutineA).Done(p => {
    Debug.Log("A: Resolved");
  }).Fail(p => {
    Debug.Log("A: Rejected");
  }).Next<Int32>((DeferredCoroutine)CoroutineA).Done(p => {
    Debug.Log("B: Resolved");
  }).Fail(p => {
    Debug.Log("B: Rejected");
  });
}

IEnumerator CoroutineA(IDeferred d) {
  yield return null;
  d.Resolve(1);
}

IEnumerator CoroutineB(IDeferred d) {
  yield return null;
  d.Resolve(2);
}
```

## License
This library is released under the MIT license.
