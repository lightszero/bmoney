import klinecharts, { TechnicalIndicatorPlot } from "klinecharts";

export type IndicatorMap = { [id: string]: object };
export class PickPoint {
    public paneId?: string;
    public coordinate?: { x: number, y: number };
    public dataIndex?: number;
    public kLineData?: klinecharts.KLineData;
    public technicalIndicatorData?: { [paneid: string]: IndicatorMap };
}

export class input_IndicatorDesc {
    name?: string;
    desc?: string;
    initparam?: string[];
    values?: string[]
}
export type input_IndicatorDescs = { [id: string]: input_IndicatorDesc };

export type input_IndicatorValues = number[];
export type input_IndicatorData = { [id: string]: number[] };
export type input_IndicatorDatas = input_IndicatorData[];

export class ChartView {
    public chart: klinecharts.Chart;
    public pick: PickPoint;
    public divAndicatorView: HTMLDivElement;
    public adata?: input_IndicatorDatas;
    public adatadesc?: input_IndicatorDescs;
    constructor(div: HTMLDivElement) {
        let chartArea = document.getElementById("klinearea") as HTMLDivElement;
        let c = klinecharts.init(chartArea);
        if (c == null)
            throw "error init kline.";
        this.chart = c;


        this.divAndicatorView = document.getElementById("andicator_value") as HTMLDivElement;
        this.InitIndicator();
        this.pick = {};
        this.chart.subscribeAction('crosshair', (param) => {
            this.pick = param;


            let time = document.getElementById("label_time") as HTMLSpanElement;
            let open = document.getElementById("label_value_open") as HTMLSpanElement;
            let high = document.getElementById("label_value_high") as HTMLSpanElement;
            let low = document.getElementById("label_value_low") as HTMLSpanElement;
            let close = document.getElementById("label_value_close") as HTMLSpanElement;
            let volume = document.getElementById("label_value_volume") as HTMLSpanElement;
            if (this.pick.kLineData != null) {
                var timenum: number = this.pick.kLineData.timestamp;
                var date = new Date();
                time.textContent = "Time:" + date.toString();
                open.textContent = "开盘:" + this.pick.kLineData.open;
                high.textContent = "最高:" + this.pick.kLineData.high;
                low.textContent = "最低:" + this.pick.kLineData.low;
                close.textContent = "收盘:" + this.pick.kLineData.close;
                volume.textContent = "成交:" + this.pick.kLineData.volume;
            }
            else {
                time.textContent = "Time:";
                open.textContent = "开盘:";
                high.textContent = "最高:";
                low.textContent = "最低:";
                close.textContent = "收盘:";
                volume.textContent = "成交:";
            }

            //console.warn(JSON.stringify(this.pick.coordinate));
            //console.warn(JSON.stringify(this.pick.kLineData));
            console.warn(JSON.stringify(this.pick.technicalIndicatorData));
        });
    }

    InitIndicator() {
        //开指标
        let paneid = this.chart.createTechnicalIndicator("MACD");


        this.chart.createTechnicalIndicator("VOL");

        //主图混
        this.chart.createTechnicalIndicator({ name: "MA", calcParams: [5, 10] }, true, { id: 'candle_pane' });

        //覆盖混在别的图上
        if (paneid != null) {
            this.chart.createTechnicalIndicator('KDJ', true, { id: paneid })
        }

        // // 添加一个指标模板
        // this.chart.addTechnicalIndicatorTemplate({
        //     name: 'TEST',
        //     calcParams: [{ allowDecimal: true, value: 5.5 }],
        //     precision: 2,
        //     plots: [{ key: 'price', title: 'price: ', type: 'line' },
        //     { key: 'price2', title: 'price2: ', type: 'line' }],
        //     calcTechnicalIndicator: function (kLineDataList, { params, plots }) {
        //         return Promise.resolve(kLineDataList.map(kLineData => ({ price: kLineData.close / params[0], price2: kLineData.close / params[0] - 20 })))
        //     }
        // }
        // );
        // this.chart.createTechnicalIndicator('TEST');
    }
    public Resize(): void {
        this.chart.resize();
    }

    public ClearKdata(): void {
        this.chart.clearData();
    }
    public AddKData(dataList: klinecharts.KLineData[]): void {
        this.chart.applyMoreData(dataList, true);
    }
    public UpdateLastKData(data: klinecharts.KLineData): void {
        this.chart.updateData(data);
    }
    public AddAData(data: input_IndicatorDatas): void {
        this.adata = data;
    }
    RegADataDesc(descs: input_IndicatorDescs): void {
        console.warn("==RegADataDesc==");
        this.adatadesc = descs;

        for (var key in this.adatadesc) {
            var desc = this.adatadesc[key];
            var indname = "CC_" + desc.name;
            // 添加一个指标模板
            if (desc.values == null) throw "";
            var valuecount = desc.values.length;
            var plots: TechnicalIndicatorPlot[] = [];
            var valuenames: string[] = [];
            for (var i = 0; i < valuecount; i++) {
                valuenames.push(desc.values[i]);
                plots.push({ key: valuenames[i], title: desc.values[i], type: "line" });
            }

            this.chart.addTechnicalIndicatorTemplate(
                {
                "name": indname,
                "calcParams": [{ allowDecimal: true, value: 5.5 }],
                "precision": valuecount,
                "plots": plots,

                calcTechnicalIndicator: async function (kLineDataList, { params, plots }): Promise<any[]> {
                    var array: any[] = [];
                    for (var i = 0; i < kLineDataList.length; i++) {
                        var obj: { [id: string]: Number } = {};
                        for (var j = 0; j < valuecount; j++) {
                            {
                                obj[valuenames[j]] = i+j;
                            }
                          
                        }
                        array.push(obj);
                    }
                    return array;
                }
                }
            );
            console.warn("addTechnicalIndicatorTemplate:" + indname);

            this.chart.createTechnicalIndicator(indname);
        }
    }
}