export function OnMouseMove(dom, mouseClientX, appIconMinWidth)
{
    const min = appIconMinWidth;
    const max = min * 1.5;
    for (var child of dom.children)
    {
        var rect = child.getBoundingClientRect();
        var delta = Math.abs(mouseClientX - (rect.x + rect.width / 2));
        var x = Math.min(max, Math.max(min, (1.5 - delta / 100) * 3.6));
        child.getElementsByTagName("img")[0].style.width = `${x}rem`;
    }
}

export function OnMouseLeave(dom, appIconMinWidth)
{
    var width = `${appIconMinWidth}rem`;
    for (var child of dom.children)
    {
        child.getElementsByTagName("img")[0].style.width = width;
    }
}