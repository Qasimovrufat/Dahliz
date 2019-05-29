﻿$(document).ready(function () {

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << Functions Executed on Window load >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
    $(function () {
        $(window).on("load", function () {
            var width = $(window).width();
            var height = $(window).height();

            adjustBodyHeight();
            adjustSelectWidth();

        });
    });

    //~~~~~~~~~~~~~~~~~~~~  << End of Window load >>  ~~~~~~~~~~~~~~~~~~~~//


    //Setting interval
    var n = 0;
    function recall() {
        adjustBodyHeight();
        adjustSelectWidth();
        n++;
    }
    var interval = setInterval(recall, 200);

    if (n >= 20) {
        clearInterval(interval);
    }


    //Plugins

    //$("#createAccidentVehicle .InputItemDate").val(tempDate); 


    $(".datepicker").datepicker({
        //defaultDate: tempDate,
        format: 'dd.mm.yyyy',
        todayHighlight: true,
        calendarWeeks: true,
        clearBtn: true,
        updateViewDate: false,
        weekStart: 1
    });

    $(".dateTimePicker").datetimepicker({
        format: 'dd.mm.yyyy hh:ii',
        startDate: "2000-01-01 00:00",
        autoclose: true,
        todayBtn: true
    });


    $('.timePicker').timepicker({
        timeFormat: 'HH:mm',
        interval: 10,
        dynamic: false,
        dropdown: true,
        scrollbar: true,
        defaultTime: false
    });

    $('.timePickerSOCAR').timepicker({
        timeFormat: 'HH:mm',
        interval: 5,
        dynamic: false,
        dropdown: true,
        scrollbar: true,
        defaultTime: false
    });


    //$(".datepicker").datepicker('clearDates');


    $('.selectpicker').selectpicker();

    $('.select2-multiple-class').select2();


    $(".formTest").scroll(function () {
        $(window).trigger('scroll');
    });

    //Update Password
    $(".updatePassword").click(function () {
        $("#profile .passwordGroup .currentPass").slideUp(900);
        $("#profile .passwordGroup .oldPass").slideDown(900);
        $("#profile .passwordGroup .newPass").slideDown(900);
        $("#profile .passwordGroup .newPassRepeat").slideDown(900);
        $(this).slideUp(900);
    });

    $(".updatePhone").click(function () {
        $("#profile .phoneGroup .inputItem").removeAttr('disabled');
        $(this).slideUp(900);
    });

    //Height of body
    //Function adjust height of body
    function adjustBodyHeight() {
        var windowWidth = $(window).width();
        var windowHeight = $(window).height();
        var header = $("#header").offset();
        var headerHeight = $("#header").height();
        var footerHeight = $("#footer").height();
        var bodyTop = headerHeight - 4;

        var bodyMarginBottom = footerHeight - 5;
        var bodyHeight = windowHeight - bodyTop - bodyMarginBottom;
        $("#body").css({
            "minHeight": bodyHeight + "px",
            "top": bodyTop + "px",
            "marginBottom": bodyMarginBottom + "px"
        });
    }

    //Width of select box
    //Function adjust Width of select box
    function adjustSelectWidth() {
        var inputWidth = $("#body form .form-group>div>div").outerWidth();

        $("#body form .form-group>div>div>button").css({
            "width": inputWidth + "px"
        });
    }

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << Functions Executed on Window scroll >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//

    var shiftScrollNav = 0;
    $(window).scroll(function () {


    })
    //~~~~~~~~~~~~~~~~~~~~  << End of Window scroll >>  ~~~~~~~~~~~~~~~~~~~~//


    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << Functions Executed on Window resize >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
    $(window).resize(function () {
        adjustBodyHeight();
        adjustSelectWidth();
    });
    //~~~~~~~~~~~~~~~~~~~~  << End of Window resize >>  ~~~~~~~~~~~~~~~~~~~~//


    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << Functions Executed by Mouse clicked position >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
    var pageX, pageY;
    $(document).click(function (event) {
        pageX = event.pageX;
        pageY = event.pageY;
    });
    //~~~~~~~~~~~~~~~~~~~~  << End of Mouse clicked position >>  ~~~~~~~~~~~~~~~~~~~~//


    //~~~~~~~~~~ << New Function : Element position settings >> ~~~~~~~~~~//
    var elemLeft, elemRight, elemTop, elemBottom, elemPos;

    function elemPositions(elem, Width, Height) {
        if (elem && Width > 0 && Height > 0) {
            elemPos = elem.offset();
            elemTop = elemPos.top - 20;
            elemLeft = elemPos.left;
            elemRight = (elemLeft + Width);
            elemBottom = (elemTop + Height);
        }
    }
    //~~~~~~~~~~ << End of Function : Element position settings >> ~~~~~~~~~~//

    //Language Slide down&up
    var shift = 0;
    $("#header .bottom .lang p").click(function () {
        if (shift === 0) {
            $("#header .bottom .lang ul").slideDown("slow");
            shift = 1;
        } else {
            $("#header .bottom .lang ul").slideUp("slow");
            shift = 0;
        }
    });
    $("#header .bottom .lang ul li a").click(function (e) {
        e.preventDefault();
    });

    // Navbar section
    //~~~~~~~~~~ << New Function : Adding&Removing active class to element >> ~~~~~~~~~~//
    function addRemoveActive(elem, This) {
        elem.removeClass("active");
        This.addClass("active");
    }
    //~~~~~~~~~~ << End of Function : Adding&Removing active class to element >> ~~~~~~~~~~//


    //Navbar menu clicked event
    $("#header .bottom .menu .menuItem").click(function () {
        var This = $(this);
        addRemoveActive($("#header .bottom .menu .active"), This)
    });


    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << Pagination codes >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//

    function CallPagination(page, recordCountOnePageParam, breakPointParam) {
        var recordsCount = $("#" + page + " .content table tbody tr").length;
        var records = $("#" + page + " .content table tbody tr");
        var recordCountOnePage = recordCountOnePageParam;
        var breakPoint = breakPointParam;
        var countOfPages = Math.ceil(recordsCount / recordCountOnePage);
        var pageButtons = $("#" + page + " .footer .paginationCauseListNav .buttonItem");

        if (countOfPages <= 1) {
            $(".footer .paginationCauseListNav").hide();
        }

        var j = 0;
        var ul = $(".footer .paginationCauseListNav");

        if (countOfPages < breakPoint) {
            for (var i = 0; i < countOfPages; i++) {

                if (j === 0) {
                    var li = $("<li><i class='fas fa-angle-double-left'></i></li>");
                    li.attr("data-index", "prev");
                    li.addClass("prev");
                    ul.append(li);
                }
                j++;

                var li = $("<li>" + (i + 1) + "</li>");
                li.attr("data-index", "" + (i + 1) + "");
                if (i === 0) {
                    li.addClass("active");
                }
                li.addClass("buttonItem");
                ul.append(li);

                if (j === countOfPages) {
                    var li = $("<li><i class='fas fa-angle-double-right'></i></li>");
                    li.attr("data-index", "next");
                    li.addClass("next");
                    ul.append(li);
                }
            }
        }
        else {
            for (var i = 0; i < countOfPages; i++) {

                if (j == 0) {
                    var li = $("<li><i class='fas fa-angle-double-left'></i></li>");
                    li.attr("data-index", "prev");
                    li.addClass("prev");
                    ul.append(li);

                    var li = $("<li>...</li>");
                    li.css("display", "none");
                    li.attr("data-index", "pointPrev");
                    li.addClass("pointPrev jump");
                    ul.append(li);
                }
                j++;

                var li = $("<li>" + (i + 1) + "</li>");
                li.attr("data-index", "" + (i + 1) + "");
                li.addClass("buttonItem");
                if (i == 0) {
                    li.addClass("active");
                }
                if (i >= breakPoint) {
                    li.css("display", "none");
                }

                ul.append(li);

                if (j == (countOfPages)) {
                    var li = $("<li>...</li>");
                    li.attr("data-index", "pointNext");
                    li.addClass("pointNext  jump");
                    ul.append(li);
                }
                if (j == countOfPages) {
                    var li = $("<li><i class='fas fa-angle-double-right'></i></li>");
                    li.attr("data-index", "next");
                    li.addClass("next");
                    ul.append(li);
                }
            }
        }


        //Go to appropriate page
        var p = 1;

        function GoPage(records, CurrentPage, recordCountOnePage, This, breakPoint, pageButtons) {

            var active = $(".footer .paginationCauseListNav .active");
            var countOfPages = Math.ceil(records.length / recordCountOnePage);
            var CurrentPageIn = CurrentPage;
            var breakPointIndex = Math.floor(countOfPages / breakPoint);

            //Go to next page   
            if (CurrentPage == "next") {
                if (countOfPages <= breakPoint) {
                    if (active.data("index") < countOfPages) {
                        CurrentPageIn = (active.data("index") + 1);
                        active.next().addClass("active");
                        active.removeClass("active");
                    } else {
                        CurrentPageIn = active.data("index");
                    }
                } else {
                    if (active.data("index") < breakPoint) {
                        CurrentPageIn = (active.data("index") + 1);
                        active.next().addClass("active");
                        active.removeClass("active");
                    } else {
                        $(".footer .paginationCauseListNav .pointPrev").css("display", "inline-block");
                        if (active.data("index") >= (countOfPages - 1)) {
                            $(".footer .paginationCauseListNav .pointNext").css("display", "none");
                        }
                        if (active.data("index") < countOfPages) {
                            active.next().addClass("active");
                            active.removeClass("active");
                            var f = 0;
                            $("#" + page + " .footer .paginationCauseListNav .buttonItem").each(function () {
                                $(this).css('display', 'none');
                                if ((f > (active.data("index") - breakPoint)) && (f <= (active.data("index")))) {
                                    $(this).css("display", "inline-block");
                                }
                                f++;
                            });
                            CurrentPageIn = (active.data("index") + 1);
                        } else {
                            CurrentPageIn = active.data("index");
                        }
                    }
                }
                showRecords(CurrentPageIn, recordCountOnePage);
            }

            //Go to previous page
            if (CurrentPage == "prev") {
                if (countOfPages <= breakPoint) {
                    if (active.data("index") > 1) {
                        CurrentPageIn = (active.data("index") - 1);
                        active.prev().addClass("active");
                        active.removeClass("active");

                    } else {
                        CurrentPageIn = 1;
                    }

                } else {
                    if (active.data("index") > (countOfPages + 1 - breakPoint)) {
                        CurrentPageIn = (active.data("index") - 1);
                        active.prev().addClass("active");
                        active.removeClass("active");
                    } else {
                        console.log("bu");
                        if (active.data("index") > 1) {
                            CurrentPageIn = (active.data("index") - 1);
                            active.prev().addClass("active");
                            active.removeClass("active");
                            var f = 0;
                            $("#" + page + " .footer .paginationCauseListNav .buttonItem").each(function () {
                                $(this).css('display', 'none');
                                if ((f >= (active.data("index") - 2)) && (f < (active.data("index") + breakPoint - 2))) {
                                    $(this).css("display", "inline-block");
                                }
                                f++;
                                $(".footer .paginationCauseListNav .pointNext").css("display", "inline-block");
                            });

                            if (active.data("index") <= 2) {
                                $(".footer .paginationCauseListNav .pointPrev").css("display", "none");
                            }
                        } else {
                            CurrentPageIn = 1;
                            $(".footer .paginationCauseListNav .pointPrev").css("display", "none");
                        }
                    }
                }
                showRecords(CurrentPageIn, recordCountOnePage);
            }

            //Go to appropriate page
            if (This && CurrentPage != "prev" && CurrentPage != "next" && CurrentPage != "pointPrev" && CurrentPage != "pointNext") {
                $(".footer .paginationCauseListNav .active").removeClass("active");
                This.addClass("active");
                showRecords(CurrentPage, recordCountOnePage);
            }


        };

        //Show appropriate records
        function showRecords(CurrentPageIn, recordCountOnePage) {
            var k = 0;
            records.each(function () {
                $(this).css('display', 'none');
                if (k < (CurrentPageIn * recordCountOnePage) && k >= ((CurrentPageIn - 1) * recordCountOnePage)) {
                    $(this).css("display", "table-row");
                }
                k++;
            });
        }

        //Caling function on button click
        $(".footer .paginationCauseListNav li").on("click", function () {
            var CurrentPage = $(this).data("index");
            var This = $(this);
            GoPage(records, CurrentPage, recordCountOnePage, This, breakPoint, pageButtons);
        });

        //Calling function on page load
        GoPage(records, 1, recordCountOnePage);

        //Show appropriate records on page load
        showRecords(1, recordCountOnePage);

    }


    var ClosedInitContIndex = "ClosedInitContIndex";
    //var accidentDescription = "accidentDescription";
    //var accidentLose = "accidentLose";
    //var accidentReportsAdmin = "accidentReportsAdmin";
    //var accidentReports = "accidentReports";

    //var driverInputIndex = "driverInputIndex";
    //var vehInputIndex = "vehInputIndex";
    //var rutInputIndex = "rutInputIndex";

    // var accidentVehicleSubmit = "accidentVehicleSubmit .content-style-outlook";    

    //Operations
    if ($("#ClosedInitContIndex").length > 0) {
        CallPagination(ClosedInitContIndex, 10, 5, "tr");
    }
    //if ($("#accidentDescription").length > 0) {
    //    CallPagination(accidentDescription, 10, 5, "tr");
    //}
    //if ($("#accidentLose").length > 0) {
    //    CallPagination(accidentLose, 10, 5, "tr");
    //}

    ////Reports
    //if ($("#accidentReportsAdmin").length > 0) {
    //    CallPagination(accidentReportsAdmin, 10, 5, "tr");
    //}
    //if ($("#accidentReports").length > 0) {
    //    CallPagination(accidentReports, 10, 5, "tr");
    //}

    ////Inputs
    //if ($("#driverInputIndex").length > 0) {
    //    CallPagination(driverInputIndex, 20, 5, "tr");
    //}
    //if ($("#vehInputIndex").length > 0) {
    //    CallPagination(vehInputIndex, 20, 5, "tr");
    //}
    //if ($("#rutInputIndex").length > 0) {
    //    CallPagination(rutInputIndex, 20, 5, "tr");
    //}

    //~~~~~~~~~~~~~~~~~~~~  << End of Pagination codes >>  ~~~~~~~~~~~~~~~~~~~~//


    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << Notifications >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//

    var bell = $("#header .user .notifications .bell");

    //bell.on("click", function (e) {
    //    e.preventDefault();
    //    $("#header .user .notifications .notificationWrapper").slideToggle();
    //});

    $(document).on("click", function (e) {
        if (e.target.classList.contains("showElement") === false) {
            $("#header .user .notifications .notificationWrapper").slideUp();
        }
    });


    $("#header .user .notifications .notificationWrapper").removeClass("showElement")

    // Click on notification icon for show notification
    bell.click(function (e) {

        e.preventDefault();
        if (!($("#header .user .notifications .notificationWrapper").hasClass("showElement"))) {
            $("#header .user .notifications .notificationWrapper").show().addClass("showElement");

        }
        else {
            $("#header .user .notifications .notificationWrapper").hide().removeClass("showElement");

        }
        var count = 0;
        count = parseInt($('#header .user .notifications .notiCount').html()) || 0;
        //only load notification if not already loaded
        if (count > 0) {
            updateNotification();
        }
        $('#header .user .notifications .notiCount', this).html('&nbsp;');
    });

    // hide notifications
    //$('html').click(function () {
    //    $('.noti-content').hide();
    //});

    // update notification
    function updateNotification() {
        $('#header .user .notifications .notificationWrapper .showElement').empty();
        $('#header .user .notifications .notificationWrapper .showElement').append($('<li>Loading...</li>'));
        $.ajax({
            type: 'GET',
            url: '/Notifications/GetNotifications',
            success: function (response) {
                $('#header .user .notifications .notificationWrapper .showElement').empty();
                if (response.length == 0) {
                    $('#header .user .notifications .notificationWrapper .showElement').append($('<li>No data available</li>'));
                }
                $.each(response, function (index, value) {
                    $('#header .user .notifications .notificationWrapper .showElement').append($('<li>New contact : ' + value.NotificationTitleId + ' (' + value.UserId + ') added</li>'));
                });
            },
            error: function (error) {
                console.log(error);
            }
        });
    }

    //update notification count
    function updateNotificationCount() {
        var count = 0;
        count = parseInt($('#header .user .notifications .notiCount').html()) || 0;
        count++;
        $('#header .user .notifications .notiCount').html(count);
    }

    //function updateNotificationCount(userId) {
    //    console.log(userId);
    //    Id = parseInt(userId);
    //    $.ajax({
    //        url: "/Notifications/GetNotificationsOfUser/" + Id,
    //        type: "get",
    //        dataType: "json",
    //        success: function (response) {
    //            if (response.status != 0) {
    //                $('#header .user .notifications .notiCount').empty();
    //                $('#header .user .notifications .notiCount').text(response.status);
    //            }
    //            console.log(response.status);
    //        },
    //        error: function (error) {
    //            console.log(response.status);
    //            console.log(error);
    //        }
    //    });
    //}

    // signalr js code for start hub and send receive notification
    var notificationHub = $.connection.notificationHub;
    var userId = $("#header #userId").html();
    $.connection.hub.qs = { "myUserId": userId };

    $.connection.hub.start().done(function () {
        console.log('Notification hub started');
    });

    //signalr method for push server message to client
    notificationHub.client.notify = function (message) {
        //var userId2 = parseInt($("#header #userId").val());
        if (message && message.toLowerCase() == "added") {
            //updateNotificationCount(userId2);
            updateNotificationCount();
            console.log(message);
        }
    };

    //testsss

    //notificationHub.client.myQueryString = function (userId, connectionId) {
    //    console.log("User Id: " + userId +" / Connection Id: "+ connectionId);
    //};

    notificationHub.client.test = function (data, userId, list) {
        console.log("Data length: " + data + ", User length: " + userId + ", List length: " + list);
    };

    notificationHub.client.test1 = function (data, userId, list) {
        console.log("Data length: " + data + ", User length: " + userId + ", List length: " + list);
    };

    //notificationHub.client.test2 = function (userId) {
    //    console.log("User id: " + userId);
    //};

    //notificationHub.client.test3 = function (test3) {
    //    console.log(test3);
    //};

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << End of Notifications >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//


    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << Notifications-responsive >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//

    var bell = $(".header-responsive .notifications .bell");


    //bell.on("click", function (e) {
    //    e.preventDefault();
    //    $("#header .user .notifications .notificationWrapper").slideToggle();
    //});

    $(document).on("click", function (e) {
        if (e.target.classList.contains("showElement") === false) {
            $(".header-responsive .notifications .notificationWrapper").slideUp();
        }
    });



    //$(".header-responsive  .notifications .notificationWrapper").removeClass("showElement")

    // Click on notification icon for show notification
    bell.click(function (e) {
        e.preventDefault();

        if (!($(".header-responsive .notifications .notificationWrapper").hasClass("showElement"))) {
            $(".header-responsive  .notifications .notificationWrapper").show().addClass("showElement");

        }
        else {
            $(".header-responsive  .notifications .notificationWrapper").hide().removeClass("showElement");

        }
        var count = 0;
        count = parseInt($('.header-responsive .notifications .notiCount').html()) || 0;
        //only load notification if not already loaded
        if (count > 0) {
            updateNotification();
        }
        $('.header-responsive .notifications .notiCount', this).html('&nbsp;');
    });

    // hide notifications
    //$('html').click(function () {
    //    $('.noti-content').hide();
    //});

    // update notification
    function updateNotification() {
        $('.header-responsive .notifications .notificationWrapper .showElement').empty();
        $('.header-responsive .notifications .notificationWrapper .showElement').append($('<li>Loading...</li>'));
        $.ajax({
            type: 'GET',
            url: '/Notifications/GetNotifications',
            success: function (response) {
                $('.header-responsive .notifications .notificationWrapper .showElement').empty();
                if (response.length == 0) {
                    $('.header-responsive .notifications .notificationWrapper .showElement').append($('<li>No data available</li>'));
                }
                $.each(response, function (index, value) {
                    $('.header-responsive .notifications .notificationWrapper .showElement').append($('<li>New contact : ' + value.NotificationTitleId + ' (' + value.UserId + ') added</li>'));
                });
            },
            error: function (error) {
                console.log(error);
            }
        });
    }

    //update notification count
    function updateNotificationCount() {
        var count = 0;
        count = parseInt($('.header-responsive .notifications .notiCount').html()) || 0;
        count++;
        $('.header-responsive .notifications .notiCount').html(count);
    }

    //function updateNotificationCount(userId) {
    //    console.log(userId);
    //    Id = parseInt(userId);
    //    $.ajax({
    //        url: "/Notifications/GetNotificationsOfUser/" + Id,
    //        type: "get",
    //        dataType: "json",
    //        success: function (response) {
    //            if (response.status != 0) {
    //                $('#header .user .notifications .notiCount').empty();
    //                $('#header .user .notifications .notiCount').text(response.status);
    //            }
    //            console.log(response.status);
    //        },
    //        error: function (error) {
    //            console.log(response.status);
    //            console.log(error);
    //        }
    //    });
    //}

    // signalr js code for start hub and send receive notification
    var notificationHub = $.connection.notificationHub;
    var userId = $(".header-responsive #userId").html();
    $.connection.hub.qs = { "myUserId": userId };

    $.connection.hub.start().done(function () {

    });

    //signalr method for push server message to client
    notificationHub.client.notify = function (message) {
        //var userId2 = parseInt($("#header #userId").val());
        if (message && message.toLowerCase() == "added") {
            //updateNotificationCount(userId2);
            updateNotificationCount();
            console.log(message);
        }
    };

    //testsss

    //notificationHub.client.myQueryString = function (userId, connectionId) {
    //    console.log("User Id: " + userId +" / Connection Id: "+ connectionId);
    //};

    notificationHub.client.test = function (data, userId, list) {
        console.log("Data length: " + data + ", User length: " + userId + ", List length: " + list);
    };

    notificationHub.client.test1 = function (data, userId, list) {
        console.log("Data length: " + data + ", User length: " + userId + ", List length: " + list);
    };

    //notificationHub.client.test2 = function (userId) {
    //    console.log("User id: " + userId);
    //};

    //notificationHub.client.test3 = function (test3) {
    //    console.log(test3);
    //};

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << End of Notifications-responsive >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//


    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << Functions Execute AJAX codes >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
    //Getting action list by operation
    $("#OperationForActions").change(function (e) {
        e.preventDefault();
        var Id = $(this).val();

        $.ajax({
            url: "/UserOperations/GetActions/" + Id,
            type: "get",
            dataType: "json",
            success: function (response) {
                // console.log(response);
                $("#JsonActionList").empty();
                for (var i = 0; i < response.length; i++) {
                    var op = "<option value=" + response[i].Id + ">" + response[i].Name + "</option>"
                    $("#JsonActionList").append(op);
                }
            }

        });
    });

    //Creating init cont
    //Checking vehicle
    $("#InitContCreate #VehicleId").change(function (e) {
        //console.log($(this).val()); SuccessInfo 
        $("#InitContCreate form .SuccessInfo").css("display", "none");
        $("#InitContCreate form .ajaxNotifFirstCheck").css("display", "none");
        var id = $(this).val();
        if (id === 0) {
            id = 0;
        }

        if ($("#InitContCreate #CheckUp").is(":checked")) {
            $.ajax({
                url: "/MainOperations/CheckupCreateCheckVehicle/" + id,
                type: "post",
                dataType: "json",
                success: function (response) {
                    if (response.status === "failedCheckup") {
                        $("#InitContCreate form .ajaxNotifVehicle").css("display", "block");
                        $("#InitContCreate form .ajaxNotifVehicle span").text("Seçdiyiniz NV artıq təmir prosesindədir.");
                    } else if (response.status === "failedInitCont") {
                        $("#InitContCreate form .ajaxNotifVehicle").css("display", "block");
                        $("#InitContCreate form .ajaxNotifVehicle span").text("Seçdiyiniz NV artıq xəttə buraxılıb.");
                    }
                    else {
                        $("#InitContCreate form .ajaxNotifVehicle").css("display", "none");
                    }

                    if (response.maintenanceStatus === "MaintenanceTrue") {
                        $("#InitContCreate form .maintenanceDataWrapper").css("display", "block");
                        $("#InitContCreate form #maintenanceData").val(response.maintenanceData);
                        $("#InitContCreate form #Description").text(response.maintenanceData);
                    } else {
                        $("#InitContCreate form .maintenanceDataWrapper").css("display", "none");
                        $("#InitContCreate form #maintenanceData").val("");
                        $("#InitContCreate form #Description").text("");
                    }
                }
            });
        } else {
            $.ajax({
                url: "/MainOperations/InitCreateCheckVehicle/" + id,
                type: "post",
                dataType: "json",
                success: function (response) {
                    if (response === "failedCheckup") {
                        $("#InitContCreate form .ajaxNotifVehicle").css("display", "block");
                        $("#InitContCreate form .ajaxNotifVehicle span").text("Seçdiyiniz NV artıq təmir prosesindədir.");
                    } else if (response === "failedInitCont") {
                        $("#InitContCreate form .ajaxNotifVehicle").css("display", "block");
                        $("#InitContCreate form .ajaxNotifVehicle span").text("Seçdiyiniz NV artıq xəttə buraxılıb.");
                    }
                    else {
                        $("#InitContCreate form .ajaxNotifVehicle").css("display", "none");
                    }
                }
            });
        }
    });

    //Checking if vehicle not selected
    $("#InitContCreate #CheckUp").click(function (e) {

        if ($(this).is(":checked")) {
            var VehivleId = $("#InitContCreate #VehicleId").val();
            if (VehivleId == 0) {
                $("#InitContCreate form .ajaxNotifFirstCheck").css("display", "block");
                $("#InitContCreate form .ajaxNotifFirstCheck span").text("Əvvəlcə NV seçməlisiniz.");
                $("#InitContCreate #CheckUp").prop("checked", false);
                return;
            }
            $("#InitContCreate #route").hide(300);
            $("#InitContCreate #driver").hide(300);
            $("#InitContCreate #others").hide(300);
            $("#InitContCreate #note").show(300);
        } else {
            $("#InitContCreate #route").show(300);
            $("#InitContCreate #driver").show(300);
            $("#InitContCreate #others").show(300);
            $("#InitContCreate #note").hide(300);
            $("#InitContCreate .maintenanceDataWrapper").hide(300);
        }
    });

    //Checking Leave kilometer
    $("#InitContCreate #LeavingKilometer").change(function (e) {
        $("#InitContCreate form .SuccessInfo").css("display", "none");

        var Id = $("#InitContCreate #VehicleId").val();
        var Km = $(this).val();

        var obj = {
            id: Id,
            km: Km
        };

        $.ajax({
            url: "/MainOperations/InitCreateCheckKM/",
            type: "post",
            data: JSON.stringify(obj),
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                if (response === "failedId") {
                    $("#InitContCreate form .ajaxNotifKM").css("display", "block");
                    $("#InitContCreate form .ajaxNotifKM span").text("Əvvəlcə NV seçməlisiniz.");
                } else if (response === "unset") {
                    $("#InitContCreate form .ajaxNotifKM").css("display", "none");
                } else {
                    $("#InitContCreate form .ajaxNotifKM").css("display", "block");
                    $("#InitContCreate form .ajaxNotifKM span").text("Seçilmiş NV-nin son km göstəricisi " + response + " olub.");
                }
            }
        });
    });

    //Checking Enter kilometer
    $("#InitContClose #EnterKilometer").change(function (e) {
        $("#InitContClose form .SuccessInfo").css("display", "none");

        if ($("#InitContClose #FirstDrvKm").val() == "") {
            $("#InitContClose .NotifKm").css("display", "block");
            $("#InitContClose .NotifKm span").text("Əvvəlcə birinci sürücünün kilometrajını daxil edin");
            return;
        } else if (parseInt($(this).val()) < parseInt($("#InitContClose #FirstDrvKm").val())) {
            $("#InitContClose .NotifKm").css("display", "block");
            $("#InitContClose .NotifKm span").text("Birinci sürücü və ya giriş kilometrajında səhvlik var");
            return;
        } else {
            $("#InitContClose .NotifKm").css("display", "none");
        }

        var Id = $("#InitContClose #CheckFuelId").val();
        var Km = $(this).val();

        var obj = {
            id: Id,
            km: Km
        };

        $.ajax({
            url: "/MainOperations/InitCreateCheckEnterKM/",
            type: "post",
            data: JSON.stringify(obj),
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                if (response.status === "unset") {
                    $("#InitContClose form .NotifKm").css("display", "none");
                }
                else if (response.status === "overLimit") {
                    $("#InitContClose form .NotifKm").css("display", "block");
                    $("#InitContClose form .NotifKm span").text("NV-nin çıxış hərəkət məsafəsi limiti " + response.kmLimit + " km-dir");
                }
                else if (response.status === "wrongKM") {
                    $("#InitContClose form .NotifKm").css("display", "block");
                    $("#InitContClose form .NotifKm span").text("NV-nin çıxış km göstəricisi " + response.currentKm + " olub.");
                }
            }
        });
    });

    //Checking First driver kilometer
    $("#InitContClose #FirstDrvKm").change(function (e) {
        $("#InitContClose form .SuccessInfo").css("display", "none");

        $("#InitContClose #EnterKilometer").val('');
        $("#InitContClose .NotifKm").css("display", "none");

        var Id = $("#InitContClose #CheckFuelId").val();
        var Km = $(this).val();

        var obj = {
            id: Id,
            km: Km
        };

        $.ajax({
            url: "/MainOperations/InitCreateCheckEnterKM/",
            type: "post",
            data: JSON.stringify(obj),
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                if (response.status === "unset") {
                    $("#InitContClose form .FirstDrvKm").css("display", "none");
                }
                else if (response.status === "overLimit") {
                    $("#InitContClose form .FirstDrvKm").css("display", "block");
                    $("#InitContClose form .FirstDrvKm span").text("NV-nin çıxış hərəkət məsafəsi limiti " + response.kmLimit + " km-dir");
                }
                else if (response.status === "wrongKM") {
                    $("#InitContClose form .FirstDrvKm").css("display", "block");
                    $("#InitContClose form .FirstDrvKm span").text("NV-nin çıxış km göstəricisi " + response.currentKm + " olub.");
                }
            }
        });
    });

    //Checking Enter fuel
    $("#InitContClose #EnterFuel").change(function (e) {
        $("#InitContClose form .SuccessInfo").css("display", "none");

        if ($("#InitContClose #FirstDrvFuel").val() == "") {
            $("#InitContClose .NotifFuel").css("display", "block");
            $("#InitContClose .NotifFuel span").text("Əvvəlcə birinci sürücünün yanacaq göstəricisini daxil edin");
            return;
        } else if (parseInt($(this).val()) > parseInt($("#InitContClose #FirstDrvFuel").val())) {
            $("#InitContClose .NotifFuel").css("display", "block");
            $("#InitContClose .NotifFuel span").text("Birinci sürücü və ya giriş yanacaq göstəricilərində səhvlik var");
            return;
        } else {
            $("#InitContClose .NotifFuel").css("display", "none");
        }

        var Id = $("#InitContClose #CheckFuelId").val();
        var litr = $(this).val();

        var obj = {
            id: Id,
            litr: litr
        };

        $.ajax({
            url: "/MainOperations/InitCreateCheckEnterFuel/",
            type: "post",
            data: JSON.stringify(obj),
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                if (response.status === "unset") {
                    $("#InitContClose form .NotifFuel").css("display", "none");
                }
                else if (response.status === "wrongLitr") {
                    $("#InitContClose form .NotifFuel").css("display", "block");
                    $("#InitContClose form .NotifFuel span").text("NV-nin çıxış yanacağı " + response.leaveFuel + " bar-dır");
                }
            }
        });
    });

    //Checking First driver fuel
    $("#InitContClose #FirstDrvFuel").change(function (e) {
        $("#InitContClose form .SuccessInfo").css("display", "none");

        $("#InitContClose #EnterFuel").val('');
        $("#InitContClose .NotifFuel").css("display", "none");

        var Id = $("#InitContClose #CheckFuelId").val();
        var litr = $(this).val();

        var obj = {
            id: Id,
            litr: litr
        };

        $.ajax({
            url: "/MainOperations/InitCreateCheckEnterFuel/",
            type: "post",
            data: JSON.stringify(obj),
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                if (response.status === "unset") {
                    $("#InitContClose form .FirstDrvFuel").css("display", "none");
                }
                else if (response.status === "wrongLitr") {
                    $("#InitContClose form .FirstDrvFuel").css("display", "block");
                    $("#InitContClose form .FirstDrvFuel span").text("NV-nin çıxış yanacağı " + response.leaveFuel + " bar-dır");
                }
            }
        });
    });

    //Get spareparts by vehicle brand - CREATE
    $("#BindSparePartCreate #VehicleBrand").change(function (e) {

        var Id = $(this).val();
        var SparePatr = $("#BindSparePartCreate #WarehouseId");

        $.ajax({
            url: "/Monipulations/GetSparePartsByvehicleBrand/" + Id,
            type: "post",
            dataType: "json",
            success: function (response) {
                console.log(response);
                if (response.length > 0) {
                    SparePatr.empty();
                    SparePatr.append($("<option selected>Seçin ...</option>"));
                    for (var i = 0; i < response.length; i++) {
                        var opt = $("<option value='" + response[i].sparePartId + "'>" + response[i].sparePartCode + " - " + response[i].sparePartName + "</option>");
                        SparePatr.append(opt);
                    }
                } else {
                    SparePatr.empty();
                    SparePatr.append($("<option selected>Seçin ...</option>"));
                }
            },
            complete: function () {
                SparePatr.selectpicker('refresh');
            }
        });

    });

    //Get spareparts by vehicle brand - UPDATE
    $("#BindSparePartUpdate #VehicleBrand").change(function (e) {

        var Id = $(this).val();
        var SparePatr = $("#BindSparePartUpdate #WarehouseId");

        $.ajax({
            url: "/Monipulations/GetSparePartsByvehicleBrand/" + Id,
            type: "post",
            dataType: "json",
            success: function (response) {
                console.log(response);
                if (response.length > 0) {
                    SparePatr.empty();
                    SparePatr.append($("<option selected>Seçin ...</option>"));
                    for (var i = 0; i < response.length; i++) {
                        var opt = $("<option value='" + response[i].sparePartId + "'>" + response[i].sparePartCode + " - " + response[i].sparePartName + "</option>");
                        SparePatr.append(opt);
                    }
                } else {
                    SparePatr.empty();
                    SparePatr.append($("<option selected>Seçin ...</option>"));
                }
            },
            complete: function () {
                SparePatr.selectpicker('refresh');
            }
        });

    });

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << End >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//



    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << Clearing filters >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
    //Open
    $("#OpenInitContIndex #clearFilter").click(function (e) {
        e.preventDefault();
        $("#OpenInitContIndex form input").val('');
        $("#OpenInitContIndex form select").val('null');
        $("#OpenInitContIndex form select").selectpicker("refresh");
    });

    //Close
    $("#ClosedInitContIndex #clearFilter").click(function (e) {
        e.preventDefault();
        $("#ClosedInitContIndex form input").val('');
        $("#ClosedInitContIndex form select").val('null');
        $("#ClosedInitContIndex form select").selectpicker("refresh");
    });

    //Close Mechanics
    $("#ClosedInitContMechIndex #clearFilter").click(function (e) {
        e.preventDefault();
        $("#ClosedInitContMechIndex form input").val('');
        $("#ClosedInitContMechIndex form select").val('null');
        $("#ClosedInitContMechIndex form select").selectpicker("refresh");
    });

    //SOCAR Fuel InitContCheckupIndex
    $("#InitContSOCARIndex #clearFilter").click(function (e) {
        e.preventDefault();
        $("#InitContSOCARIndex form input").val('');
        $("#InitContSOCARIndex form select").val('null');
        $("#InitContSOCARIndex form select").selectpicker("refresh");
    });

    //Checkup Index
    $("#InitContCheckupIndex #clearFilter").click(function (e) {
        e.preventDefault();
        $("#InitContCheckupIndex form input").val('');
        $("#InitContCheckupIndex form select").val('null');
        $("#InitContCheckupIndex form select").selectpicker("refresh");
    });

    //Maintenance manitoring
    $("#MaintenanceMonitorngIndex #clearFilter").click(function (e) {
        e.preventDefault();
        $("#MaintenanceMonitorngIndex form input").val('');
        $("#MaintenanceMonitorngIndex form select").val('null');
        $("#MaintenanceMonitorngIndex form select").selectpicker("refresh");
    });

    //Jobcards Index
    $("#JobcardIndex #clearFilter").click(function (e) {
        e.preventDefault();
        $("#JobcardIndex form input").val('');
        $("#JobcardIndex form select").val('null');
        $("#JobcardIndex form select").selectpicker("refresh");
    });

    //Open Jobcards
    $("#JobcardOpen #clearFilter").click(function (e) {
        e.preventDefault();
        $("#JobcardOpen form input").val('');
        $("#JobcardOpen form select").val('null');
        $("#JobcardOpen form select").selectpicker("refresh");
    });

    //Closed Jobcards
    $("#JobcardClosed #clearFilter").click(function (e) {
        e.preventDefault();
        $("#JobcardClosed form input").val('');
        $("#JobcardClosed form select").val('null');
        $("#JobcardClosed form select").selectpicker("refresh");
    });

    //Jobcards in wait route
    $("#JobcardInWaitRoute #clearFilter").click(function (e) {
        e.preventDefault();
        $("#JobcardInWaitRoute form input").val('');
        $("#JobcardInWaitRoute form select").val('null');
        $("#JobcardInWaitRoute form select").selectpicker("refresh");
    });

    //Jobcards in wait depot
    $("#JobcardInWaitDepot #clearFilter").click(function (e) {
        e.preventDefault();
        $("#JobcardInWaitDepot form input").val('');
        $("#JobcardInWaitDepot form select").val('null');
        $("#JobcardInWaitDepot form select").selectpicker("refresh");
    });

    //Jobcards All Admin
    $("#JobcardAllAdmin #clearFilter").click(function (e) {
        e.preventDefault();
        $("#JobcardAllAdmin form input").val('');
        $("#JobcardAllAdmin form select").val('null');
        $("#JobcardAllAdmin form select").selectpicker("refresh");
    });

    //Jobcards - Submit repaired bus
    $("#SubmitRepairedBus #clearFilter").click(function (e) {
        e.preventDefault();
        $("#SubmitRepairedBus form input").val('');
        $("#SubmitRepairedBus form select").val('null');
        $("#SubmitRepairedBus form select").selectpicker("refresh");
    });

    //Employee
    $("#driverInputIndex #clearFilter").click(function (e) {
        e.preventDefault();
        $("#driverInputIndex form input").val('');
        $("#driverInputIndex form select").val('null');
        $("#driverInputIndex form select").selectpicker("refresh");
    });

    //Done works
    $("#DoneWorksIndexInput #clearFilter").click(function (e) {
        e.preventDefault();
        $("#DoneWorksIndexInput form input").val('');
        $("#DoneWorksIndexInput form select").val('null');
        $("#DoneWorksIndexInput form select").selectpicker("refresh");
    });

    //Binding spare parts
    $("#BindSparePartIndex #clearFilter").click(function (e) {
        e.preventDefault();
        $("#BindSparePartIndex form input").val('');
        $("#BindSparePartIndex form select").val('null');
        $("#BindSparePartIndex form select").selectpicker("refresh");
    });

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << End >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << Input validations >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//

    $(".numberValidInput").keypress(function (event) {
        return isNumber(event, this);
    });

    function isNumber(evt, element) {

        //console.log(evt);
        //console.log(evt.which);

        var charCode = (evt.which) ? evt.which : event.keyCode;

        if (
            /*(charCode !== 45 || $(element).val().indexOf('-') !== -1) &&*/      // “-” CHECK MINUS, AND ONLY ONE.
            (charCode !== 46 || $(element).val().indexOf('.') !== -1) &&          // “.” CHECK DOT, AND ONLY ONE.
            (charCode !== 44 || $(element).val().indexOf(',') !== -1) &&          // “,” CHECK DOT, AND ONLY ONE.
            (charCode < 48 || charCode > 57))
            return false;

        return true;
    }

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << End >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//


    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << Requisitions >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//

    //Add new row
    var count = 1;
    $("#RequisitionsAdd form .addItem .iClick").on("click", function () {

        var id = $(this).data("jobcard");

        $.ajax({
            url: "/MainOperations/RequisitionsAddItem/" + id,
            type: "post",
            dataType: "json",
            success: function (response) {

                var requsitions = $("#RequisitionsAdd form .requsitions");

                //itemGroup
                var itemGroup = $("<div class='itemGroup'></div>");


                //itemGood
                var itemGood = $("<div class='itemGood'></div>");

                var select = $("<select class='form-control inputItem inputItemSelect selectpicker' name='[" + count + "].TempWarehouseId.' data-live-search='true'></select>");
                var option = $("<option value='0' selected>Seçin ...</option>");
                select.append(option);

                for (var i = 0; i < response.length; i++) {
                    var option2 = $("<option value='" + response[i].wId + "'>" + response[i].PartCode + " - " + response[i].Name + "</option>");
                    select.append(option2);
                }
                itemGood.append(select);

                //itemQuantity
                var itemQuantity = $("<div class='itemQuantity'></div>");
                var inputQuantity = $("<input type='text' class='form-control inputItem numberValidInput' name='[" + count + "].RequiredQuantity'>");
                itemQuantity.append(inputQuantity);

                //If is okay
                var isOk = $("<div class='resultIsOk' name='[" + count + "]'></div>");

                //Alert

                var alert = $('<div class="alert alert-warning" role="alert" name=[' + count + ']></div>')
                //Add All
                itemGroup.append(itemGood);
                itemGroup.append(itemQuantity);
                itemGroup.append(isOk);
                itemGroup.append(alert)
                requsitions.append(itemGroup);
            },
            complete: function () {
                $("select").selectpicker('refresh');
            }



            //success: function (response) {
            //    for (var k = 0; k <response.length; k++) {
            //        //console.log(response[k].Name);
            //    }
            //    //return;

            //    var requsitions = $("#RequisitionsAdd form .requsitions");

            //    //itemGroup
            //    var itemGroup = $("<div class='itemGroup'></div>");


            //    //itemGood
            //    var itemGood = $("<div class='itemGood'></div>");

            //    var dropdownBootstrapSelect = $("<div class='dropdown bootstrap-select form-control inputItem inputItemSelect'></div>");

            //    var select = $("<select id='TempWarehouseId' class='form-control inputItem inputItemSelect selectpicker' name='[" + count +"].TempWarehouseId' data-live-search='true' tabindex='-98'></select>");
            //    var option = $("<option value='0' selected>Seçin ...</option>");
            //    select.append(option);

            //    for (var i = 0; i < response.length; i++) {
            //        var option2 = $("<option value='"+response[i].wId+"'>"+response[i].Name+"</option>");
            //        select.append(option2);
            //    }     
            //    //var option2 = $("<option value='1' selected>Rasim</option>");
            //    //var option3 = $("<option value='2' selected>Tofiq</option>");

            //    //select.append(option2);
            //    //select.append(option3);


            //    var button = $("<button  type='button' class='btn dropdown-toggle btn-light' data-toggle='dropdown' role='button' data-id='TempWarehouseId' title='Seçin ...' aria-expanded='false'></button>");
            //    var filterOption = $("<div class='filter-option'></div>");
            //    var filterOptionInner = $("<div class='filter-option-inner'></div>");
            //    var filterOptionInnerInner = $("<div class='filter-option-inner-inner'>Seçin ...</div>");
            //    filterOptionInner.append(filterOptionInnerInner);
            //    filterOption.append(filterOptionInner);
            //    button.append(filterOption);

            //    var dropdownMenu = $("<div class='dropdown-menu' role='combobox' x-placement='top-start' style='max-height: 204.328px; overflow: hidden; min-height: 162px; min-width: 511px; position: absolute; will-change: transform; top: 0px; left: 0px; transform: translate3d(-1px, -2px, 0px);'></div>");
            //    var bsSearchbox = $("<div class='bs-searchbox'></div>");
            //    var input = $("<input type='text' class='form-control' autocomplete='off' role='textbox' aria-label='Search'>");
            //    bsSearchbox.append(input);

            //    var innerShow = $("<div class='inner show' role='listbox' aria-expanded='false' tabindex='-1' style='max-height: 163.672px; overflow-y: auto; min-height: 0px;'></div>");
            //    var ul = $("<ul class='dropdown-menu inner show'></ul>");
            //    var li = $("<li class='selected active'></li>");
            //    var a = $("<a role='option' class='dropdown-item selected active' aria-disabled='false' tabindex='0' aria-selected='true'></a>");
            //    var span = $("<span class='bs-ok-default check-mark'></span>");
            //    var span2 = $("<span class='text'>Seçin ...</span>");
            //    a.append(span);
            //    a.append(span2);
            //    li.append(a);
            //    ul.append(li);

            //    for (var j = 0; j <response.length; j++) {
            //        var li2 = $("<li></li>");
            //        var a2 = $("<a role='option' class='dropdown-item' aria-disabled='false' tabindex='0' aria-selected='false'></a>");
            //        var span21 = $("<span class='bs-ok-default check-mark'></span>");
            //        var span22 = $("<span class='text'>"+ response[j].Name +"</span>");

            //        a2.append(span21);
            //        a2.append(span22);
            //        li2.append(a2);
            //        ul.append(li2);
            //    }



            //    innerShow.append(ul);

            //    dropdownMenu.append(bsSearchbox);
            //    dropdownMenu.append(innerShow);

            //    dropdownBootstrapSelect.append(select);
            //    dropdownBootstrapSelect.append(button);
            //    dropdownBootstrapSelect.append(dropdownMenu);

            //    itemGood.append(dropdownBootstrapSelect);



            //    //itemQuantity
            //    var itemQuantity = $("<div class='itemQuantity'></div>");
            //    var inputQuantity = $("<input type='text' class='form-control inputItem numberValidInput' id='RequiredQuantity' name='[" + count +"].RequiredQuantity'>");
            //    itemQuantity.append(inputQuantity);

            //    //addItem
            //    //var addItem = $("<div class='addItem'></div>");
            //    //var i = $("<i class='fa fa-plus iClick' aria-hidden='true'></i>");
            //    //addItem.append(i);

            //    //Add All
            //    itemGroup.append(itemGood);
            //    itemGroup.append(itemQuantity);
            //    //itemGroup.append(addItem);

            //    requsitions.append(itemGroup);

            //$(this).hide(300);

            //}
        });

        count++;
    });

    //Quantity change - Add method
    $("#RequisitionsAdd form ").on("change", ".itemGroup .itemQuantity input", function () {


        var t1 = $(this)[0].name.indexOf("[");
        var t2 = $(this)[0].name.indexOf("]");
        var val = $(this)[0].name.substring(t1 + 1, t2);

        var valueOfWhouseItem = $("#RequisitionsAdd form .itemGroup .itemGood select[name*=" + val + "]").val();

        //console.log(valueOfWhouseItem);
        //return;

        if (valueOfWhouseItem == 0) {
            $("#RequisitionsAdd form .requsitions .alert-warning[name*=" + val + "]").css("display", "block");
            $("#RequisitionsAdd form .requsitions .alert-warning[name*=" + val + "]").text("Əvvəlcə ehtiyat hissəsi seçin ...");
            return;
        } else {
            $("#RequisitionsAdd form .requsitions .alert-warning[name*=" + val + "]").css("display", "none");
        }

        if ($(this).val() == "") {
            var okay2 = $("#RequisitionsAdd form .itemGroup .resultIsOk[name*=" + val + "]");
            okay2.empty();
            var i2 = $("<i class='fa fa-exclamation' aria-hidden='true'></i>");
            okay2.append(i2);
            return;
        } else {
            var okay3 = $("#RequisitionsAdd form .itemGroup .resultIsOk[name*=" + val + "]");
            okay3.empty();
        }


        var obj = {
            PartId: valueOfWhouseItem,
            Quantity: $(this).val()
        };

        $.ajax({
            url: "/MainOperations/RequisitionsAddItemCheckWarehouse/",
            type: "post",
            data: JSON.stringify(obj),
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            success: function (response) {


                //return;

                if (response.statusCompare === true) {
                    var okay = $("#RequisitionsAdd form .itemGroup .resultIsOk[name*=" + val + "]");
                    okay.empty();
                    var i = $("<i class='fa fa-check' aria-hidden='true'></i>");
                    okay.append(i);

                    $("#RequisitionsAdd form .requsitions .alert-warning[name*=" + val + "]").css("display", "none");

                } else {
                    var okay2 = $("#RequisitionsAdd form .itemGroup .resultIsOk[name*=" + val + "]");
                    okay2.empty();
                    var i2 = $("<i class='fa fa-exclamation' aria-hidden='true'></i>");
                    okay2.append(i2);

                    $("#RequisitionsAdd form .requsitions .alert-warning[name*=" + val + "]").css("display", "block");
                    $("#RequisitionsAdd form .requsitions .alert-warning[name*=" + val + "]").text("Bu ehtiyyat hissəsindən anbarda cəmi " + response.balance + " ədəd(dəst) qalıb.");
                }



            }
        });

    });

    //Part change - Add method
    $("#RequisitionsAdd form").on("change",".itemGroup .itemGood select", function () {
        var t1 = $(this)[0].name.indexOf("[");
        var t2 = $(this)[0].name.indexOf("]");
        var val = $(this)[0].name.substring(t1 + 1, t2);

        $("#RequisitionsAdd form .itemGroup .itemQuantity input[name*=" + val + "]").val('');
        $("#RequisitionsAdd form .requsitions .alert-warning[name*=" + val + "]").css("display", "none");

        var sparePartId = $(this).val();
        var jobCardId = $("#RequisitionsAdd form #JobCardId").val();
        var obj = {
            sparePartId: sparePartId,
            jobCardId: jobCardId
        };

        $.ajax({
            url: "/MainOperations/CheckForNotRepeatSparePart/",
            type: "post",
            data: JSON.stringify(obj),
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                if (response.status === "invalidId") {
                    $("#RequisitionsAdd form .requsitions .alert-warning[name*=" + val + "]").text("Təmir kartı və ya ehtiyyat hissəsi ID-si yanlışdır!");
                    $("#RequisitionsAdd form .requsitions .alert-warning[name*=" + val + "]").css("display", "block");

                } else if (response.status === "included") {
                    $("#RequisitionsAdd form .requsitions .alert-warning[name*=" + val + "]").text("Seçdiyiniz ehtiyyat hissəsi artıq bu təmir kartına əlavə edilib, siz sadəcə sayını dəyişə bilərsiniz!");
                    $("#RequisitionsAdd form .requsitions .alert-warning[name*=" + val + "]").css("display", "block");
                } else {
                    $("#RequisitionsAdd form .requsitions .alert-warning[name*=" + val + "]").css("display", "none");
                }
            }
        });


    });

    //Quantity change - Update method
    $("#RequisitionsUpdate form").on("change",".itemGroup .itemQuantity input", function () {

        var valueOfWhouseItem = $("#RequisitionsUpdate form .itemGroup .itemGood select").val();
        var RequisitionId = $("#RequisitionsUpdate #RequisitionId").val();

        if (valueOfWhouseItem == 0) {
            $("#RequisitionsUpdate form .requsitions .alert-warning[name*=notification]").css("display", "block");
            $("#RequisitionsUpdate form .requsitions .alert-warning[name*=notification]").text("Əvvəlcə ehtiyat hissəsi seçin ...");
            return;
        } else {
            $("#RequisitionsUpdate form .requsitions .alert-warning[name*=notification]").css("display", "none");
        }

        if ($(this).val() == "") {
            var okay2 = $("#RequisitionsUpdate form .itemGroup .resultIsOk");
            okay2.empty();
            var i2 = $("<i class='fa fa-exclamation' aria-hidden='true'></i>");
            okay2.append(i2);
            return;
        } else {
            var okay3 = $("#RequisitionsUpdate form .itemGroup .resultIsOk");
            okay3.empty();
        }


        var obj = {
            PartId: valueOfWhouseItem,
            Quantity: $(this).val(),
            requisitionId: RequisitionId
        };

        $.ajax({
            url: "/MainOperations/RequisitionsAddItemCheckWarehouse/",
            type: "post",
            data: JSON.stringify(obj),
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                if (response.statusCompare === true) {
                    var okay = $("#RequisitionsUpdate form .itemGroup .resultIsOk");
                    okay.empty();
                    var i = $("<i class='fa fa-check' aria-hidden='true'></i>");
                    okay.append(i);

                    $("#RequisitionsUpdate form .requsitions .alert-warning[name*=notification]").css("display", "none");

                } else {
                    var okay2 = $("#RequisitionsUpdate form .itemGroup .resultIsOk");
                    okay2.empty();
                    var i2 = $("<i class='fa fa-exclamation' aria-hidden='true'></i>");
                    okay2.append(i2);

                    $("#RequisitionsUpdate form .requsitions .alert-warning[name*=notification]").css("display", "block");
                    if (response.balance <= 0) {
                        $("#RequisitionsUpdate form .requsitions .alert-warning[name*=notification]").text("Bu ehtiyyat hissəsindən hal-hazırda anbarda qalmayıb.");
                    } else {
                        $("#RequisitionsUpdate form .requsitions .alert-warning[name*=notification]").text("Bu ehtiyyat hissəsindən anbarda cəmi " + response.balance + " ədəd(dəst) qalıb.");
                    }
                }
            }
        });

    });


    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << End >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << Submit Repaired jobcards>>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//

    //Show exclamation mark when checkbox checked
    $(document).on("click", "#SubmitRepairedBus .content .checkForSubmit", function (e) {
        $("#SubmitRepairedBus .footer .submitChecked i").css("display", "inline-block");
    });

    //Submit checked rows
    $(document).on("click", "#SubmitRepairedBus .footer .submitChecked .submitbutton", function (e) {

        if (confirm('Seçilmiş TK-(lar)ı təsdiq etməyə əminsiniz?')) {
            var checkedItems = "", uncheckedItems = "";

            //var dates = $("#accidentVehicleSubmitInsur .content-style-outlook .dateItem");
            //var date = $(this).data("date");
            //var sentData = $("#accidentVehicleSubmitInsur .footer .sentData");

            //Show warning sign
            $("#SubmitRepairedBus .footer .submitChecked i").css("display", "none");

            var checkedJobcards = $("#SubmitRepairedBus .content table tbody .checkForSubmit");
            //var checkedJobcards = $('*[data-jobcardId="1"]');
            checkedJobcards.each(function () {
                if ($(this).is(":checked")) {
                    checkedItems += $(this).data("jobcardid") + "-";
                }

                if (!$(this).is(":checked")) {
                    uncheckedItems += $(this).data("jobcardid") + "-";
                }

            });


            //console.log(checkedItems + "/" + uncheckedItems);
            //return;


            //Sent data
            var obj = {
                checkedItems: checkedItems,
                uncheckedItems: uncheckedItems
            };

            $.ajax({
                url: "/MainOperations/SubmitRepairedBusSubmit/",
                type: "post",
                data: JSON.stringify(obj),
                dataType: "json",
                contentType: 'application/json; charset=utf-8',
                success: function (response) {
                    //for (var i = 0; i < dates.length; i++) {
                    //    if (dates[i].dataset.dateval === date) {
                    //        if (response.countOfNew === 0) {
                    //            dates[i].lastChild.textContent = "";
                    //        } else {
                    //            dates[i].lastChild.textContent = response.countOfNew;
                    //        }
                    //        sentData.text(response.countOfSubmitted);
                    //    }
                    //}

                    if (response.statusChecked == "success" || response.statusUnchecked == "success") {
                        alert("Əməliyyat uğurla yerinə yetirildi");
                    }
                }
            });
        }
    });

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~  << End >>  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//




    /////// API Test

    $("#APITest #button").on("click", function () {

        //var id = $(this).data("jobcard");
        var tbody = $("#APITest table tbody");
        $.ajax({
            url: "http://10.1.20.114:1010/api/employees",
            type: "get",
            dataType: "json",
            success: function (response) {
                tbody.empty();
                for (var i = 0; i < response.length; i++) {
                    var tr = $("<tr></tr>");
                    var tdOrder = $("<td>" + (i + 1) + "</td>");
                    var tdName = $("<td>" + response[i].Name + "</td>");
                    var tdGender = $("<td>" + response[i].Gender + "</td>");
                    var tdScore = $("<td>" + response[i].Score + "</td>");
                    tr.append(tdOrder);
                    tr.append(tdName);
                    tr.append(tdGender);
                    tr.append(tdScore);
                    tbody.append(tr);
                }
            }
        });

        count++;
    });

});