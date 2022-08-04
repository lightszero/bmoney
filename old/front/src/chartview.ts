import klinecharts from "klinecharts";

export type IndicatorMap = {[id:string]:object};
export class PickPoint
{
    public paneId?:string;
    public coordinate?:{x:number,y:number};
    public dataIndex?:number;
    public kLineData?:klinecharts.KLineData;
    public technicalIndicatorData?:{[paneid:string]:IndicatorMap};
}
export class ChartView
{
    public chart: klinecharts.Chart;
    public pick:PickPoint;
    constructor(div: HTMLDivElement)
    {
        let chartArea = document.getElementById("klinearea") as HTMLDivElement;
        let c = klinecharts.init(chartArea);
        if (c == null)
            throw "error init kline.";
        this.chart = c;


        this.InitIndicator();
        this.pick={};
        this.chart.subscribeAction('crosshair', (param) =>
        {
            this.pick = param;
            console.warn(JSON.stringify(this.pick.coordinate));
            console.warn(JSON.stringify(this.pick.kLineData));
            console.warn(JSON.stringify(this.pick.technicalIndicatorData));
        });
    }
    
    InitIndicator()
    {
        //开指标
        let paneid = this.chart.createTechnicalIndicator("MACD");


        this.chart.createTechnicalIndicator("VOL");

        //主图混
        this.chart.createTechnicalIndicator({ name: "MA", calcParams: [5, 10] }, true, { id: 'candle_pane' });

        //覆盖混在别的图上
        if (paneid != null)
        {
            this.chart.createTechnicalIndicator('KDJ', true, { id: paneid })
        }

        // 添加一个指标模板
        this.chart.addTechnicalIndicatorTemplate({
            name: 'TEST',
            calcParams: [{ allowDecimal: true, value: 5.5 }],
            precision: 2,
            plots: [{ key: 'price', title: 'price: ', type: 'line' },
            { key: 'price2', title: 'price2: ', type: 'line' }],
            calcTechnicalIndicator: function (kLineDataList, { params, plots })
            {
                return Promise.resolve(kLineDataList.map(kLineData => ({ price: kLineData.close / params[0], price2: kLineData.close / params[0] - 20 })))
            }
        }
        );
        this.chart.createTechnicalIndicator('TEST');
    }
    public Resize():void
    {
        this.chart.resize();
    }

    public ClearKdata(): void
    {
        this.chart.clearData();
    }
    public AddKData(dataList: klinecharts.KLineData[]): void
    {
        this.chart.applyMoreData(dataList, true);
    }
    public UpdateLastKData(data: klinecharts.KLineData): void
    {
        this.chart.updateData(data);
    }
}