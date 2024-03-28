//判斷會員有沒有登入
var MID = sessionStorage.getItem("LogAccount")
var logging;
var user;
if (MID == null) {
    console.log("沒有登入");
    $(".member-logging").css("display", "none");
    $(".member-logout").css("display", "block");
    logging = false;
} else {
    console.log("顯示MID:" + MID);
    user = MID;
    $(".member-logout").css("display", "none");
    $(".member-logging").css("display", "block");
    logging = true;
}

var stocksID = $("#stocks-id").text();

$("#memberLogging").on("click", function () {
    window.location.href = "/Member/Login"
})
//圖表大小的設置
var margin = { top: 20, right: 50, bottom: 30, left: 50 },
    width = 600 - margin.left - margin.right,
    height = 400 - margin.top - margin.bottom;

var parseDate = d3.timeParse("%Y-%m-%d");       //設定日期格式

//X軸輸出範圍
var x = techan.scale.financetime()
    .range([0, width])

//Y軸整體輸出範圍(for十字線用)
var allY = d3.scaleLinear()
    .range([height, 0]);

//Y軸輸出範圍
var y = d3.scaleLinear()
    .range([height - 60, 0])

var yZoom = d3.scaleLinear().range([height - 60, 0])

//成交量的X、Y軸範圍
var xVolume = d3.scaleBand().range([0, width]).padding(0.15);
var yVolume = d3.scaleLinear().range([height, height - 60]);

//K棒計算
var candlestick = techan.plot.candlestick().xScale(x).yScale(y);

//成交量計算
var volume = techan.plot.volume().accessor(candlestick.accessor()).xScale(x).yScale(yVolume);

//SMA計算
var sma = techan.plot.sma().xScale(x).yScale(y);

//XY軸顯示的位置
var xAxis = d3.axisBottom(x)
var yAxis = d3.axisLeft(y)
var YzoomAxis = d3.axisLeft(yZoom);
////詳細數值顯示在右側
var yRightAxis = d3.axisRight(yZoom);
var VyRightAxis = d3.axisRight(yVolume);


//十字線右側顯示的文字
var ohlcRightAnnotation = techan.plot.axisannotation()
    .axis(yRightAxis)
    .orient('right')
    .height(25)
    .translate([width, 0])
//十字線右側顯示成交量的文字
var VohlcRightAnnotation = techan.plot.axisannotation()
    .axis(VyRightAxis)
    .orient('right')    //顯示在軸的右側
    .height(25)
    .translate([width, 0])
    .format(d3.format('.1s'));
//十字線下方要顯示的日期文字
var timeAnnotation = techan.plot.axisannotation()
    .axis(xAxis)
    .orient('bottom')       //顯示在軸的下方
    .format(d3.timeFormat('%Y-%m-%d'))
    .width(80)
    .height(25)
    .translate([0, height]);

//十字線
var crosshair = techan.plot.crosshair()
    .xScale(x)
    .yScale(allY)
    .xAnnotation(timeAnnotation)
    .yAnnotation([ohlcRightAnnotation, VohlcRightAnnotation])
    .on("enter", enter)
    .on("out", out)
    .on("move", move);

//設定K棒生成的layer
var svg = d3.select("#forKcandle").append("svg")
    .attr("width", width + margin.left + margin.right)
    .attr("height", height + margin.top + margin.bottom)
    .append("g")
    .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

//放大縮小的設定
var zoom = d3.zoom()
    .scaleExtent([1, 5])    //大小範圍
    .translateExtent([[0, 0], [width, height]])
    .extent([[0, 0], [width, height]])   //縮放的區塊
    .on("zoom", zooming);

var zoomableInit;

// ------MACD------ //

var macdX = techan.scale.financetime()
    .range([0, width]);

var macdY = d3.scaleLinear()
    .range([height, 0]);

var macd = techan.plot.macd()
    .xScale(macdX)
    .yScale(macdY);

var xAxisMacd = d3.axisBottom(macdX);
var yAxisMacd = d3.axisLeft(macdY);

var svgMACD = d3.select("#forIndicators").append("svg")
    .attr("width", width + margin.left + margin.right)
    .attr("height", height + margin.top + margin.bottom)
    .append("g")
    .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

//十字線右側要顯示的文字
var ohlcRightAnnotationMACD = techan.plot.axisannotation()
    .axis(yAxisMacd)
    .orient('right')
    .height(25)
    .translate([width, 0])
    .format(d3.format('2,.2f'));

////十字線下方要顯示的日期文字
var timeAnnotationMACD = techan.plot.axisannotation()
    .axis(xAxisMacd)
    .orient('bottom')
    .format(d3.timeFormat('%Y-%m-%d'))
    .width(70)
    .height(25)
    .translate([0, height]);

var crosshairMACD = techan.plot.crosshair()
    .xScale(macdX)
    .yScale(macdY)
    .xAnnotation(timeAnnotationMACD)
    .yAnnotation(ohlcRightAnnotationMACD)
    .on("enter", enter)
    .on("out", out)
    .on("move", move);


// ------KD------ //
var yScaleKD = d3.scaleLinear()
    .range([height + margin.top + margin.bottom, 0]);


//---抓資料---//
var dataAll;

d3.json(`/Home/showStocks/${stocksID}`, function (data) {
    $(".td-tittle").text(data[0].StockName) //股票名稱
    data = data.map(function (d) {
        return {
            date: parseDate(d.Date),
            open: +d.Open,
            high: +d.High,
            low: +d.Low,
            close: +d.Close,
            volume: +d.Volume,
            sma05: +d.SMA05,
            k05: +d.K05,
            d05: +d.D05,
            spreadRatio: +d.SpreadRatio,
            tradeMoney: +d.TradeMoney,
            tradeQuantity: +d.TradeQuantity,
            eps: +d.EPS,
            bussinessIncome: +d.BussinessIncome,
            //nonBussinessIncome: +d.NonBussinessIncome,
            //nonBussinessIncomeRatio: +d.NonBussinessIncomeRatio,
            name: +d.StockName
        };
    })
    dataAll = data;
    defultStockInfo();  //預設股票資料
    //成交量的data
    var volumeData = data.map(function (d) {
        return {
            date: d.date,
            volume: d.volume
        }
    });



    //K棒X、Y軸
    svg.append("g").attr("class", "y axis");
    svg.append("g").attr("class", "y axis zoom");
    svg.append("g")
        .attr("class", "x axis")
        .attr("transform", "translate(0," + height + ")")

    //K棒區
    svg.append("g")
        .datum(data)    //綁定資料
        .attr("class", "candlestick")

    //成交量
    svg.append("g")
        .attr("class", "volume");

    //sma5、sm30
    svg.append("g")
        .datum(techan.indicator.sma().period(5)(data))
        .attr("class", "sma ma-5");
    svg.append("g")
        .datum(techan.indicator.sma().period(30)(data))
        .attr("class", "sma ma-30");


    // --- MACD --- //

    //MACD X軸
    svgMACD.append("g")
        .attr("class", "x axis macd")
        .attr("transform", "translate(0," + height + ")");

    //MACD Y軸
    svgMACD.append("g")
        .attr("class", "y axis macd")
        .append("text")
        .attr("transform", "rotate(-90)")
        .attr("y", 6)
        .attr("dy", ".71em")
        .style("text-anchor", "end")
        .text("MACD");

    svgMACD.append("g")
        .attr("class", "macd here"); //可能哪裡撞classㄌ，範圍要再小一點所以加了here

    //MACD十字線
    svgMACD.append('g')
        .attr("class", "crosshairMacd")

    draw(data, volumeData);       //畫K棒&成交量
    drawKD(data);                 //畫KD

});

//畫出圖表
function draw(data, volumeData) {
    //x的資料範圍
    x.domain(data.map(candlestick.accessor().d));
    //y的資料範圍
    y.domain(techan.scale.plot.ohlc(data, candlestick.accessor()).domain());
    yZoom.domain(techan.scale.plot.ohlc(data, candlestick.accessor()).domain());
    //成交量
    yVolume.domain(techan.scale.plot.volume(data).domain());
    xVolume.domain(data.map(function (d) { return d.date; }))
    //draw範圍
    var clip = svg.append("defs").append("svg:clipPath")
        .attr("id", "clip")
        .append("svg:rect")
        .attr("width", width)
        .attr("height", height)
        .attr("x", 0)
        .attr("y", 0);
    var candlestickClip = svg.append("defs").append("svg:clipPath")
        .attr("id", "candlestickClip")
        .append("svg:rect")
        .attr("width", width)
        .attr("height", height - 60)
        .attr("x", 0)
        .attr("y", 0);
    xVolume.range([0, width].map(d => d)); // 回到初始值
    //成交量
    var chart = svg.selectAll("volumeBar")
        .append("g")
        .data(volumeData)
        .enter().append("g")
        .attr("clip-path", "url(#clip)");

    chart.append("rect")
        .attr("class", "volumeBar")
        .attr("x", function (d) { return xVolume(d.date); })
        .attr("height", function (d) { return height - yVolume(d.volume); })
        .attr("y", function (d) { return yVolume(d.volume); })
        .attr("width", xVolume.bandwidth()) //find the width of each band
        .style("fill", function (d, i) { // 根據漲跌幅決定成交量的顏色
            if (i != 0 && (data[i].close - data[i - 1].close) > 0) { return "#b84121" } else if (i != 0 && (data[i].close - data[i - 1].close) < 0) {
                return "#9eb54c"
            } else {
                return "#DDDDDD"
            }
        });


    //ticks(幾個刻度?縮放比例?) timeFormat(什麼樣的數值) tickSize(表格內的格線)
    svg.select("g.candlestick").call(candlestick).attr("clip-path", "url(#candlestickClip)");  //K棒
    svg.selectAll("g.x.axis").call(xAxis.ticks(d3.timeMonth, 2).tickFormat(d3.timeFormat("%m/%d")).tickSize(-height, -height).tickPadding(10));   //X軸
    svg.selectAll("g.y.axis").call(yAxis.ticks(5).tickSize(-width, 0).tickPadding(10));   //Y軸
    svg.selectAll("g.y.axis.zoom").call(YzoomAxis.ticks(5).tickSize(-width, 0).tickPadding(10));   //Y軸
    svg.select("g.sma.ma-5").attr("clip-path", "url(#candlestickClip)").call(sma);
    svg.select("g.sma.ma-30").attr("clip-path", "url(#candlestickClip)").call(sma);

    //K棒十字線，丟到這裡生成，避免被成交量蓋掉
    svg.append('g').attr("class", "crosshair")
    //zoom的初始值
    zoomableInit = x.zoomable().clamp(false).copy();
    svg.select("g.crosshair").call(crosshair).call(zoom);  //十字線+呼叫zoom的功能
}

function drawMACD(data) {
    var macdData = techan.indicator.macd()(data);
    macdX.domain(macdData.map(macd.accessor().d));

    //套件的公式有問題，重新寫方法抓
    var sList = [], mLisr = [], dList = [];
    for (var i = 0; i < macdData.length; i++) {
        sList.push(macdData[i].signal);
        mLisr.push(macdData[i].macd);
        dList.push(macdData[i].difference);
    }
    var macdList = sList.concat(mLisr).concat(dList);   //合併
    macdY.domain([(d3.min(macdList) - 0.1), (d3.max(macdList) + 0.1)]);     //找signal、macd跟difference的最大最小值，避免某一方超過軸度

    svgMACD.selectAll("g.macd.here").datum(macdData).call(macd);
    svgMACD.selectAll("g.x.axis.macd").call(xAxisMacd.ticks(d3.timeMonth, 1).tickFormat(d3.timeFormat("%m/%d")).tickSize(-height, -height).tickPadding(10));
    svgMACD.selectAll("g.y.axis.macd").call(yAxisMacd.ticks(10).tickSize(-width, -width).tickPadding(10));
    svgMACD.select("g.crosshairMacd").call(crosshairMACD);  //十字線
}

function drawKD(data) {

    macdX.domain(data.map(macd.accessor().d));
    let minY = d3.min(data.map(function (d) { return d.k05 }));
    let maxY = d3.max(data.map(function (d) { return d.k05 }));
    macdY.domain([minY - 10, maxY + 10]);



    svgMACD.selectAll("g.x.axis.macd").call(xAxisMacd.ticks(5).tickFormat(d3.timeFormat("%m/%d")).tickSize(-height, -height).tickPadding(10));
    svgMACD.selectAll("g.y.axis.macd").call(yAxisMacd.ticks(10).tickSize(-width, -width).tickPadding(10));

    var lineK = d3.line()   //K線
        .x(d => macdX(d.date))
        .y(d => macdY(d.k05))

    var lineD = d3.line()   //D線
        .x(d => macdX(d.date))
        .y(d => macdY(d.d05))

    svgMACD.selectAll("g.macd.here").append("path")
        .attr("class", "pathK")
        .attr('d', lineK(data))

    svgMACD.selectAll("g.macd.here").append("path")
        .attr("class", "pathD")
        .attr('d', lineD(data))

    svgMACD.select("g.crosshairMacd").call(crosshairMACD);  //十字線
}

function enter() {
    $(".td-date").css('display', 'inline')
}

function out() {
    //$(".tb-content").text("-");
    //$(".td-date").css('display', 'none')
    defultStockInfo();
}

function defultStockInfo(){
    //預設資料顯示
    $(".tb-open").text(dataAll[dataAll.length - 1].open);
    $(".tb-close").text(dataAll[dataAll.length - 1].close);
    $(".tb-SR").text(d3.format(".2f")(dataAll[dataAll.length - 1].spreadRatio) + "%");
    $(".tb-max").text(dataAll[dataAll.length - 1].high);
    $(".tb-min").text(dataAll[dataAll.length - 1].low);
    $(".tb-TM").text(d3.format(",")(dataAll[dataAll.length - 1].tradeMoney));
    $(".tb-TQ").text(d3.format(",")(dataAll[dataAll.length - 1].tradeQuantity));
    $(".tb-EPS").text(d3.format(".2f")(dataAll[dataAll.length - 1].eps));
    $(".tb-BI").text(d3.format(",")(dataAll[dataAll.length - 1].bussinessIncome));
    $(".td-date").text(d3.timeFormat('%Y-%m-%d')(dataAll[dataAll.length - 1].date));
    //

}

function move(coords) {
    for (let i = 0; i < dataAll.length; i++) {
        //如果十字線對齊的日期=資料內的某日
        if (coords.x === dataAll[i].date) {
            $(".tb-open").text(dataAll[i].open);
            $(".tb-close").text(dataAll[i].close);
            $(".tb-SR").text(d3.format(".2f")(dataAll[i].spreadRatio)+"%");
            $(".tb-max").text(dataAll[i].high);
            $(".tb-min").text(dataAll[i].low);
            $(".tb-TM").text(d3.format(",")(dataAll[i].tradeMoney));
            $(".tb-TQ").text(d3.format(",")(dataAll[i].tradeQuantity));
            $(".tb-EPS").text(d3.format(".2f")(dataAll[i].eps));
            $(".tb-BI").text(d3.format(",")(dataAll[i].bussinessIncome));
            $(".td-date").text(d3.timeFormat('%Y-%m-%d')(dataAll[i].date));
        }
    }
}

//處理單位，但可能用不到囉
function turnM(x) {
    return (x / 1000).toFixed(0) + "K";
}

var SIchange = false;
$("#change").on("click", function () {
    svgMACD.select("g.macd.here").remove();
    svgMACD.select("g.crosshairMacd").remove();
    svgMACD.append("g").attr("class", "macd here");
    svgMACD.append('g').attr("class", "crosshairMacd")
    if (!SIchange) {
        drawMACD(dataAll);
        this.innerText = "KD"
        SIchange = true;
    } else {
        drawKD(dataAll);
        this.innerText = "MACD"
        SIchange = false;
    }
})

function smaClick(btn) {
    switch (btn.innerText) {
        case "SMA5":
            $(".ma-5").toggleClass("hideElem");
            break;

        case "SMA30":
            $(".ma-30").toggleClass("hideElem");
            break;
    }
}

//撈預測區資料
d3.json(`/Home/showAllStocks/${stocksID}`, function (Alldata) {

    var dateList = []; //把所有日期資料整理成陣列
    for (let i = 0; i < Alldata.length; i++) {
        dateList.push(Alldata[i].Date);
    }
    d3.json(`/Home/showPredictions/${stocksID}`, function (Ddata) {
        // -----------* 先只找第1筆 記得改成Ddata.length*----------- //
        //最新的資料在最前
        for (let i = 0; i < Ddata.length; i++) {
            var PID = Ddata[i].PID; //卡片ID
            var xData = [Ddata[i].FinishTime] //預測日&前推五日的大禮包
            var yData = [Ddata[i].Label];
            var preDate = xData[0]; //預測日 懶人傳值
            var preBuildDate = Ddata[i].BuildTime;

            //console.log("預測日: "+ xData)
            //用BuildDate去找列表中最近的日子(closestDate)，closestDate可能等於BuildDate，也可能是前一天
            var closestDate = dateList.reduce((prev, curr) => {
                var prevDiff = Math.abs(new Date(preBuildDate) - new Date(prev));
                var currDiff = Math.abs(new Date(preBuildDate) - new Date(curr));

                return currDiff < prevDiff ? curr : prev;
            })
            /*            console.log("最近近的日子: " + closestDate);*/

            var Today = (new Date()).toISOString().split('T')[0];
            var preState;   //判斷結案狀態
            if (Today >= preDate) {
                preState = "已結案"

            } else {
                preState = "追蹤中"
            }

            //找最接近的日子，並往回推5次紀錄
            for (var j = 0; j < Alldata.length; j++) {
                if (Alldata[j].Date == closestDate) {
                    //最近的那天是建立那天嗎？（根據那張卡片對於使用者是當天or之前建立的）
                    var endDate = j - (closestDate == preBuildDate ? 1 : 0);
                    for (var x = 0; x < 5; x++) {
                        xData.unshift((Alldata[endDate - x]).Date);
                        yData.unshift((Alldata[endDate - x]).Close);
                    }
                    break;
                }
            }
            //
            var preData = [];
            for (var z = 0; z < xData.length; z++) {
                preData[z] = { Date: parseDate(xData[z]), Close: yData[z] };
            }
            var index = i;

            //整理字串
            var preVariable = JSON.parse(Ddata[i].Variable);    //所選變數
            var preDummy = JSON.parse(Ddata[i].Dummyblock);     //結果參數
            var pAccount = Ddata[i].Account;

            drawPre(preData, index, preState, preDate, PID, preBuildDate, preVariable, preDummy, pAccount);
        }
    })
})

function drawPre(myData, index, preState, preDate, PID, preBuildDate, preVariable, preDummy, pAccount) {

    
    //console.log(preDummy);

    var prelist = "";
    if (preVariable[0] != null) {
        prelist += `<button class="copyAll" id="card${PID}" onclick="copyAllList(this)">複製全部</button>`;
        for (var i = 0; i < preVariable.length; i++) {
            prelist += `<button class="pre-list-btn" onclick="copyList(this)">${preVariable[i]}</button>`
        };
    };
    /*console.log(prelist);*/
    $(".predictionArea").prepend(`<label class='prediction-card ${index}'>
    <input type='checkbox' class='card-btn' />
    <div class='card-content'><div class='card-front'>
    <div class="card-o">
        <p class="pre-state">${preState}</p>
        <table >
            <tr><th class="pre-th">準確率</th><td class="pre-td pre-date">-</td></tr>
            <tr><th class="pre-th">建立帳號</th><td class="pre-td pre-date">${pAccount}</td></tr>
            <tr><th class="pre-th">預測模型</th><td class="pre-td pre-date">-</td></tr>
        </table>
    </div>
    <p class="pre-state">${preState}</p>
    <table class="table-inside">
    <tr><th class="pre-th">建立日期</th><td class="pre-td pre-date">${preBuildDate}</td></tr>
    <tr><th class="pre-th">預測日期</th><td class="pre-td pre-date">${preDate}</td></tr>
    <tr><th class="pre-th">預測價格</th><td class ="pre-td">${myData[5].Close}</td></tr>
    <tr><th class="pre-th">結果參數</th><td class="pre-td">MSE:${preDummy.MSE}</td></tr>
    </table>
    <button class='prediction-collect' id="PID${PID}" onclick="btnTest(this)">♥</button>
    </div>
    <div class='card-back'>
    <div class='forPrediction'>
    </div></div></div>
    <div class="preVar">${prelist}</div>
    </label>`);
    //重新整理日期
    var dateList = [];
    for (var i = 0; i < myData.length; i++) {
        dateList.push(myData[i].Date);
    }

    //圖表大小的設置
    var preMargin = { top: 20, right: 50, bottom: 30, left: 50 },
        preWidth = 325 - preMargin.left - preMargin.right,
        preHeight = 200 - preMargin.top - preMargin.bottom;
    
    //X、Y軸scale
    var preScaleX = d3.scaleBand().range([10, preWidth]).domain(dateList);
    var preScaleY = d3.scaleLinear().range([preHeight - 35, 25]).domain(d3.extent(myData, d => d.Close));

    //X、Y軸
    var preAxisX = d3.axisBottom(preScaleX)
    var preAxisY = d3.axisLeft(preScaleY)

    //生成線
    var linePre = d3.line()
        .x(d => preScaleX(d.Date))
        .y(d => preScaleY(d.Close));
    /*.curve(d3.curveBasis);*/  //讓折線有弧度

    //生SVG
    var preSvg = d3.select(".forPrediction").append("svg")
        .attr("class", "forPre")
        .attr("width", preWidth + preMargin.left + preMargin.right)
        .attr("height", preHeight + preMargin.top + preMargin.bottom)
        .append("g")
        .attr("transform", "translate(" + preMargin.left + "," + preMargin.top + ")");

    //顏色
    var maxDPre = d3.max(myData, function (d) { return +d.Date; });
    //設定Y軸刻度
    var maxCPre = d3.max(myData, function (d) { return +d.Close; });
    var minCPre = d3.min(myData, function (d) { return +d.Close; });


    var colorX = d3.scaleLinear()
        .domain([0, maxDPre])
        .range([preWidth, 0]);

    preSvg.append("linearGradient")
        .attr("id", "line-gradient")
        .attr("gradientUnits", "userSpaceOnUse")
        .attr("x1", colorX(0))
        .attr("y1", 0)
        .attr("x2", colorX(maxDPre))
        .attr("y2", 0)
        .selectAll("stop")
        .data([
            { offset: "0%", color: "#51a1b7" },
            { offset: "100%", color: "#cedba0" }
        ])
        .enter().append("stop")
        .attr("offset", function (d) { return d.offset; })
        .attr("stop-color", function (d) { return d.color; });

    
    preSvg.append("g").attr("class", "x axis pre").attr("transform", "translate(0," + preHeight + ")");     //X軸
    preSvg.append("g").attr("class", "y axis pre");     //Y軸
    preSvg.append("g").datum(myData).attr("class", "predictionLine here").attr("transform", "translate(18,0)");   //折線

    preSvg.select("g.predictionLine.here").append("path").attr("class", "pathPre").attr("d", linePre(myData)).attr("fill", "none")
        .attr("stroke", "#bbbdbe").attr("stroke-width", 2);
    preSvg.select("g.x.axis.pre").call(preAxisX.tickValues(dateList).tickFormat(d3.timeFormat("%m/%d")).tickPadding(10).tickSizeInner(-preHeight - 10, -preHeight))
        .selectAll("text")
        .style("text-anchor", "end")
        .attr("dx", "-.8em")
        .attr("dy", ".15em")
        .attr("transform", "rotate(-25) translate(20, 10)");

    preSvg.select("g.y.axis.pre").call(preAxisY.tickSizeInner(-preWidth - 10, -preWidth).tickPadding(10).tickFormat(d3.format(".1f")).tickValues([minCPre-10, d3.mean([minCPre, maxCPre]), maxCPre+10]));

    //--點點--//
    //提示框
    var Tooltip = d3.select(".forPrediction")
        .append("div")
        .style("opacity", 0)
        .attr("class", "tooltip")
        .style("background-color", "white")
        .style("border", "solid")
        .style("border-width", "2px")
        .style("border-radius", "5px")
        .style("padding", "5px")
        .style("width", "100px")
        .style("height", "33px")
        .style("text-align", "center")


    var mouseover = function (d) {
        Tooltip.style("opacity", 1)
    }
    var mousemove = function (d) {
        Tooltip.html("價格: " + d.Close)
            .style("left", (d3.mouse(this)[0] + 20) + "px")
            .style("top", (d3.mouse(this)[1]) + "px")
    }
    //console.log($(".circleGroup"));
    var mouseleave = function (d) {
        Tooltip.style("opacity", 0)
    }
    //加圓點
    preSvg.append("g")
        .attr("class", `circleGroup${index}`)
        .attr("transform", "translate(18,0)")
        .selectAll("dot")
        .data(myData)
        .enter()
        .append("circle")
        .attr("class", "myCircle")
        .attr("cx", d => preScaleX(d.Date))
        .attr("cy", d => preScaleY(d.Close))
        .attr("r", 5)
        .attr("stroke", "#cedba0")
        .attr("fill", "#cedba0")
        .on("mouseover", mouseover)
        .on("mousemove", mousemove)
        .on("mouseleave", mouseleave);

    d3.selectAll(".prediction-card")
        .each(function () {
            var lastTick = d3.select(this).selectAll(".x.axis.pre .tick:last-child");
            lastTick.select("line").style("stroke-dasharray", "6,6");
        });

    //動畫?
    pointAni(index);
    function pointAni(index) {
        var strokeC = (myData[5].Close > myData[4].Close) ? "#b84121" : "#69b3a2";
        var fillC = preState == "已結案" ? strokeC : "white"
        //var fillC = (myData[5].Close > myData[4].Close) ? "#f77465" : "#cedba0";
    /*        console.log("strokeC:" + strokeC + "  fillC:" + fillC);*/

        d3.select(`.circleGroup${index} :last-child`)
            .attr("stroke", strokeC)
            .style("stroke-width", 2)
            //.style("stroke-opacity", 1)
            .style("r", 5)

        d3.select(`.circleGroup${index} :last-child`)
            .attr("fill", fillC)
            //.transition()
            //.duration(1000)
            //.style("stroke-width", 10)
            //.style("stroke-opacity", 0)
            //.style("r", 7)
            //.on("end", function () { pointAni(index) });
    }
}

var rescaledX, rescaledY;
var t;
function zooming() {

    var t = d3.event.transform;

    rescaledX = d3.event.transform.rescaleY(x);
    rescaledY = d3.event.transform.rescaleY(y);
    //Y座標
    yAxis.scale(rescaledY);
    candlestick.yScale(rescaledY);
    sma.yScale(rescaledY);


    //X座標
    x.zoomable().domain(d3.event.transform.rescaleX(zoomableInit).domain());

    yZoom.range([height - 60, 0].map(d => d3.event.transform.applyY(d)));
    xVolume.range([0, width].map(d => d3.event.transform.applyX(d)));
    redraw();
}

function redraw() {
    svg.select("g.candlestick").call(candlestick);  //K棒
    svg.selectAll("g.x.axis").call(xAxis.ticks(d3.timeMonth, 2).tickFormat(d3.timeFormat("%m/%d")).tickSize(-height, -height).tickPadding(10));   //X軸
    svg.selectAll("g.y.axis").call(yAxis.ticks(5).tickSize(-width, 0).tickPadding(10));   //Y軸
    svg.select("g.sma.ma-5").call(sma);
    svg.select("g.sma.ma-30").call(sma);
    svg.selectAll("rect.volumeBar")
        .attr("x", function (d) { return xVolume(d.date); })
        .attr("width", (xVolume.bandwidth()));
}




//如果有登入 需要改變的部分
//user = "apple5678";       /////////先寫死是apple
//logging = true;     /////////先寫死是true
function btnTest(btn) {
    if (logging) {      //如果有登入
        //到時候user要改成抓目前登入者的帳號ㄛ
        var dataToServer = { user: user, cardID: $(btn).attr("id").substring(3) };
        $.ajax({
            url: "/Home/CheckCard",
            method: "POST",
            data: dataToServer,
            success: function (e) {
                console.log(e);
                switch (e.substring(0,1)) {
                    case "A": {
                        $(btn).css("color", "red");
                        console.log("新增一筆");
                        sessionStorage.setItem("LogMemberfavoriteModel", e.substring(1));
                        break
                    }
                    case "D": {
                        $(btn).css("color", "black");
                        console.log("刪除一筆");
                        sessionStorage.setItem("LogMemberfavoriteModel", e.substring(1));
                        break;
                    }
                    case "R":
                        console.log("收藏上限了朋友");
                        break;
                }
            }
        })
    } else {    
        //---沒登入的話---//
    }
    
}
setTimeout(function () {    //要抓剛appen上去的元素，所以設timeout
    if (MID == null) { //這邊要判斷是否有登入
        //沒登入時按鈕
        $(".prediction-collect").on({
            mouseenter: function () {
                $(this).removeClass("collectBtnStart");
                $(this).removeClass("collectBtnLeave");
                //先清掉先前的class
                $(this).addClass("collectBtnStart");
            },
            mouseleave: function () {
                $(this).addClass("collectBtnLeave");
            }
        })
    } else {
        d3.json(`/Home/cardCheck/${user}`, function (list) {
            list.forEach(function (item, i) {
                //針對會員有按愛心的按鈕 變化
                $(`#PID${parseInt(item)}`).css({
                    color: "red",
                    /* fontSize: "32px" */
                });
            })
        });


    }//else尾


}, 500);




function copyList(obj) {
    var myList = $(".var-tag .var-tag-a");
    var copyAlready = false;
    if (myList[0] != null) {
        for (var i = 0; i < myList.length; i++) {
            if (myList[i].innerText == $(obj).text()) {
                copyAlready = true;
                $(obj).addClass("pre-list-btn-click-again");
                $(obj).addClass("pre-list-btn-click");
                setTimeout(function () {
                    $(obj).removeClass("pre-list-btn-click")
                }, 1000);
                break;
            }
        }
    }
    if (!copyAlready) {
        $(".var-tag").append(`<div class="var-tag-a" id="${$(obj).text()}" onClick="tagClick(this)">${$(obj).text()}</div>`);
        saveTag();
        $(obj).removeClass("pre-list-btn-click-again");
        $(obj).addClass("pre-list-btn-click");
        setTimeout(function () {
            $(obj).removeClass("pre-list-btn-click")
            $(obj).addClass("pre-list-btn-click-again");
        }, 1000);
       
    }

    //console.log($(obj).text());
    //
    var content = $(obj).text();
    navigator.clipboard.writeText(content);
}

function copyAllList(obj) {
    $(obj).addClass("copyAllCopied");
    setTimeout(function () {
        $(obj).removeClass("copyAllCopied");
    }, 1000)
    var thisCard = $(obj).attr("id");
    var thisCardList = $(`#${thisCard} ~ .pre-list-btn`);

 


    var content="";
    for (var i = 0; i < thisCardList.length; i++) {
        if (i != 0) content += ",";
        content += thisCardList[i].innerText;
    }
    navigator.clipboard.writeText(content);
        

}

setTimeout(function () {
    $(".pre-list-btn").on({
        mouseenter: function () {
            $(this).removeClass("pre-list-btn-he");
            $(this).removeClass("pre-list-btn-hl");
            $(this).addClass("pre-list-btn-he");
        },
        mouseleave: function () {
            $(this).addClass("pre-list-btn-hl");
        }
    })
}, 500);

var tagforSession = "";
function saveTag() {
    tagforSession = "";
    var myList = $(".var-tag .var-tag-a");
    console.log(myList);
    
    if (myList[0] != null) {
        for (var i = 0; i < myList.length; i++) {
            if (i != 0) tagforSession += ",";
            tagforSession += myList[i].innerText;
        }
    }
    sessionStorage.setItem("VarTag", tagforSession);
}

$(".bg-right-hand")
$(".bg-left-hand")