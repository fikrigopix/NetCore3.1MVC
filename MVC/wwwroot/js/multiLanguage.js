var multiLanguage = {}; // Global variable.
(function ($) {
    $.getJSON("/Resource/GetResources", function (data) {
        multiLanguage = data;
    });
})(jQuery);