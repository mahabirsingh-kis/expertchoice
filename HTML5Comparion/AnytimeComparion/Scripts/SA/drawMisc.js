function getFont(context, text, maxFontSize, minFontSize, maxWidth, fontFace) {
    if (maxWidth < 10) maxWidth = 10;
    var fontsize = maxFontSize;
    do {
        fontsize--;
        context.font = fontsize + 'px ' + fontFace;
        if (fontsize <= minFontSize) break;
    } while (context.measureText(text).width > maxWidth)
    return fontsize + 'px ' + fontFace;
}

function getTruncatedString(ctx, text, maxWidth) {
    if (ctx.measureText(text).width <= maxWidth) { return text };
    var words = text.split(" ");
    var result = text;
    while ((ctx.measureText(result).width > maxWidth) && (words.length > 1)) {
        words.pop();
        result = words.join(" ") + "...";
    };
    return result;
}

function drawButton(ctx, x, y, size, iconID, mode, rotateAngle) {
    var hsize = size / 2;
    var lsize = size / 10;
    var centerX = x + hsize;
    var centerY = y + hsize;
    var radAngle = Math.PI / 180 * rotateAngle;
    ctx.save();

    ctx.strokeStyle = '#fff';
    ctx.lineWidth = lsize;
    ctx.fillStyle = '#aaa';
    if (mode === 'over') {
        ctx.fillStyle = '#444';
    };
    ctx.beginPath();
    ctx.arc(x + hsize, y + hsize, hsize - lsize / 2, 0, 2 * Math.PI, false);
    ctx.fill();
    ctx.stroke();

    ctx.save();
    ctx.translate(centerX, centerY);
    ctx.rotate(radAngle);
    ctx.translate(-centerX, -centerY);

    switch (iconID) {
        case 'nextArrow':
            ctx.beginPath();
            ctx.moveTo(x + hsize - lsize, y + hsize - hsize / 2);
            ctx.lineTo(x + hsize + lsize * 2, y + hsize);
            ctx.lineTo(x + hsize - lsize, y + hsize + hsize / 2);
            ctx.stroke();
            break;
        case 'plus':
            ctx.beginPath();
            ctx.moveTo(x + hsize, y + hsize - lsize * 2);
            ctx.lineTo(x + hsize, y + hsize + lsize * 2);
            ctx.stroke();
            ctx.beginPath();
            ctx.moveTo(x + hsize - lsize * 2, y + hsize);
            ctx.lineTo(x + hsize + lsize * 2, y + hsize);
            ctx.stroke();
            break;
        case 'minus':
            ctx.beginPath();
            ctx.moveTo(x + hsize - lsize * 2, y + hsize);
            ctx.lineTo(x + hsize + lsize * 2, y + hsize);
            ctx.stroke();
            break;
        default:

    };
    ctx.restore();
    ctx.restore();
}