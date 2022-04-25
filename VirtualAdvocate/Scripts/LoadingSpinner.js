var spinnerVisible = false;
function showProgress() {
    if (!spinnerVisible) {
        $("#overlay").show();
        $("div#spinner").fadeIn("fast");
        spinnerVisible = true;
    }
};
function hideProgress() {
    if (spinnerVisible) {
        $("#overlay").hide();
        var spinner = $("div#spinner");
        spinner.stop();
        spinner.fadeOut("fast");
        spinnerVisible = false;
    }
};
