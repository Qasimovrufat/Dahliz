document.addEventListener("DOMContentLoaded", function () {
    //Product Add
    $(".product_photo").change(function () {
        var fileReader = new FileReader();
        var id = "#";
        var langId = $(".select-language").val();
        id += langId;
        var input = document.querySelector(id + " .product_photo");
        fileReader.readAsDataURL(input.files[0]);
        fileReader.onload = function () {
            var src = fileReader.result;
            document.querySelector(id+" .photo_view").setAttribute("src", src);
            document.querySelector(id+" .photo_view").style.width = "150px";
            document.querySelector(id+" .photo_view").style.height = "150px";
        }
    })

    $(".add-button").click(function (e) {
        e.preventDefault();
        var id = "#";
        var langId = $(".select-language").val();
        id += langId;

        var realpartno = $(id+" .input-tags").val();

        var value = [];
        var newRealPart ="";
        for (var i = 0; i < realpartno.length; i++) {

            if (realpartno[i] != ",") {
                newRealPart += realpartno[i];
            }
            else {
                value.push(newRealPart);
                newRealPart = "";
            }
            if (i == realpartno.length - 1) {
                value.push(newRealPart);
            }
        }
        var formData = new FormData();
        formData.append("Description", $(id + " .description").val());
        formData.append("SubCategory", $(id + " .subcategory").val());
        formData.append("RealPartNos", value);
        formData.append("Categories", $(id +" .category").val());
        formData.append("Photo", $(id + " .product_photo").get(0).files[0]);
        formData.append("LanguageId", $(".select-language").val());

        $(id + " .error_view").empty();
        $(id + " .error_view").text("Zehmet Olmasa Gozleyin Sorgu Icra Edilir ...")
        $.ajax({
            url: "/Admin/Home/Add/",
            type: "post",
            dataType: "json",
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.status == 200) {
                    swal({
                        title:"Success",
                        icon: "success",
                        text: response.message,
                        buttons: ["Siyahiya Kec", "Yeni Mehsul Elave Et"]
                    });
                    ClearArea("#description");
                    ClearArea("#product_photo");
                    ClearElement(".input-tags .item");

                    var a = document.createElement("a");
                    a.href = "/Admin/Home/Index";
                    a.innerText = "Siyahiya Qayit";
                    a.className = "btn btn-success my-2";
                    a.style.color = "#fff";
                    $(id + " .error_view").empty();
                    $(id + " .error_view").append(a);
                } else {
                    swal({
                        title: "Error",
                        icon: "error",
                        text: response.errorMessage
                    });
                }
            }
        });
    })
    $(".delete-link").click(function (e) {
        localStorage.id = $(this).data("id");
    })

    $(".delete-product").click(function () {
        var id = parseInt(localStorage.id);

        $.ajax({
            url: "/Admin/Home/Delete/" + id,
            type: "post",
            dataType: "json",
            success: function (response) {
                if (response.status == 200) {
                    swal({
                        html: true,
                        title: "Melumat",
                        icon: "success",
                        text: "Mehsul Silindi <a href='/Admin/Home/Index' class='btn btn-success'>Ana Sehifeye Kecin</a>",
                    })
                    localStorage.clear();
                }
                else {
                     swal({
                        title: "Melumat",
                        icon: "error",
                        text: "Xeta Bas Verdi",
                    })
                }
            }
        })
    })


    $("#edit-button").click(function () {
        var value = $(".input-tags").val();
        var id = $("#product").data("id");
        var langId = $("#product").data("lang");
        var formData = new FormData();
        formData.append("array", value);
        formData.append("id", id);
        formData.append("langId", langId)
        $.ajax({
            url: "/Admin/Home/EditRealPartNo",
            data: formData,
            dataType: "json",
            type: "post",
            cache: false,
            contentType: false,
            processData: false,
            success: function (response) {

            }
        })
    })

    $("#search").click(function () {
        console.log("test");
    })




    function ClearArea(area) {
        $(area).val("");
    }
    function ClearElement(element) {
        $(element).remove();
    }
})