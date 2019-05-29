$(document).ready(function () {
    $(window).resize(function () {
        if ($(this).width() >= 992) {
            $(".product-wrapper").removeClass("col-md-12").addClass("col-md-9")
        }
        else {
            $(".product-wrapper").removeClass("col-md-9").addClass("col-md-12")
        }
    })

    //Filter dropdown
    var filter_buttons = document.querySelectorAll(".filter-button");
    for (var filter_button of filter_buttons) {
        filter_button.addEventListener("click", function () {

            if (this.classList.contains("active")) {
                this.classList.remove("active");
                this.nextElementSibling.classList.remove("active");
            }
            else {
                this.classList.add("active");
                this.nextElementSibling.classList.add("active");
            }

        })
    }
    //End filter dropdown
    //hamburger menu
    var hamb = document.querySelector(".hamburger-menu");
    hamb.addEventListener("click", function () {
        var nav_mobile = document.querySelector(".navbar-mobile");
        if (nav_mobile.classList.contains("active")) {
            nav_mobile.classList.remove("active");
        }
        else {
            nav_mobile.classList.add("active");
        }
    })
    $("#search").keyup(function () {
        
        $(".search-result-wrapper").addClass("active");
     
        var langId = $("#languages").val();
        var value = $(this).val();
        var formData = new FormData();
        formData.append("Description", value);
        formData.append("LangId", langId);
        var _url = "/Catalogue/GetCategories"
        SearchAjax(formData, _url, langId);
    })
    async function SearchAjax(formData,_url,langId) {
        await $.ajax( {
            url: _url,
            data: formData,
            dataType: "json",
            type: "post",
            cache: false,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.status == 200) {
                    $(".search-result-wrapper").empty();
                    for (var element of response.data) {
                        var a = document.createElement("a");
                        a.setAttribute("href", `/Catalogue/Search/?desc=${element.description}&lang=${langId}`);
                        a.innerText = element.description;
                        $(".search-result-wrapper").append(a);
                    }
                }
            }
        })
    }

    async function RemoveElement(element) {
        await element.empty();
    }

    $(document).click(function (e) {
        var target = e.target;
        if (target.getAttribute("id") != "search" || !target.classList.contains(".search-result-wrapper")) {
            $(".search-result-wrapper").removeClass("active");
        }
    })


    $(".search-form").submit(false);

    function FillText(data,callback) {
        for (var d of data) {
            var text = callback(d)
           return text
        }
    }

})

