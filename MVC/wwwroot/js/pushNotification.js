"use strict";
// Creating a connection to SignalR Hub
var connection = new signalR.HubConnectionBuilder().withUrl("/signalr-hub").build();

Object.defineProperty(WebSocket, 'OPEN', { value: 1, });

// Starting the connection with server
connection.start().then(function () {
    //console.log("connected");
}).catch(function (err) {
    return console.error(err.toString());
});

// Subscribing to the messages broadcasted by Hub every time when a new message is pushed to it
connection.on("BroadcastMessagePaymentSuccess", function (invoiceId, message) {
    toastr.info(message);
    RefreshBadgeBell();
});

connection.on("RefreshTheBell", function () {
    RefreshBadgeBell();
});

function RefreshBadgeBell() {
    $.ajax({
        type: "GET",
        url: "/Invoice/GetUnreadInvoice",
        success: function (data) {
            var total = data.unreadInvoice;
            $(".push_notif_payment").html(total);
            if (total === 0) {
                $(".push_notif_payment").html('');                
                $("#push_notif_text").html(multiLanguage["No new notification"]);
                $("#notificationLink").removeAttr("href");
            } else {
                $("#push_notif_text").html(" " + multiLanguage["new success payment"]);
                $("#badge_bell_dropdown").removeAttr("style");
            }
        },
        error: function (data) {
            return console.error(data.toString());
        }
    });
}