$("#tttest").on
$("#search").on("keydown", function (e) {
    if (e.which == 13) changePage();
});
function changePage() {
    $.ajax({
        url: `/Home/checkStocks/${$("#search").val()}`,
        method: "post"
    }).done(function (ans) {
        
        if (ans == "wrongCode") {
            $(".search-hint").css("display", "block");
        } else {

            document.location.href = `/Home/StockCharts/${ans}`;
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
//按鈕網頁跳轉
$(".btn-m").on("click", function () {   //會員專區按鈕
    
})

$(".btn-t").on("click", function () {   //訪客體驗按鈕
    window.location.href = "/Home/Visitor"
    console.log("wwf");
})


