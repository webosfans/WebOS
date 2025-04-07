function DomInvoke(obj, funcName, ...args) {
    return obj[funcName](...args);
}

function DomGetSize(obj) {
    return [obj.clientWidth, obj.clientHeight];
}

function SessionSaveItem(key, value) {
    sessionStorage.setItem(key, value);
}

function SessionLoadItem(key) {
    return sessionStorage.getItem(key);
}

function SessionLoadAndCleanItem(key) {
    var val = sessionStorage.getItem(key);
    sessionStorage.removeItem(key);
    return val;
}

window.onresize = () => {
    DotNet.invokeMethodAsync("Desktop", 'OnBrowserResize').then(data => data);
}

window.onmessage = async (evt) => {
    var ret = await DotNet.invokeMethodAsync("Desktop", "OnMessage", JSON.stringify({
        "origin": evt.origin,
        "data": evt.data
    }));

    if (ret != null) {
        evt.source.postMessage(ret, evt.origin);
    }
}