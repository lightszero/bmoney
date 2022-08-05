"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var chartview_1 = require("./chartview");
var testdata_1 = require("./testdata");
function annotationDrawExtend(ctx, coordinate, text) {
    ctx.font = '12px Roboto';
    ctx.fillStyle = '#2d6187';
    ctx.strokeStyle = '#2d6187';
    if (coordinate.x == null || coordinate.y == null)
        throw "";
    var textWidth = ctx.measureText(text).width;
    var startX = coordinate.x;
    var startY = coordinate.y + 6;
    ctx.beginPath();
    ctx.moveTo(startX, startY);
    startY += 5;
    ctx.lineTo(startX - 4, startY);
    ctx.lineTo(startX + 4, startY);
    ctx.closePath();
    ctx.fill();
    var rectX = startX - textWidth / 2 - 6;
    var rectY = startY;
    var rectWidth = textWidth + 12;
    var rectHeight = 28;
    var r = 2;
    ctx.beginPath();
    ctx.moveTo(rectX + r, rectY);
    ctx.arcTo(rectX + rectWidth, rectY, rectX + rectWidth, rectY + rectHeight, r);
    ctx.arcTo(rectX + rectWidth, rectY + rectHeight, rectX, rectY + rectHeight, r);
    ctx.arcTo(rectX, rectY + rectHeight, rectX, rectY, r);
    ctx.arcTo(rectX, rectY, rectX + rectWidth, rectY, r);
    ctx.closePath();
    ctx.fill();
    ctx.fillStyle = '#fff';
    ctx.textBaseline = 'middle';
    ctx.textAlign = 'center';
    ctx.fillText(text, startX, startY + 14);
}
window.onload = function () {
    console.log("hello");
    var chartArea = document.getElementById("klinearea");
    var view = new chartview_1.ChartView(chartArea);
    var dataList = (0, testdata_1.generateTestData)();
    view.AddKData(dataList);
    var p = dataList[dataList.length - 20];
    // {
    //     offset: [0, 20] // value , time 
    //     position: 'top', //top or point
    //     symbol: {
    //       type: 'diamond',
    //       size: 8,
    //       color: '#1e88e5',
    //       activeSize: 10,
    //       activeColor: '#FF9600',
    //     }
    //   },
    view.chart.createAnnotation({
        point: { timestamp: p.timestamp, value: p.close },
        styles: {
            position: 'point',
            offset: [-50, 0],
            symbol: {
                type: 'diamond',
            }
        },
        drawCustomSymbol: function (_a) {
            var ctx = _a.ctx, coordinate = _a.coordinate, isActive = _a.isActive;
            if (coordinate == null || coordinate.x == null || coordinate.y == null)
                throw "";
            var color;
            var size;
            if (isActive) {
                color = '#6767dd';
                size = 6;
            }
            else {
                color = '#dd22fc';
                size = 4;
            }
            ctx.fillStyle = color;
            ctx.beginPath();
            ctx.moveTo(coordinate.x - size, coordinate.y - size);
            ctx.lineTo(coordinate.x + size, coordinate.y - size);
            ctx.lineTo(coordinate.x + size, coordinate.y + size);
            ctx.lineTo(coordinate.x - size, coordinate.y + size);
            ctx.closePath();
            ctx.fill();
        },
        drawExtend: function (params) {
            if (params.coordinate == null)
                throw "";
            annotationDrawExtend(params.ctx, params.coordinate, '这是一个固定显示标记');
        }
    });
    window.onresize = function () {
        view.Resize();
    };
};
//# sourceMappingURL=main.js.map