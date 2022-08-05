"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.ChartView = exports.PickPoint = void 0;
var klinecharts_1 = __importDefault(require("klinecharts"));
var PickPoint = /** @class */ (function () {
    function PickPoint() {
    }
    return PickPoint;
}());
exports.PickPoint = PickPoint;
var ChartView = /** @class */ (function () {
    function ChartView(div) {
        var _this = this;
        var chartArea = document.getElementById("klinearea");
        var c = klinecharts_1.default.init(chartArea);
        if (c == null)
            throw "error init kline.";
        this.chart = c;
        this.InitIndicator();
        this.pick = {};
        this.chart.subscribeAction('crosshair', function (param) {
            _this.pick = param;
            console.warn(JSON.stringify(_this.pick.coordinate));
            console.warn(JSON.stringify(_this.pick.kLineData));
            console.warn(JSON.stringify(_this.pick.technicalIndicatorData));
        });
    }
    ChartView.prototype.InitIndicator = function () {
        //开指标
        var paneid = this.chart.createTechnicalIndicator("MACD");
        this.chart.createTechnicalIndicator("VOL");
        //主图混
        this.chart.createTechnicalIndicator({ name: "MA", calcParams: [5, 10] }, true, { id: 'candle_pane' });
        //覆盖混在别的图上
        if (paneid != null) {
            this.chart.createTechnicalIndicator('KDJ', true, { id: paneid });
        }
        // 添加一个指标模板
        this.chart.addTechnicalIndicatorTemplate({
            name: 'TEST',
            calcParams: [{ allowDecimal: true, value: 5.5 }],
            precision: 2,
            plots: [{ key: 'price', title: 'price: ', type: 'line' },
                { key: 'price2', title: 'price2: ', type: 'line' }],
            calcTechnicalIndicator: function (kLineDataList, _a) {
                var params = _a.params, plots = _a.plots;
                return Promise.resolve(kLineDataList.map(function (kLineData) { return ({ price: kLineData.close / params[0], price2: kLineData.close / params[0] - 20 }); }));
            }
        });
        this.chart.createTechnicalIndicator('TEST');
    };
    ChartView.prototype.Resize = function () {
        this.chart.resize();
    };
    ChartView.prototype.ClearKdata = function () {
        this.chart.clearData();
    };
    ChartView.prototype.AddKData = function (dataList) {
        this.chart.applyMoreData(dataList, true);
    };
    ChartView.prototype.UpdateLastKData = function (data) {
        this.chart.updateData(data);
    };
    return ChartView;
}());
exports.ChartView = ChartView;
//# sourceMappingURL=chartview.js.map