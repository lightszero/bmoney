
import * as klinecharts from "klinecharts"
import { ChartView } from "./chartview";
import { generateTestData } from "./testdata";

function annotationDrawExtend(ctx: CanvasRenderingContext2D, coordinate: klinecharts.Coordinate, text: string): void
{
    ctx.font = '12px Roboto';
    ctx.fillStyle = '#2d6187';
    ctx.strokeStyle = '#2d6187';
    if (coordinate.x == null || coordinate.y == null)
        throw "";
    const textWidth = ctx.measureText(text).width;
    const startX = coordinate.x;
    let startY = coordinate.y + 6;
    ctx.beginPath();
    ctx.moveTo(startX, startY);
    startY += 5;
    ctx.lineTo(startX - 4, startY);
    ctx.lineTo(startX + 4, startY);
    ctx.closePath();
    ctx.fill();

    const rectX = startX - textWidth / 2 - 6;
    const rectY = startY;
    const rectWidth = textWidth + 12;
    const rectHeight = 28;
    const r = 2;
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
window.onload = () =>
{
    console.log("hello");


    let chartArea = document.getElementById("klinearea") as HTMLDivElement;
    let view = new ChartView(chartArea);

    let dataList = generateTestData();

    view.AddKData(dataList);

    let p = dataList[dataList.length - 20];

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
                type:'diamond',
                
            }
        },
        drawCustomSymbol: function ({ ctx, coordinate, isActive })
        {
            if (coordinate == null || coordinate.x == null || coordinate.y == null)
                throw "";
            var color
            var size
            if (isActive)
            {
                color = '#6767dd'
                size = 6
            } else
            {
                color = '#dd22fc'
                size = 4
            }
            ctx.fillStyle = color
            ctx.beginPath()
            ctx.moveTo(coordinate.x - size, coordinate.y - size)
            ctx.lineTo(coordinate.x + size, coordinate.y - size)
            ctx.lineTo(coordinate.x + size, coordinate.y + size)
            ctx.lineTo(coordinate.x - size, coordinate.y + size)
            ctx.closePath()
            ctx.fill()
        },
        drawExtend: (params: klinecharts.AnnotationDrawParams) =>
        {
            if (params.coordinate == null)
                throw "";
            annotationDrawExtend(params.ctx, params.coordinate, '这是一个固定显示标记');
        }
    });




    window.onresize = () =>
    {
        view.Resize();
    }
}


