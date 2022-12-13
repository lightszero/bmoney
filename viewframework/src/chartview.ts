import klinecharts, { KLineData, TechnicalIndicatorPlot, TechnicalIndicatorPlotType } from "klinecharts";

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
    title?:string;
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


        this.chart.createTechnicalIndicator("VOL");


        //let paneid = this.chart.createTechnicalIndicator("MACD");
        //this.chart.createTechnicalIndicator("KDJ");

        //主图混
        this.chart.createTechnicalIndicator({ name: "EMA", calcParams: [12] }, true, { id: 'candle_pane' });

        //覆盖混在别的图上
         //if (paneid != null) {
         //    this.chart.createTechnicalIndicator('KDJ', true, { id: paneid })
         //}

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
    mapIDs:{[id:string]:string}={};
    RegADataDesc(descs: input_IndicatorDescs): void {
        console.warn("==RegADataDesc==");
        this.adatadesc = descs;

        for (let key in this.adatadesc) {
            let desc = this.adatadesc[key];
            let iname = desc.name;
            let indname = "CC_" + desc.name;
            // 添加一个指标模板
            if (desc.values == null) throw "";
            let valuecount = desc.values.length;
            let plots: TechnicalIndicatorPlot[] = [];
            let valuenames: string[] = [];
            let skipcount = 0;
            for (let i = 0; i < valuecount; i++) {
                valuenames.push(desc.values[i]);

                if (desc.values[i].indexOf("*") == 0) {
                    skipcount++;
                    continue;
                }
                if(valuenames[i].indexOf("$")>0)
                {
                    let info  =valuenames[i].split('$');
                    var _st =info[1] as TechnicalIndicatorPlotType;
                    if(info.length>=2)
                    {
                        var _bv =parseFloat(info[2]);
                        plots.push({
                            key: valuenames[i], title: info[0] + ":", type:_st, baseValue: _bv
                             ,
                             "color": (_data, _option) => {
                                 //console.warn(_data.current?.technicalIndicatorData[valuenames[i]]>0);
                                 return _data.current?.technicalIndicatorData[valuenames[i]]>_bv?"red":"green";
                             }
                        });
                    }
                    else
                    {
                        plots.push({ key: valuenames[i], title: info[0] + ":", type: _st });
                    }
                  
                }
                else {
                    plots.push({ key: valuenames[i], title: valuenames[i] + ":", type: "line" });
                }
            }
            let clousethis = this;
            let closuefunc = function (kLineDataList: KLineData[], options?: any): any[] {
                var array: any[] = [];
                for (var i = 0; i < kLineDataList.length; i++) {
                    var obj: { [id: string]: Number } = {};

                    if (clousethis.adata != null && iname != null) {
                        let values = clousethis.adata[i][iname];


                        for (var j = 0; j < valuecount; j++) {
                            {
                                obj[valuenames[j]] = values[j];
                            }

                        }
                    }
                    array.push(obj);
                }
                return array;
            }
            this.chart.addTechnicalIndicatorTemplate(
                {
                    "name": indname,
                    "calcParams": [{ allowDecimal: true, value: [desc.title]}],
                    "precision": valuecount - skipcount,
                    "plots": plots,

                    calcTechnicalIndicator: closuefunc
                }
            );
            console.warn("addTechnicalIndicatorTemplate:" + indname);

            this.mapIDs[indname]=this.chart.createTechnicalIndicator(indname);
            {
            var viewlist =document.getElementById("viewlist") as HTMLDivElement;
            var span =document.createElement("span");
            var span2 =document.createElement("span");
            let cb =document.createElement("input");
            var br =document.createElement("br");
            span.innerText="View:";
            span2.innerText =desc.title+":"+desc.desc;
            cb.type="checkbox";
            cb.checked=true;
            cb.onchange=(e)=>
            {
                if(cb.checked)
                { console.log("checked.");
                     this.mapIDs[indname]=this.chart.createTechnicalIndicator(indname);
                }
                else
                {
                    this.chart.removeTechnicalIndicator(this.mapIDs[indname]);
                    console.log("unchecked.");
                }
            }
            viewlist.appendChild(span);
            viewlist.appendChild(cb);
            viewlist.appendChild(span2);
            viewlist.appendChild(br);
            
            }
        }
    }
}