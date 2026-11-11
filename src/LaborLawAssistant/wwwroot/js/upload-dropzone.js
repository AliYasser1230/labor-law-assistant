(function () {
    var zone = document.querySelector("[data-dropzone]");
    if (!zone) {
        return;
    }

    var input = zone.querySelector("input[type=file]");
    var fileName = zone.querySelector("[data-file-name]");

    function formatSize(bytes) {
        return bytes < 1024 * 1024
            ? Math.max(1, Math.round(bytes / 1024)) + " KB"
            : (bytes / 1024 / 1024).toFixed(1) + " MB";
    }

    function showSelectedFile() {
        var file = input.files[0];
        if (file) {
            fileName.textContent = file.name + " · " + formatSize(file.size);
            zone.classList.add("has-file");
        } else {
            fileName.textContent = "";
            zone.classList.remove("has-file");
        }

        if (window.jQuery && jQuery.validator) {
            jQuery(input).valid();
        }
    }

    input.addEventListener("change", showSelectedFile);

    ["dragenter", "dragover"].forEach(function (eventName) {
        zone.addEventListener(eventName, function (event) {
            event.preventDefault();
            zone.classList.add("is-dragover");
        });
    });

    ["dragleave", "drop"].forEach(function (eventName) {
        zone.addEventListener(eventName, function (event) {
            event.preventDefault();
            zone.classList.remove("is-dragover");
        });
    });

    zone.addEventListener("drop", function (event) {
        var dropped = event.dataTransfer.files;
        if (dropped.length === 0) {
            return;
        }

        var transfer = new DataTransfer();
        transfer.items.add(dropped[0]);
        input.files = transfer.files;
        showSelectedFile();
    });
})();
