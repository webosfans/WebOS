function DomInvoke(obj, funcName, ...args) {
    return obj[funcName](...args);
}

function DomGetSize(obj) {
    return [obj.clientWidth, obj.clientHeight];
}

window.onresize = () => {
    DotNet.invokeMethodAsync("Desktop", 'OnBrowserResize').then(data => data);
}