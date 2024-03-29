﻿$(".bg-right-hand").addClass("bg-right-hand-move");
$(".bg-left-hand").addClass("bg-left-hand-move");
setTimeout(function () {
    $(".bg-water-blue").addClass("bg-water-show");
}, 1000);
$("#search").on("keydown", function (e) {
    if (e.which == 13) changePage();
});
function changePage() {
    //if (!refreshStockDB($("#search").val())) {
    //    return false;
    //} else {

    //}
    async function jget(url, stockId) {
        // Note: Below variable will hold the value of the resolved promise
        let response = await fetchingData(url, stockId);
        return response;
    }
    $.ajax({
        url: `/Home/checkStocks/${$("#search").val()}`,
        method: "post"
    }).done(function (ans) {

        if (ans == "wrongCode") {
            $(".search-hint").css("display", "block");
        } else {
            jget("/StockModel/UpdateOneStock", ans).then(function (e) {

                document.location.href = `/Home/StockCharts/${ans}`;
            });
        } 
    })
}

$.widget("custom.catcomplete", $.ui.autocomplete, {
    _create: function () {
        this._super();
        this.widget().menu("option", "items", "> :not(.ui-autocomplete-category)");
    },
    _renderMenu: function (ul, items) {
        var that = this,
            currentCategory = "";
        $.each(items, function (index, item) {
            var li;
            if (item.category != currentCategory) {
                ul.append("<li class='ui-autocomplete-category'>" + item.category + "</li>");   //標籤
                currentCategory = item.category;
            }
            li = that._renderItemData(ul, item);
            if (item.category) {
                li.attr("aria-label", item.category + " : " + item.label);  //內容
            }
        });
    }
});

$.getJSON("/Home/stocksListAC", function (myData) {
    $("#search").catcomplete({
        delay: 0,
        source: myData
    });
})
$.getJSON("/stockmodel/stocksListACA", function (myData) {
    $("#stockid").catcomplete({
        delay: 0,
        source: myData
    });
})

//kazuo新增 for 首頁search 呼叫API用
function fetchingData(url, stockId) {
    return new Promise((resolve, reject) => {
        $.get(url, { stockCode: stockId }, html => {
            resolve(html);
            console.log(html);
        })
    })
}
//function refreshStockDB(string stockid) {

//    async function jget(url, stockId) {
//        // Note: Below variable will hold the value of the resolved promise
//        let response = await fetchingData(url, stockId);
//        return response;
//    }
//    var rr = jget("/StockModel/UpdateOneStock", stockId);
//    var result = true;
//    rr.then(function (e) {
//        if (e.stockname == "查無這支股票") {
//            alert("請選擇其他股票");
//            result = false;
//        } else if (e.stockname != null) {
//            result = true;
//        } else {
//            alert("請選擇其他股票");
//            result = false;
//        }
//        console.log(rr);
//        return result;
//    })

//}
//function changePage() {
//    async function jget(url, stockId) {
//        // Note: Below variable will hold the value of the resolved promise
//        let response = await fetchingData(url, stockId);
//        return response;
//    }
//    var stockId = ($("#search").val().split(' ').length > 1) ? $("#search").val().split(' ')[1] : $("#search").val();
//    var rr = jget("/StockModel/UpdateOneStock", stockId);
//    var result = true;
//    rr.then(function (e) {
//        if (e.stockname == "查無這支股票") {
//            alert("請選擇其他股票");
//            //$(".search-hint").css("display", "block");
//            result = false;
//        } else if (e.stockname != null) {
//            $.ajax({
//                url: `/Home/checkStocks/${$("#search").val()}`,
//                method: "post"
//            }).done(function (ans) {
//                document.location.href = `/Home/StockCharts/${ans}`;
//            })
//        } else {
//            alert("請選擇其他股票");
//            result = false;
//        }
//        console.log(rr);
//        return result;
//    })
//};

//叫loading動畫
var animationPath = '@Url.Content("~/animations/animation.json")';
var animation = lottie.loadAnimation({
    container: document.getElementById("#loading-ani div"), //要動畫的地方
    renderer: "svg",
    loop: false,
    autoplay: false,
    path: "Animation.json", //載下來的動畫json黨
});